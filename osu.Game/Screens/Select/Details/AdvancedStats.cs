// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Framework.Bindables;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Game.Rulesets.Mods;
using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using osu.Framework.Extensions;
using osu.Framework.Localisation;
using osu.Framework.Threading;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Overlays.Mods;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Utils;

namespace osu.Game.Screens.Select.Details
{
    public partial class AdvancedStats : Container
    {
        private readonly int columns;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; }

        protected FillFlowContainer Flow { get; private set; }
        private readonly StatisticRow starDifficulty;

        private IBeatmapInfo beatmapInfo;

        public IBeatmapInfo BeatmapInfo
        {
            get => beatmapInfo;
            set
            {
                if (value == beatmapInfo) return;

                beatmapInfo = value;

                updateStatistics();
            }
        }

        /// <summary>
        /// Ruleset to be used for certain elements of display.
        /// When set, this will override the set <see cref="Beatmap"/>'s own ruleset.
        /// </summary>
        /// <remarks>
        /// No checks are done as to whether the ruleset specified is valid for the currently <see cref="BeatmapInfo"/>.
        /// </remarks>
        public Bindable<RulesetInfo> Ruleset { get; } = new Bindable<RulesetInfo>();

        /// <summary>
        /// Mods to be used for certain elements of display.
        /// </summary>
        /// <remarks>
        /// No checks are done as to whether the mods specified are valid for the current <see cref="Ruleset"/>.
        /// </remarks>
        public Bindable<IReadOnlyList<Mod>> Mods { get; } = new Bindable<IReadOnlyList<Mod>>(Array.Empty<Mod>());

