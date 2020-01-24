// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Beatmaps;
using osu.Framework.Bindables;
using System.Collections.Generic;
using osu.Game.Rulesets.Mods;
using System.Linq;
using osu.Framework.Threading;
using osu.Game.Configuration;
using osu.Game.Overlays.Settings;

namespace osu.Game.Screens.Select.Details
{
    public class AdvancedStats : Container
    {
        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; }

        private readonly StatisticRow firstValue, hpDrain, accuracy, approachRate, starDifficulty;

        private BeatmapInfo beatmap;

        public BeatmapInfo Beatmap
        {
            get => beatmap;
            set
            {
                if (value == beatmap) return;

                beatmap = value;

                updateStatistics();
            }
        }

        public AdvancedStats()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(4f),
                Children = new[]
                {
                    firstValue = new StatisticRow(), //circle size/key amount
                    hpDrain = new StatisticRow { Title = "HP Drain" },
                    accuracy = new StatisticRow { Title = "Accuracy" },
                    approachRate = new StatisticRow { Title = "Approach Rate" },
                    starDifficulty = new StatisticRow(10, true) { Title = "Star Difficulty" },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            starDifficulty.AccentColour = colours.Yellow;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            mods.BindValueChanged(modsChanged, true);
        }

        private readonly List<ISettingsItem> references = new List<ISettingsItem>();

        private void modsChanged(ValueChangedEvent<IReadOnlyList<Mod>> mods)
        {
            // TODO: find a more permanent solution for this if/when it is needed in other components.
            // this is generating drawables for the only purpose of storing bindable references.
            foreach (var r in references)
                r.Dispose();

            references.Clear();

            ScheduledDelegate debounce = null;

            foreach (var mod in mods.NewValue.OfType<IApplicableToDifficulty>())
            {
                foreach (var setting in mod.CreateSettingsControls().OfType<ISettingsItem>())
                {
                    setting.SettingChanged += () =>
                    {
                        debounce?.Cancel();
                        debounce = Scheduler.AddDelayed(updateStatistics, 100);
                    };

                    references.Add(setting);
                }
            }

            updateStatistics();
        }

        private void updateStatistics()
        {
            BeatmapDifficulty baseDifficulty = Beatmap?.BaseDifficulty;
            BeatmapDifficulty adjustedDifficulty = null;

            if (baseDifficulty != null && mods.Value.Any(m => m is IApplicableToDifficulty))
            {
                adjustedDifficulty = baseDifficulty.Clone();

                foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                    mod.ApplyToDifficulty(adjustedDifficulty);
            }

            // Account for mania differences
            firstValue.Title = (Beatmap?.Ruleset?.ID ?? 0) == 3 ? "Key Amount" : "Circle Size";
            firstValue.Value = (baseDifficulty?.CircleSize ?? 0, adjustedDifficulty?.CircleSize);

            starDifficulty.Value = ((float)(Beatmap?.StarDifficulty ?? 0), null);

            hpDrain.Value = (baseDifficulty?.DrainRate ?? 0, adjustedDifficulty?.DrainRate);
            accuracy.Value = (baseDifficulty?.OverallDifficulty ?? 0, adjustedDifficulty?.OverallDifficulty);
            approachRate.Value = (baseDifficulty?.ApproachRate ?? 0, adjustedDifficulty?.ApproachRate);
        }

        private class StatisticRow : Container, IHasAccentColour
        {
            private const float value_width = 25;
            private const float name_width = 70;

            private readonly float maxValue;
            private readonly bool forceDecimalPlaces;
            private readonly OsuSpriteText name, valueText;
            private readonly Bar bar, modBar;

            [Resolved]
            private OsuColour colours { get; set; }

            public string Title
            {
                get => name.Text;
                set => name.Text = value;
            }

            private (float baseValue, float? adjustedValue) value;

            public (float baseValue, float? adjustedValue) Value
            {
                get => value;
                set
                {
                    if (value == this.value)
                        return;

                    this.value = value;

                    bar.Length = value.baseValue / maxValue;

                    valueText.Text = (value.adjustedValue ?? value.baseValue).ToString(forceDecimalPlaces ? "0.00" : "0.##");
                    modBar.Length = (value.adjustedValue ?? 0) / maxValue;

                    if (value.adjustedValue > value.baseValue)
                        modBar.AccentColour = valueText.Colour = colours.Red;
                    else if (value.adjustedValue < value.baseValue)
                        modBar.AccentColour = valueText.Colour = colours.BlueDark;
                    else
                        modBar.AccentColour = valueText.Colour = Color4.White;
                }
            }

            public Color4 AccentColour
            {
                get => bar.AccentColour;
                set => bar.AccentColour = value;
            }

            public StatisticRow(float maxValue = 10, bool forceDecimalPlaces = false)
            {
                this.maxValue = maxValue;
                this.forceDecimalPlaces = forceDecimalPlaces;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                Children = new Drawable[]
                {
                    new Container
                    {
                        Width = name_width,
                        AutoSizeAxes = Axes.Y,
                        Child = name = new OsuSpriteText
                        {
                            Font = OsuFont.GetFont(size: 13)
                        },
                    },
                    bar = new Bar
                    {
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        Height = 5,
                        BackgroundColour = Color4.White.Opacity(0.5f),
                        Padding = new MarginPadding { Left = name_width + 10, Right = value_width + 10 },
                    },
                    modBar = new Bar
                    {
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.X,
                        Alpha = 0.5f,
                        Height = 5,
                        Padding = new MarginPadding { Left = name_width + 10, Right = value_width + 10 },
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
                            Font = OsuFont.GetFont(size: 13)
                        },
                    },
                };
            }
        }
    }
}
