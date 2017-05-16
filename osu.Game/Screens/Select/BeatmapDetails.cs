// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using System.Globalization;
using System.Linq;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Framework.Threading;

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

        private ScheduledDelegate pendingBeatmapSwitch;
        private BeatmapInfo beatmap;

        public BeatmapInfo Beatmap
        {
            get { return beatmap; }
            set
            {
                beatmap = value;

                pendingBeatmapSwitch?.Cancel();
                pendingBeatmapSwitch = Schedule(updateStats);
            }
        }

        private void updateStats()
        {
            if (beatmap == null) return;

            description.Text = beatmap.Version;
            source.Text = beatmap.Metadata.Source;
            tags.Text = beatmap.Metadata.Tags;

            circleSize.Value = beatmap.Difficulty.CircleSize;
            drainRate.Value = beatmap.Difficulty.DrainRate;
            overallDifficulty.Value = beatmap.Difficulty.OverallDifficulty;
            approachRate.Value = beatmap.Difficulty.ApproachRate;
            stars.Value = (float)beatmap.StarDifficulty;

            var requestedBeatmap = beatmap;
            if (requestedBeatmap.Metrics == null)
            {
                var lookup = new GetBeatmapDetailsRequest(requestedBeatmap);
                lookup.Success += res =>
                {
                    if (beatmap != requestedBeatmap)
                            //the beatmap has been changed since we started the lookup.
                            return;

                    requestedBeatmap.Metrics = res;
                    Schedule(() => updateMetrics(res));
                };
                lookup.Failure += e => updateMetrics(null);

                api.Queue(lookup);
            }

            updateMetrics(requestedBeatmap.Metrics, false);
        }

        /// <summary>
        /// Update displayed metrics.
        /// </summary>
        /// <param name="metrics">New metrics to overwrite the existing display. Can be null.</param>
        /// <param name="failOnMissing">Whether to hide the display on null or empty metrics. If false, we will dim as if waiting for further updates.</param>
        private void updateMetrics(BeatmapMetrics metrics, bool failOnMissing = true)
        {
            var hasRatings = metrics?.Ratings.Any() ?? false;
            var hasRetriesFails = (metrics?.Retries.Any() ?? false) && metrics.Fails.Any();

            if (hasRatings)
            {
                var ratings = metrics.Ratings.ToList();
                ratingsContainer.Show();

                negativeRatings.Text = ratings.GetRange(0, ratings.Count / 2).Sum().ToString();
                positiveRatings.Text = ratings.GetRange(ratings.Count / 2, ratings.Count / 2).Sum().ToString();
                ratingsBar.Length = (float)ratings.GetRange(0, ratings.Count / 2).Sum() / ratings.Sum();

                ratingsGraph.Values = ratings.Select(rating => (float)rating);

                ratingsContainer.FadeColour(Color4.White, 500, EasingTypes.Out);
            }
            else if (failOnMissing)
                ratingsGraph.Values = new float[10];
            else
                ratingsContainer.FadeColour(Color4.Gray, 500, EasingTypes.Out);

            if (hasRetriesFails)
            {
                var retries = metrics.Retries;
                var fails = metrics.Fails;
                retryFailContainer.Show();

                float maxValue = fails.Zip(retries, (fail, retry) => fail + retry).Max();
                failGraph.MaxValue = maxValue;
                retryGraph.MaxValue = maxValue;

                failGraph.Values = fails.Select(fail => (float)fail);
                retryGraph.Values = retries.Zip(fails, (retry, fail) => retry + MathHelper.Clamp(fail, 0, maxValue));

                retryFailContainer.FadeColour(Color4.White, 500, EasingTypes.Out);
            }
            else if (failOnMissing)
            {
                failGraph.Values = new float[100];
                retryGraph.Values = new float[100];
            }
            else
                retryFailContainer.FadeColour(Color4.Gray, 500, EasingTypes.Out);
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
                                    Spacing = new Vector2(0,5),
                                    Padding = new MarginPadding(10),
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
                                    Size = new Vector2(1 / 0.6f, 50),
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

        private APIAccess api;

        [BackgroundDependencyLoader]
        private void load(OsuColour colour, APIAccess api)
        {
            this.api = api;

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
                    valueText.Text = value.ToString("N1", CultureInfo.CurrentCulture);
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
