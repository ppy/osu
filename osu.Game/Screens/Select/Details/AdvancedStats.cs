// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using System;
using osu.Game.Beatmaps;

namespace osu.Game.Screens.Select.Details
{
    public class AdvancedStats : Container
    {
        private readonly StatRow firstValue, hpDrain, accuracy, approachRate, starDifficulty;

        private BeatmapInfo beatmap;
        public BeatmapInfo Beatmap
        {
            get { return beatmap; }
            set
            {
                if (value == beatmap) return;
                beatmap = value;

                //mania specific
                if ((Beatmap?.Ruleset?.ID ?? 0) == 3)
                {
                    firstValue.Title = "Key Amount";
                    firstValue.Value = (int)Math.Round(Beatmap.Difficulty.CircleSize);
                }
                else
                {
                    firstValue.Title = "Circle Size";
                    firstValue.Value = Beatmap.Difficulty.CircleSize;
                }

                hpDrain.Value = beatmap.Difficulty.DrainRate;
                accuracy.Value = beatmap.Difficulty.OverallDifficulty;
                approachRate.Value = beatmap.Difficulty.ApproachRate;
                starDifficulty.Value = (float)beatmap.StarDifficulty;
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
                    firstValue = new StatRow(), //circle size/key amount
                    hpDrain = new StatRow { Title = "HP Drain" },
                    accuracy = new StatRow { Title = "Accuracy" },
                    approachRate = new StatRow { Title = "Approach Rate" },
                    starDifficulty = new StatRow(10, true) { Title = "Star Difficulty" },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            starDifficulty.AccentColour = colours.Yellow;
        }

        private class StatRow : Container, IHasAccentColour
        {
            private const float value_width = 25;
            private const float name_width = 70;

            private readonly float maxValue;
            private readonly bool forceDecimalPlaces;
            private readonly OsuSpriteText name, value;
            private readonly Bar bar;

            public string Title
            {
                get { return name.Text; }
                set { name.Text = value; }
            }

            private float difficultyValue;
            public float Value
            {
                get { return difficultyValue; }
                set
                {
                    difficultyValue = value;
                    bar.Length = value / maxValue;
                    this.value.Text = value.ToString(forceDecimalPlaces ? "#.00" : "0.##");
                }
            }

            public Color4 AccentColour
            {
                get { return bar.AccentColour; }
                set { bar.AccentColour = value; }
            }

            public StatRow(float maxValue = 10, bool forceDecimalPlaces = false)
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
                        Child = this.name = new OsuSpriteText
                        {
                            TextSize = 13,
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
                            TextSize = 13,
                        },
                    },
                };
            }
        }
    }
}
