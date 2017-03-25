// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Graphics;
using System.Linq;

namespace osu.Game.Screens.Select
{
    public class Details : Container
    {
        private SpriteText description;
        private SpriteText source;
        private FillFlowContainer<SpriteText> tags;

        private DifficultyRow circleSize;
        private DifficultyRow drainRate;
        private DifficultyRow approachRate;
        private DifficultyRow overallDifficulty;
        private DifficultyRow stars;

        private BeatmapInfo beatmap;
        public BeatmapInfo Beatmap
        {
            get
            {
                return beatmap;
            }

            set
            {
                if (beatmap == value) return;
                beatmap = value;
                description.Text = beatmap.Version;
                source.Text = beatmap.Metadata.Source;
                tags.Children = beatmap.Metadata.Tags.Split(' ').Select(text => new SpriteText
                {
                    Text = text,
                    TextSize = 14,
                    Font = "Exo2.0-Medium",
                });

                circleSize.Value = beatmap.Difficulty.CircleSize;
                drainRate.Value = beatmap.Difficulty.DrainRate;
                approachRate.Value = beatmap.Difficulty.ApproachRate;
                overallDifficulty.Value = beatmap.Difficulty.OverallDifficulty;
                stars.Value = (float) beatmap.StarDifficulty;
            }
        }

        public Details()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                new FillFlowContainer()
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.4f,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding(5),
                    Children = new Drawable[]
                    {
                        new SpriteText
                        {
                            Text = "Description",
                            TextSize = 14,
                            Font = @"Exo2.0-Bold",
                        },
                        description = new SpriteText
                        {
                            TextSize = 14,
                            Font = @"Exo2.0-Medium",
                            Direction = FillDirection.Full,
                        },
                        new SpriteText
                        {
                            Text = "Source",
                            TextSize = 14,
                            Font = @"Exo2.0-Bold",
                            Margin = new MarginPadding { Top = 20 },
                        },
                        source = new SpriteText
                        {
                            TextSize = 14,
                            Font = @"Exo2.0-Medium",
                            Direction = FillDirection.Full,
                        },
                        new SpriteText
                        {
                            Text = "Tags",
                            TextSize = 14,
                            Font = @"Exo2.0-Bold",
                            Margin = new MarginPadding { Top = 20 },
                        },
                        tags = new FillFlowContainer<SpriteText>
                        {
                            RelativeSizeAxes = Axes.X,
                            Spacing = new Vector2(3,0),
                        },
                    },
                },
                new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.6f,
                    Padding = new MarginPadding(5) { Top = 0 },
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0.5f,
                        },
                        new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Vertical,
                            Spacing = new Vector2(0,10),
                            Padding = new MarginPadding(7),
                            Children = new []
                            {
                                circleSize = new DifficultyRow
                                {
                                    DifficultyName = "Circle Size",
                                    AutoSizeAxes = Axes.Y,
                                    RelativeSizeAxes = Axes.X,
                                    MaxValue = 7,
                                },
                                drainRate = new DifficultyRow
                                {
                                    DifficultyName = "HP Drain",
                                    AutoSizeAxes = Axes.Y,
                                    RelativeSizeAxes = Axes.X,
                                },
                                approachRate = new DifficultyRow
                                {
                                    DifficultyName = "Accuracy",
                                    AutoSizeAxes = Axes.Y,
                                    RelativeSizeAxes = Axes.X,
                                },
                                overallDifficulty = new DifficultyRow
                                {
                                    DifficultyName = "Limit Break",
                                    AutoSizeAxes = Axes.Y,
                                    RelativeSizeAxes = Axes.X,
                                },
                                stars = new DifficultyRow
                                {
                                    DifficultyName = "Star Difficulty",
                                    AutoSizeAxes = Axes.Y,
                                    RelativeSizeAxes = Axes.X,
                                },
                            },
                        },
                    },
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            description.Colour = colour.GrayB;
            source.Colour = colour.GrayB;
            tags.Colour = colour.YellowLight;
            stars.BarColour = colour.YellowLight;
        }

        private class DifficultyRow : Container
        {
            private SpriteText name;
            private DetailsBar bar;
            private SpriteText valueText;

            private float difficultyValue;
            public float Value
            {
                get
                {
                    return difficultyValue;
                }
                set
                {
                    difficultyValue = value;
                    bar.Value = value/maxValue;
                    valueText.Text = value.ToString();
                }
            }

            private float maxValue = 10;
            public float MaxValue
            {
                get
                {
                    return maxValue;
                }
                set
                {
                    maxValue = value;
                    bar.Value = Value/value;
                }
            }

            public string DifficultyName
            {
                get
                {
                    return name.Text;
                }
                set
                {
                    name.Text = value;
                }
            }

            public SRGBColour BarColour
            {
                get
                {
                    return bar.BarColour;
                }
                set
                {
                    bar.BarColour = value;
                }
            }

            public DifficultyRow()
            {
                Children = new Drawable[]
                {
                    name = new SpriteText
                    {
                        TextSize = 14,
                        Font = @"Exo2.0-Medium",
                    },
                    bar = new DetailsBar
                    {
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(1, 0.35f),
                        Padding = new MarginPadding { Left = 100, Right = 25 },
                    },
                    valueText = new SpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        TextSize = 14,
                        Font = @"Exo2.0-Medium",
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colour)
            {
                name.Colour = colour.GrayB;
                bar.BackgroundColour = colour.Gray7;
                valueText.Colour = colour.GrayB;
            }
        }
    }
}
