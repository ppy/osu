// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Extensions;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select
{
    public partial class LengthAndBPMStatisticPill : CompositeDrawable
    {
        private PillStatistic lengthStatistic = null!;
        private PillStatistic bpmStatistic = null!;

        private ModSettingChangeTracker? modSettingChangeTracker;

        [Resolved]
        private IBindable<IBeatmapInfo?> beatmapInfo { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private Bindable<WorkingBeatmap> workingBeatmap { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        public LengthAndBPMStatisticPill()
        {
            AutoSizeAxes = Axes.X;
            Height = 20;
            CornerRadius = 10;
            Masking = true;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4
                },
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.X,
                    RelativeSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10),
                    Padding = new MarginPadding { Horizontal = 20 },
                    Children = new Drawable[]
                    {
                        lengthStatistic = new PillStatistic(new BeatmapStatistic { Name = "Length" })
                        {
                            Value = "-"
                        },
                        bpmStatistic = new PillStatistic(new BeatmapStatistic { Name = BeatmapsetsStrings.ShowStatsBpm })
                        {
                            Value = "-"
                        },
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmapInfo.BindValueChanged(_ => updateStatistics());

            mods.BindValueChanged(m =>
            {
                // only valid in song select context
                if (beatmapInfo.Value is not BeatmapInfo) return;

                modSettingChangeTracker?.Dispose();

                updateStatistics();

                modSettingChangeTracker = new ModSettingChangeTracker(m.NewValue);
                modSettingChangeTracker.SettingChanged += _ => updateStatistics();
            }, true);
        }

        private void updateStatistics()
        {
            switch (beatmapInfo.Value)
            {
                case BeatmapInfo:
                    // TODO: consider mods which apply variable rates.
                    double rate = 1;
                    foreach (var mod in mods.Value.OfType<IApplicableToRate>())
                        rate = mod.ApplyToRate(0, rate);

                    var beatmap = workingBeatmap.Value.Beatmap;

                    int bpmMax = FormatUtils.RoundBPM(beatmap.ControlPointInfo.BPMMaximum, rate);
                    int bpmMin = FormatUtils.RoundBPM(beatmap.ControlPointInfo.BPMMinimum, rate);
                    int mostCommonBPM = FormatUtils.RoundBPM(60000 / beatmap.GetMostCommonBeatLength(), rate);

                    string labelText = bpmMin == bpmMax
                        ? $"{bpmMin}"
                        : $"{bpmMin}-{bpmMax} ({mostCommonBPM})";

                    bpmStatistic.Value = labelText;

                    double drainLength = Math.Round(beatmap.CalculateDrainLength() / rate);
                    double hitLength = Math.Round(beatmap.BeatmapInfo.Length / rate);

                    lengthStatistic.Value = hitLength.ToFormattedDuration();
                    lengthStatistic.TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(drainLength.ToFormattedDuration());

                    bpmStatistic.ValueColour = getColourByRate(rate);
                    lengthStatistic.ValueColour = getColourByRate(rate);
                    break;

                case APIBeatmap apiBeatmap:
                    lengthStatistic.Value = apiBeatmap.Length.ToFormattedDuration();
                    lengthStatistic.TooltipText = BeatmapsetsStrings.ShowStatsTotalLength(apiBeatmap.HitLength.ToFormattedDuration());
                    bpmStatistic.Value = apiBeatmap.BPM.ToLocalisableString(@"0.##");
                    break;
            }
        }

        private Color4 getColourByRate(double rate)
        {
            switch (rate)
            {
                case < 1:
                    return colours.ForModType(ModType.DifficultyReduction);

                case > 1:
                    return colours.ForModType(ModType.DifficultyIncrease);

                default:
                    return colourProvider.Content2;
            }
        }

        public partial class PillStatistic : FillFlowContainer, IHasTooltip
        {
            private readonly BeatmapStatistic statistic;
            private OsuSpriteText valueText = null!;

            public LocalisableString Value
            {
                set => Schedule(() => valueText.Text = LocalisableString.IsNullOrEmpty(value) ? "-" : value);
            }

            public Color4 ValueColour
            {
                get => valueText.Colour;
                set => Schedule(() => valueText.Colour = value);
            }

            public PillStatistic(BeatmapStatistic statistic)
            {
                this.statistic = statistic;

                Anchor = Anchor.CentreLeft;
                Origin = Anchor.CentreLeft;
                AutoSizeAxes = Axes.Both;
                Direction = FillDirection.Horizontal;
                Spacing = new Vector2(5);
            }

            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider colourProvider)
            {
                Children = new Drawable[]
                {
                    new OsuSpriteText
                    {
                        Text = statistic.Name,
                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 14),
                    },
                    valueText = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 14),
                        Text = statistic.Content,
                        Colour = colourProvider.Content2,
                    }
                };
            }

            public LocalisableString TooltipText { get; set; }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            modSettingChangeTracker?.Dispose();
        }
    }
}
