// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
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

        private readonly Container ratingsContainer;
        private readonly Bar ratingsBar;
        private readonly OsuSpriteText negativeRatings;
        private readonly OsuSpriteText positiveRatings;
        private readonly BarGraph ratingsGraph;

        private readonly FillFlowContainer retryFailContainer;
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
                beatmap = value;
                if (beatmap == null) return;

                description.Text = beatmap.Version;
                source.Text = beatmap.Metadata.Source;
                tags.Text = beatmap.Metadata.Tags;

                circleSize.Value = beatmap.Difficulty.CircleSize;
                drainRate.Value = beatmap.Difficulty.DrainRate;
                overallDifficulty.Value = beatmap.Difficulty.OverallDifficulty;
                approachRate.Value = beatmap.Difficulty.ApproachRate;
                stars.Value = (float)beatmap.StarDifficulty;

                if (beatmap.Metrics?.Ratings.Any() ?? false)
                {
                    var ratings = beatmap.Metrics.Ratings.ToList();
                    ratingsContainer.Show();

                    negativeRatings.Text = ratings.GetRange(0, ratings.Count / 2).Sum().ToString();
                    positiveRatings.Text = ratings.GetRange(ratings.Count / 2, ratings.Count / 2).Sum().ToString();
                    ratingsBar.Length = (float)ratings.GetRange(0, ratings.Count / 2).Sum() / ratings.Sum();

                    ratingsGraph.Values = ratings.Select(rating => (float)rating);
                }
                else
                    ratingsContainer.Hide();

                if ((beatmap.Metrics?.Retries.Any() ?? false) && beatmap.Metrics.Fails.Any())
                {
                    var retries = beatmap.Metrics.Retries;
                    var fails = beatmap.Metrics.Fails;
                    retryFailContainer.Show();

                    float maxValue = fails.Zip(retries, (fail, retry) => fail + retry).Max();
                    failGraph.MaxValue = maxValue;
                    retryGraph.MaxValue = maxValue;

                    failGraph.Values = fails.Select(fail => (float)fail);
                    retryGraph.Values = retries.Zip(fails, (retry, fail) => retry + MathHelper.Clamp(fail, 0, maxValue));
                }
                else
                    retryFailContainer.Hide();
            }
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
                    LayoutDuration = 200,
                    LayoutEasing = EasingTypes.OutQuint,
                    Padding = new MarginPadding(10) { Top = 25 },
                    Children = new []
                    {
                        description = new MetadataSegment("Description"),
                        source = new MetadataSegment("Source"),
                        tags = new MetadataSegment("Tags")
                    },
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Width = 0.6f,
                    Direction = FillDirection.Vertical,
                    Spacing = new Vector2(0, 15),
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
                                        circleSize = new DifficultyRow("Circle Size", 7),
                                        drainRate = new DifficultyRow("HP Drain"),
                                        overallDifficulty = new DifficultyRow("Accuracy"),
                                        approachRate = new DifficultyRow("Approach Rate"),
                                        stars = new DifficultyRow("Star Diffculty"),
                                    },
                                },
                            },
                        },
                        ratingsContainer = new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Alpha = 0,
                            AlwaysPresent = true,
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
                                    Padding = new MarginPadding
                                    {
                                        Top = 25,
                                        Left = 15,
                                        Right = 15,
                                    },
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
                        retryFailContainer = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Alpha = 0,
                            Children = new Drawable[]
                            {
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
                            }
                        },
                    },
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colour)
        {
            description.AccentColour = colour.GrayB;
            source.AccentColour = colour.GrayB;
            tags.AccentColour = colour.YellowLight;

            stars.AccentColour = colour.Yellow;

            ratingsBar.BackgroundColour = colour.Green;
            ratingsBar.AccentColour = colour.YellowDark;
            ratingsGraph.Colour = colour.BlueDark;

            failGraph.Colour = colour.YellowDarker;
            retryGraph.Colour = colour.Yellow;
        }

        private class DifficultyRow : Container, IHasAccentColour
        {
            private readonly OsuSpriteText name;
            private readonly Bar bar;
            private readonly OsuSpriteText valueText;

            private readonly float maxValue;

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

            public Color4 AccentColour
            {
                get
                {
                    return bar.AccentColour;
                }
                set
                {
                    bar.AccentColour = value;
                }
            }

            public DifficultyRow(string difficultyName, float maxValue = 10)
            {
                this.maxValue = maxValue;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Children = new Drawable[]
                {
                    name = new OsuSpriteText
                    {
                        Font = @"Exo2.0-Regular",
                        Text = difficultyName,
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

        private class MetadataSegment : Container, IHasAccentColour
        {
            private readonly OsuSpriteText header;
            private readonly FillFlowContainer<OsuSpriteText> content;

            public string Text
            {
                set
                {
                    if (string.IsNullOrEmpty(value))
                        Hide();
                    else
                    {
                        Show();
                        if (header.Text == "Tags")
                            content.Children = value.Split(' ').Select(text => new OsuSpriteText
                            {
                                Text = text,
                                Font = "Exo2.0-Regular",
                            });
                        else
                            content.Children = new[]
                            {
                                new OsuSpriteText
                                {
                                    Text = value,
                                    Font = "Exo2.0-Regular",
                                }
                            };
                    }
                }
            }

            public Color4 AccentColour
            {
                get
                {
                    return content.Colour;
                }
                set
                {
                    content.Colour = value;
                }
            }

            public MetadataSegment(string headerText)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Margin = new MarginPadding { Top = 10 };
                Children = new Drawable[]
                {
                    header = new OsuSpriteText
                    {
                        Font = @"Exo2.0-Bold",
                        Text = headerText,
                    },
                    content = new FillFlowContainer<OsuSpriteText>
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Full,
                        Spacing = new Vector2(5,0),
                        Margin = new MarginPadding { Top = header.TextSize }
                    }
                };
            }
        }
    }
}