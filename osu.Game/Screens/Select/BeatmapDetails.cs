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
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace osu.Game.Screens.Select
{
    public class BeatmapDetails : Container
    {
        private readonly MetadataSegment description;
        private readonly MetadataSegment source;
        private readonly MetadataSegment tags;

        private readonly DifficultyRow circleSize;
        private readonly DifficultyRow drainRate;
        private readonly DifficultyRow overallDifficulty;
        private readonly DifficultyRow approachRate;
        private readonly DifficultyRow stars;

        private readonly Bar ratingsBar;
        private readonly OsuSpriteText negativeRatings;
        private readonly OsuSpriteText positiveRatings;
        private readonly BarGraph ratingsGraph;

        private readonly BarGraph retryGraph;
        private readonly BarGraph failGraph;

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

                description.ContentText = beatmap.Version;
                source.ContentText = beatmap.Metadata.Source;
                tags.ContentText = beatmap.Metadata.Tags;

                circleSize.Value = beatmap.Difficulty.CircleSize;
                drainRate.Value = beatmap.Difficulty.DrainRate;
                overallDifficulty.Value = beatmap.Difficulty.OverallDifficulty;
                approachRate.Value = beatmap.Difficulty.ApproachRate;
                stars.Value = (float)beatmap.StarDifficulty;
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

                ratingsGraph.Values = ratings.Select(rating => (float)rating);
            }
        }

        private List<int> retries;
        public IEnumerable<int> Retries
        {
            get
            {
                return retries;
            }
            set
            {
                retries = value.ToList();
                calcRetryAndFailGraph();
            }
        }

        private List<int> fails;
        public IEnumerable<int> Fails
        {
            get
            {
                return fails;
            }
            set
            {
                fails = value.ToList();
                calcRetryAndFailGraph();
            }
        }

        private void calcRetryAndFailGraph()
        {
            failGraph.Values = fails.Select(fail => (float)fail);
            retryGraph.Values = retries?.Select((retry, index) => (float)retry + fails?[index] ?? 0) ?? new List<float>();
        }

        public BeatmapDetails()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black,
                    Alpha = 0.5f,
                },
                new FillFlowContainer<MetadataSegment>()
                {
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.4f,
                    Direction = FillDirection.Vertical,
                    LayoutDuration = 1,
                    LayoutEasing = EasingTypes.OutQuint,
                    Padding = new MarginPadding(10) { Top = 25 },
                    Children = new []
                    {
                        description = new MetadataSegment
                        {
                            HeaderText = "Description",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        source = new MetadataSegment
                        {
                            HeaderText = "Source",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                        tags = new MetadataSegment
                        {
                            HeaderText = "Tags",
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        }
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
                    Children = new Drawable[]
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
                                        new OsuSpriteText
                                        {
                                            Text = "User Rating",
                                            Font = @"Exo2.0-Medium",
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                        },
                                        ratingsBar = new Bar
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
                                                negativeRatings = new OsuSpriteText
                                                {
                                                    Font = @"Exo2.0-Regular",
                                                    Text = "0",
                                                },
                                                positiveRatings = new OsuSpriteText
                                                {
                                                    Font = @"Exo2.0-Regular",
                                                    Text = "0",
                                                    Anchor = Anchor.TopRight,
                                                    Origin = Anchor.TopRight,
                                                },
                                            },
                                        },
                                        new OsuSpriteText
                                        {
                                            Text = "Rating Spread",
                                            TextSize = 14,
                                            Font = @"Exo2.0-Regular",
                                            Anchor = Anchor.TopCentre,
                                            Origin = Anchor.TopCentre,
                                        },
                                        ratingsGraph = new BarGraph
                                        {
                                            RelativeSizeAxes = Axes.X,
                                            Height = 50,
                                        },
                                    },
                                },
                            },
                        },
                        new OsuSpriteText
                        {
                            Text = "Points of Failure",
                            Font = @"Exo2.0-Regular",
                        },
                        new Container<BarGraph>
                        {
                            RelativeSizeAxes = Axes.X,
                            Size = new Vector2(1/0.6f, 50),
                            Children = new[]
                            {
                                retryGraph = new BarGraph
                                {
                                    RelativeSizeAxes = Axes.Both,
                                },
                                failGraph = new BarGraph
                                {
                                    RelativeSizeAxes = Axes.Both,
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
            description.ContentColour = colour.GrayB;
            source.ContentColour = colour.GrayB;
            tags.ContentColour = colour.YellowLight;

            stars.BarColour = colour.Yellow;

            ratingsBar.BackgroundColour = colour.Green;
            ratingsBar.BarColour = colour.YellowDark;
            ratingsGraph.Colour = colour.BlueDark;

            failGraph.Colour = colour.YellowDarker;
            retryGraph.Colour = colour.Yellow;
        }

        private class DifficultyRow : Container
        {
            private readonly OsuSpriteText name;
            private readonly Bar bar;
            private readonly OsuSpriteText valueText;

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
                    bar.Length = value / maxValue;
                    valueText.Text = value.ToString(CultureInfo.InvariantCulture);
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
                    bar.Length = Value / value;
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
                    name = new OsuSpriteText
                    {
                        Font = @"Exo2.0-Regular",
                    },
                    bar = new Bar
                    {
                        Origin = Anchor.CentreLeft,
                        Anchor = Anchor.CentreLeft,
                        RelativeSizeAxes = Axes.Both,
                        Size = new Vector2(1, 0.35f),
                        Padding = new MarginPadding { Left = 100, Right = 25 },
                    },
                    valueText = new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Font = @"Exo2.0-Regular",
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

        private class MetadataSegment : Container
        {
            private readonly OsuSpriteText header;
            private readonly FillFlowContainer<OsuSpriteText> content;

            private const int fade_time = 250;

            public string HeaderText
            {
                set
                {
                    header.Text = value;
                }
            }

            public string ContentText
            {
                set
                {
                    if (value == "")
                        FadeOut(fade_time);
                    else
                    {
                        FadeIn(fade_time);
                        content.Children = value.Split(' ').Select(text => new OsuSpriteText
                        {
                            Text = text + " ",
                            Font = "Exo2.0-Regular",
                        });
                    }
                }
            }

            public SRGBColour ContentColour
            {
                set
                {
                    content.Colour = value;
                }
            }

            public MetadataSegment()
            {
                Children = new Drawable[]
                {
                    header = new OsuSpriteText
                    {
                        Font = @"Exo2.0-Bold",
                    },
                    content = new FillFlowContainer<OsuSpriteText>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Full,
                        Margin = new MarginPadding { Top = header.TextSize }
                    }
                };
            }
        }
    }
}