        public AdvancedStats(int columns = 1)
        {
            this.columns = columns;

            switch (columns)
            {
                case 1:
                    Child = Flow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Children = new[]
                        {
                            starDifficulty = new StatisticRow(forceDecimalPlaces: true)
                            {
                                Title = BeatmapsetsStrings.ShowStatsStars,
                                MaxValue = 10,
                            },
                        },
                    };
                    break;

                case 2:
                    Child = Flow = new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Full,
                        Children = new[]
                        {
                            starDifficulty = new StatisticRow(forceDecimalPlaces: true)
                            {
                                MaxValue = 10,
                                Title = BeatmapsetsStrings.ShowStatsStars,
                                Width = 0.5f,
                                Padding = new MarginPadding { Horizontal = 5, Vertical = 2.5f },
                            },
                        },
                    };
                    break;
            }

            Debug.Assert(Flow != null);
            Flow.SetLayoutPosition(starDifficulty, float.MaxValue);
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            starDifficulty.AccentColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Ruleset.BindValueChanged(_ => updateStatistics());
            Mods.BindValueChanged(modsChanged, true);
        }

        private ModSettingChangeTracker modSettingChangeTracker;
        private ScheduledDelegate debouncedStatisticsUpdate;

        private void modsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            modSettingChangeTracker?.Dispose();

            modSettingChangeTracker = new ModSettingChangeTracker(mods.NewValue);
            modSettingChangeTracker.SettingChanged += _ =>
            {
                debouncedStatisticsUpdate?.Cancel();
                debouncedStatisticsUpdate = Scheduler.AddDelayed(updateStatistics, 100);
            };

            updateStatistics();
        }

        private void updateStatistics()
        {
            if (BeatmapInfo != null && Ruleset.Value != null)
            {
                var displayAttributes = Ruleset.Value.CreateInstance().GetBeatmapAttributesForDisplay(BeatmapInfo, Mods.Value).ToList();

                // if there are not enough attribute displays, make more
                // the subtraction of 1 is to exclude the star rating row which is always present (and always last)
                for (int i = Flow.Count - 1; i < displayAttributes.Count; i++)
                {
                    Flow.Add(new StatisticRow
                    {
                        Width = columns == 1 ? 1 : 0.5f,
                        Padding = columns == 1 ? new MarginPadding() : new MarginPadding { Horizontal = 5, Vertical = 2.5f },
                    });
                }

                // populate all attribute displays that need to be visible...
                for (int i = 0; i < displayAttributes.Count; i++)
                {
                    var attribute = displayAttributes[i];
                    var row = (StatisticRow)Flow.Where(r => r != starDifficulty).ElementAt(i);
                    row.SetAttribute(attribute);
                }

                // and hide any extra ones
                foreach (var row in Flow.Where(r => r != starDifficulty).Skip(displayAttributes.Count))
                    ((StatisticRow)row).SetAttribute(null);
            }

            updateStarDifficulty();
        }

        private CancellationTokenSource starDifficultyCancellationSource;

        /// <summary>
        /// Updates the displayed star difficulty statistics with the values provided by the currently-selected beatmap, ruleset, and selected mods.
        /// </summary>
        /// <remarks>
        /// This is scheduled to avoid scenarios wherein a ruleset changes first before selected mods do,
        /// potentially resulting in failure during difficulty calculation due to incomplete bindable state updates.
        /// </remarks>
        private void updateStarDifficulty() => Scheduler.AddOnce(() =>
        {
            starDifficultyCancellationSource?.Cancel();

            if (BeatmapInfo == null)
                return;

            starDifficultyCancellationSource = new CancellationTokenSource();

            var normalStarDifficultyTask = difficultyCache.GetDifficultyAsync(BeatmapInfo, Ruleset.Value, null, starDifficultyCancellationSource.Token);
            var moddedStarDifficultyTask = difficultyCache.GetDifficultyAsync(BeatmapInfo, Ruleset.Value, Mods.Value, starDifficultyCancellationSource.Token);

            Task.WhenAll(normalStarDifficultyTask, moddedStarDifficultyTask).ContinueWith(_ => Schedule(() =>
            {
                var normalDifficulty = normalStarDifficultyTask.GetResultSafely();
                var moddedDifficulty = moddedStarDifficultyTask.GetResultSafely();

                if (normalDifficulty == null || moddedDifficulty == null)
                    return;

                starDifficulty.Value = ((float)normalDifficulty.Value.Stars.FloorToDecimalDigits(2), (float)moddedDifficulty.Value.Stars.FloorToDecimalDigits(2));
            }), starDifficultyCancellationSource.Token, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Current);
        });

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            modSettingChangeTracker?.Dispose();
            starDifficultyCancellationSource?.Cancel();
        }

        public partial class StatisticRow : Container, IHasAccentColour, IHasCustomTooltip<RulesetBeatmapAttribute>
        {
            private const float value_width = 25;
            private const float name_width = 70;

            private readonly bool forceDecimalPlaces;
            private readonly OsuSpriteText name, valueText;
            private readonly Bar bar;
            public readonly Bar ModBar;

            [Resolved]
            private OsuColour colours { get; set; }

            public LocalisableString Title
            {
                get => name.Text;
                set => name.Text = value;
            }

            public float MaxValue { get; set; }

            private (float baseValue, float? adjustedValue)? value;

            public (float baseValue, float? adjustedValue) Value
            {
                get => value ?? (0, null);
                set
                {
                    if (value == this.value)
                        return;

                    this.value = value;

                    bar.Length = value.baseValue / MaxValue;

                    valueText.Text = (value.adjustedValue ?? value.baseValue).ToString(forceDecimalPlaces ? "0.00" : "0.##");
                    ModBar.Length = (value.adjustedValue ?? 0) / MaxValue;

                    if (Precision.AlmostEquals(value.baseValue, value.adjustedValue ?? value.baseValue, 0.05f))
                        ModBar.AccentColour = valueText.Colour = Color4.White;
                    else if (value.adjustedValue > value.baseValue)
                        ModBar.AccentColour = valueText.Colour = colours.Red;
                    else if (value.adjustedValue < value.baseValue)
                        ModBar.AccentColour = valueText.Colour = colours.BlueDark;
                }
            }

            public Color4 AccentColour
            {
                get => bar.AccentColour;
                set => bar.AccentColour = value;
            }

            public StatisticRow(bool forceDecimalPlaces = false)
            {
                this.forceDecimalPlaces = forceDecimalPlaces;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding { Vertical = 2.5f };

                Children = new Drawable[]
                {
                    new Container
                    {
                        Width = name_width,
                        AutoSizeAxes = Axes.Y,
                        // osu-web uses 1.25 line-height, which at 12px font size makes the element 14px tall - this compentates that difference
                        Padding = new MarginPadding { Vertical = 1 },
                        Child = name = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 12)
                        },
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Left = name_width + 10, Right = value_width + 10 },
                        Children = new Drawable[]
                        {
                            new Container
                            {
                                Origin = Anchor.CentreLeft,
                                Anchor = Anchor.CentreLeft,
                                RelativeSizeAxes = Axes.X,
                                Height = 5,

                                CornerRadius = 2,
                                Masking = true,
                                Children = new Drawable[]
                                {
                                    bar = new Bar
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        BackgroundColour = Color4.White.Opacity(0.5f),
                                    },
                                    ModBar = new Bar
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0.5f,
                                    },
                                }
                            },
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Width = value_width,
                        RelativeSizeAxes = Axes.Y,
                        Child = valueText = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 12)
                        },
                    },
                };
            }

            public void SetAttribute([CanBeNull] RulesetBeatmapAttribute attribute)
            {
                if (attribute != null)
                {
                    Title = attribute.Label;
                    MaxValue = attribute.MaxValue;
                    Value = (attribute.OriginalValue, attribute.AdjustedValue);
                    Alpha = 1;
                }
                else
                    Alpha = 0;

                TooltipContent = attribute;
            }

            public ITooltip<RulesetBeatmapAttribute> GetCustomTooltip() => new BeatmapAttributeTooltip();

            [CanBeNull]
            public RulesetBeatmapAttribute TooltipContent { get; set; }
        }
    }
}
