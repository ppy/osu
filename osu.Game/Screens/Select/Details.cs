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
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Screens.Select
{
    public class Details : Container
    {
        private readonly SpriteText description;
        private readonly SpriteText source;
        private readonly FillFlowContainer<SpriteText> tags;

        private readonly DifficultyRow circleSize;
        private readonly DifficultyRow drainRate;
        private readonly DifficultyRow overallDifficulty;
        private readonly DifficultyRow approachRate;
        private readonly DifficultyRow stars;

        private readonly DetailsBar ratingsBar;
        private readonly SpriteText negativeRatings;
        private readonly SpriteText positiveRatings;
        private readonly FillFlowContainer<DetailsBar> ratingsGraph;

        private readonly FillFlowContainer<RetryAndFailBar> retryAndFailGraph;

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
                tags.Children = beatmap.Metadata.Tags?.Split(' ').Select(text => new SpriteText
                {
                    Text = text,
                    TextSize = 14,
                    Font = "Exo2.0-Medium",
                });

                circleSize.Value = beatmap.Difficulty.CircleSize;
                drainRate.Value = beatmap.Difficulty.DrainRate;
                overallDifficulty.Value = beatmap.Difficulty.OverallDifficulty;
                approachRate.Value = beatmap.Difficulty.ApproachRate;
                stars.Value = (float) beatmap.StarDifficulty;
            }
        }

        private List<int> ratings;
        public IEnumerable<int> Ratings
        {
            get
            {
                return ratings;
            }
            set
            {
                ratings = value.ToList();
                negativeRatings.Text = ratings.GetRange(0, 5).Sum().ToString();
                positiveRatings.Text = ratings.GetRange(5, 5).Sum().ToString();
                ratingsBar.Length = (float)ratings.GetRange(0, 5).Sum() / ratings.Sum();

                List<DetailsBar> ratingsGraphBars = ratingsGraph.Children.ToList();
                for (int i = 0; i < 10; i++)
                    if(ratingsGraphBars.Count > i)
                        ratingsGraphBars[i].Length = (float)ratings[i] / ratings.Max();
                    else
                        ratingsGraph.Add(new DetailsBar
                        {
                            RelativeSizeAxes = Axes.Both,
                            Width = 0.1f,
                            Length = (float)ratings[i] / ratings.Max(),
                            Direction = BarDirection.BottomToTop,
                            BackgroundColour = new Color4(0, 0, 0, 0),
                        });
            }
        }

        private List<int> retries = Enumerable.Repeat(0,100).ToList();
        public IEnumerable<int> Retries
        {
            get
            {
                return retries;
            }
            set
            {
                retries = value.ToList();
                calcRetryAndFailBarLength();
            }
        }

        private List<int> fails = Enumerable.Repeat(0,100).ToList();
        public IEnumerable<int> Fails
        {
            get
            {
                return fails;
            }
            set
            {
                fails = value.ToList();
                calcRetryAndFailBarLength();
            }
        }

        private void calcRetryAndFailBarLength()
        {
            List<RetryAndFailBar> retryAndFailGraphBars = retryAndFailGraph.Children.ToList();
            float maxValue = fails.Select((value, index) => value + retries[index]).Max();
            for (int i = 0; i < 100; i++)
                if (retryAndFailGraphBars.Count > i)
                {
                    retryAndFailGraphBars[i].FailLength = fails[i] / maxValue;
                    retryAndFailGraphBars[i].RetryLength = retries[i] / maxValue;
                }
                else
                    retryAndFailGraph.Add(new RetryAndFailBar
                    {
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.01f,
                        FailLength = fails[i] / maxValue,
                        RetryLength = retries[i] / maxValue,
                    });
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
                    Padding = new MarginPadding(10) { Top = 25 },
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
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.6f,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0,15),
                    Padding = new MarginPadding(10) { Top = 0 },
                    Children = new []
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
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
                                    Padding = new MarginPadding(15) { Top = 25 },
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
                                        overallDifficulty = new DifficultyRow
                                        {
                                            DifficultyName = "Accuracy",
                                            AutoSizeAxes = Axes.Y,
                                            RelativeSizeAxes = Axes.X,
                                        },
                                        approachRate = new DifficultyRow
                                        {
                                            DifficultyName = "Approach Rate",
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
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
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
                                    Padding = new MarginPadding(15) { Top = 25, Bottom = 0 },
                                    Children = new Drawable[]
                                    {
                                        new SpriteText
                                        {
                                            Text = "User Rating",
                                            TextSize = 14,
                                            Font = @"Exo2.0-Medium",
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                        },
                                        ratingsBar = new DetailsBar
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 5,
                                            Length = 0,
                                        },
                                        new Container
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            AutoSizeAxes = Axes.Y,
                                            Children = new[]
                                            {
                                                negativeRatings = new SpriteText
                                                {
                                                    TextSize = 14,
                                                    Font = @"Exo2.0-Medium",
                                                    Text = "0",
                                                },
                                                positiveRatings = new SpriteText
                                                {
                                                    TextSize = 14,
                                                    Font = @"Exo2.0-Medium",
                                                    Text = "0",
                                                    Anchor = Anchor.TopRight,
                                                    Origin = Anchor.TopRight,
                                                },
                                            },
                                        },
                                        new SpriteText
                                        {
                                            Text = "Rating Spread",
                                            TextSize = 14,
                                            Font = @"Exo2.0-Medium",
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                        },
                                        ratingsGraph = new FillFlowContainer<DetailsBar>
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Direction = FillDirection.Horizontal,
                                            Height = 50,
                                        },
                                    },
                                },
                            },
                        },
                    },
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.BottomLeft,
                    Origin = Anchor.BottomLeft,
                    Direction = FillDirection.Vertical,
                    Padding = new MarginPadding { Left = 10, Right = 10 },
                    Children = new Drawable[]
                    {
						retryAndFailGraph = new FillFlowContainer<RetryAndFailBar>
						{
							RelativeSizeAxes = Axes.X,
							Direction = FillDirection.Horizontal,
							Height = 50,
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                        },
                        new SpriteText
                        {
                            Text = "Points of Failure",
                            TextSize = 14,
                            Font = @"Exo2.0-Medium",
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
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

            stars.BarColour = colour.Yellow;

            ratingsBar.BackgroundColour = colour.Green;
            ratingsBar.BarColour = colour.YellowDark;
            ratingsGraph.Colour = colour.BlueDark;
        }

        private class DifficultyRow : Container
        {
            private readonly SpriteText name;
            private readonly DetailsBar bar;
            private readonly SpriteText valueText;

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
                    bar.Length = value/maxValue;
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
                    bar.Length = Value/value;
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

        private class RetryAndFailBar : Container<DetailsBar>
        {
            private readonly DetailsBar retryBar;
            private readonly DetailsBar failBar;

            public float RetryLength
            {
                get
                {
                    return retryBar.Length;
                }
                set
                {
                    retryBar.Length = value + FailLength;
                }
            }

            public float FailLength
            {
                get
                {
                    return failBar.Length;
                }
                set
                {
                    failBar.Length = value;
                }
            }

            public RetryAndFailBar()
            {
                Children = new[]
                {
                    retryBar = new DetailsBar
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = BarDirection.BottomToTop,
                        Length = 0,
                        BackgroundColour = new Color4(0,0,0,0),
                    },
                    failBar = new DetailsBar
                    {
                        RelativeSizeAxes = Axes.Both,
                        Direction = BarDirection.BottomToTop,
                        Length = 0,
                        BackgroundColour = new Color4(0,0,0,0),
                    },
                };
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colour)
            {
                retryBar.Colour = colour.Yellow;
                failBar.Colour = colour.YellowDarker;
            }
        }
    }
}
