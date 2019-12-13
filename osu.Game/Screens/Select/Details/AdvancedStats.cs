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
using System;
using osu.Game.Beatmaps;
using osu.Framework.Bindables;
using System.Collections.Generic;
using osu.Game.Rulesets.Mods;
using System.Linq;

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
            mods.ValueChanged += _ => updateStatistics();
        }

        private void updateStatistics()
        {
            BeatmapInfo processed = Beatmap?.Clone();

            if (processed != null && mods.Value.Any(m => m is IApplicableToDifficulty))
            {
                processed.BaseDifficulty = processed.BaseDifficulty.Clone();

                foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                    mod.ApplyToDifficulty(processed.BaseDifficulty);
            }

            BeatmapDifficulty baseDifficulty = Beatmap?.BaseDifficulty;
            BeatmapDifficulty moddedDifficulty = processed?.BaseDifficulty;

            //mania specific
            if ((processed?.Ruleset?.ID ?? 0) == 3)
            {
                firstValue.Title = "Key Amount";
                firstValue.BaseValue = (int)MathF.Round(baseDifficulty?.CircleSize ?? 0);
                firstValue.ModdedValue = (int)MathF.Round(moddedDifficulty?.CircleSize ?? 0);
            }
            else
            {
                firstValue.Title = "Circle Size";
                firstValue.BaseValue = baseDifficulty?.CircleSize ?? 0;
                firstValue.ModdedValue = moddedDifficulty?.CircleSize ?? 0;
            }

            hpDrain.BaseValue = baseDifficulty?.DrainRate ?? 0;
            accuracy.BaseValue = baseDifficulty?.OverallDifficulty ?? 0;
            approachRate.BaseValue = baseDifficulty?.ApproachRate ?? 0;
            starDifficulty.BaseValue = (float)(processed?.StarDifficulty ?? 0);

            hpDrain.ModdedValue = moddedDifficulty?.DrainRate ?? 0;
            accuracy.ModdedValue = moddedDifficulty?.OverallDifficulty ?? 0;
            approachRate.ModdedValue = moddedDifficulty?.ApproachRate ?? 0;
        }

        private class StatisticRow : Container, IHasAccentColour
        {
            private const float value_width = 25;
            private const float name_width = 70;

            private readonly float maxValue;
            private readonly bool forceDecimalPlaces;
            private readonly OsuSpriteText name, value;
            private readonly Bar bar, modBar;

            [Resolved]
            private OsuColour colours { get; set; }

            public string Title
            {
                get => name.Text;
                set => name.Text = value;
            }

            private float baseValue;

            private float moddedValue;

            public float BaseValue
            {
                get => baseValue;
                set
                {
                    baseValue = value;
                    bar.Length = value / maxValue;
                    this.value.Text = value.ToString(forceDecimalPlaces ? "0.00" : "0.##");
                }
            }

            public float ModdedValue
            {
                get => moddedValue;
                set
                {
                    moddedValue = value;
                    modBar.Length = value / maxValue;
                    this.value.Text = value.ToString(forceDecimalPlaces ? "0.00" : "0.##");

                    if (moddedValue > baseValue)
                        modBar.AccentColour = this.value.Colour = colours.Red;
                    else if (moddedValue < baseValue)
                        modBar.AccentColour = this.value.Colour = colours.BlueDark;
                    else
                        modBar.AccentColour = this.value.Colour = Color4.White;
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
                        Child = value = new OsuSpriteText
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
