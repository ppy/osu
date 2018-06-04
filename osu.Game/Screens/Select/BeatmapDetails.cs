// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using OpenTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using System.Linq;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Framework.Threading;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Screens.Select.Details;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Select
{
    public class BeatmapDetails : Container
    {
        private const float spacing = 10;
        private const float transition_duration = 250;

        private readonly FillFlowContainer top, statsFlow;
        private readonly AdvancedStats advanced;
        private readonly DetailBox ratingsContainer;
        private readonly UserRatings ratings;
        private readonly ScrollContainer metadataScroll;
        private readonly MetadataSection description, source, tags;
        private readonly Container failRetryContainer;
        private readonly FailRetryGraph failRetryGraph;
        private readonly DimmedLoadingAnimation loading;

        private APIAccess api;

        private ScheduledDelegate pendingBeatmapSwitch;

        private BeatmapInfo beatmap;
        public BeatmapInfo Beatmap
        {
            get { return beatmap; }
            set
            {
                if (value == beatmap) return;
                beatmap = value;

                pendingBeatmapSwitch?.Cancel();
                pendingBeatmapSwitch = Schedule(updateStatistics);
            }
        }

        public BeatmapDetails()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.5f),
                },
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding { Horizontal = spacing },
                    Children = new Drawable[]
                    {
                        top = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Direction = FillDirection.Horizontal,
                            Children = new Drawable[]
                            {
                                statsFlow = new FillFlowContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    Width = 0.5f,
                                    Spacing = new Vector2(spacing),
                                    Padding = new MarginPadding { Right = spacing / 2 },
                                    Children = new[]
                                    {
                                        new DetailBox
                                        {
                                            Child = advanced = new AdvancedStats
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Padding = new MarginPadding { Horizontal = spacing, Top = spacing * 2, Bottom = spacing },
                                            },
                                        },
                                        ratingsContainer = new DetailBox
                                        {
                                            Child = ratings = new UserRatings
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                Height = 134,
                                                Padding = new MarginPadding { Horizontal = spacing, Top = spacing },
                                            },
                                        },
                                    },
                                },
                                metadataScroll = new ScrollContainer
                                {
                                    RelativeSizeAxes = Axes.X,
                                    Width = 0.5f,
                                    ScrollbarVisible = false,
                                    Padding = new MarginPadding { Left = spacing / 2 },
                                    Child = new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        LayoutDuration = transition_duration,
                                        Spacing = new Vector2(spacing * 2),
                                        Margin = new MarginPadding { Top = spacing * 2 },
                                        Children = new[]
                                        {
                                            description = new MetadataSection("Description"),
                                            source = new MetadataSection("Source"),
                                            tags = new MetadataSection("Tags"),
                                        },
                                    },
                                },
                            },
                        },
                        failRetryContainer = new Container
                        {
                            Anchor = Anchor.BottomLeft,
                            Origin = Anchor.BottomLeft,
                            RelativeSizeAxes = Axes.X,
                            Children = new Drawable[]
                            {
                                new OsuSpriteText
                                {
                                    Text = "Points of Failure",
                                    Font = @"Exo2.0-Bold",
                                    TextSize = 14,
                                },
                                failRetryGraph = new FailRetryGraph
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Padding = new MarginPadding { Top = 14 + spacing / 2 },
                                },
                            },
                        },
                    },
                },
                loading = new DimmedLoadingAnimation
                {
                    RelativeSizeAxes = Axes.Both,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(APIAccess api)
        {
            this.api = api;
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            metadataScroll.Height = statsFlow.DrawHeight;
            failRetryContainer.Height = DrawHeight - Padding.TotalVertical - (top.DrawHeight + spacing / 2);
        }

        private void updateStatistics()
        {
            if (Beatmap == null)
            {
                clearStats();
                return;
            }

            ratingsContainer.FadeIn(transition_duration);
            advanced.Beatmap = Beatmap;
            description.Text = Beatmap.Version;
            source.Text = Beatmap.Metadata.Source;
            tags.Text = Beatmap.Metadata.Tags;

            var requestedBeatmap = Beatmap;
            if (requestedBeatmap.Metrics == null)
            {
                var lookup = new GetBeatmapDetailsRequest(requestedBeatmap);
                lookup.Success += res =>
                {
                    if (beatmap != requestedBeatmap)
                        //the beatmap has been changed since we started the lookup.
                        return;

                    requestedBeatmap.Metrics = res;
                    Schedule(() => displayMetrics(res));
                };
                lookup.Failure += e => Schedule(() => displayMetrics(null));

                api.Queue(lookup);
                loading.Show();
            }

            displayMetrics(requestedBeatmap.Metrics, false);
        }

        private void displayMetrics(BeatmapMetrics metrics, bool failOnMissing = true)
        {
            var hasRatings = metrics?.Ratings?.Any() ?? false;
            var hasRetriesFails = (metrics?.Retries?.Any() ?? false) && (metrics.Fails?.Any() ?? false);

            if (failOnMissing) loading.Hide();

            if (hasRatings)
            {
                ratings.Metrics = metrics;
                ratings.FadeIn(transition_duration);
            }
            else if (failOnMissing)
            {
                ratings.Metrics = new BeatmapMetrics
                {
                    Ratings = new int[10],
                };
            }
            else
            {
                ratings.FadeTo(0.25f, transition_duration);
            }

            if (hasRetriesFails)
            {
                failRetryGraph.Metrics = metrics;
                failRetryContainer.FadeIn(transition_duration);
            }
            else if (failOnMissing)
            {
                failRetryGraph.Metrics = new BeatmapMetrics
                {
                    Fails = new int[100],
                    Retries = new int[100],
                };
            }
            else
            {
                failRetryContainer.FadeTo(0.25f, transition_duration);
            }
        }

        private void clearStats()
        {
            description.Text = null;
            source.Text = null;
            tags.Text = null;

            advanced.Beatmap = new BeatmapInfo
            {
                StarDifficulty = 0,
                BaseDifficulty = new BeatmapDifficulty
                {
                    CircleSize = 0,
                    DrainRate = 0,
                    OverallDifficulty = 0,
                    ApproachRate = 0,
                },
            };

            loading.Hide();
            ratingsContainer.FadeOut(transition_duration);
            failRetryContainer.FadeOut(transition_duration);
        }

        private class DetailBox : Container
        {
            private readonly Container content;
            protected override Container<Drawable> Content => content;

            public DetailBox()
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChildren = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.5f),
                    },
                    content = new Container
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                    },
                };
            }
        }

        private class MetadataSection : Container
        {
            private readonly FillFlowContainer textContainer;
            private TextFlowContainer textFlow;

            public MetadataSection(string title)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Alpha = 0;

                InternalChild = textContainer = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(spacing / 2),
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = new OsuSpriteText
                            {
                                Text = title,
                                Font = @"Exo2.0-Bold",
                                TextSize = 14,
                            },
                        },
                        textFlow = new OsuTextFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                        },
                    },
                };
            }

            public string Text
            {
                set
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        this.FadeOut(transition_duration);
                        return;
                    }

                    setTextAsync(value);
                }
            }

            private void setTextAsync(string text)
            {
                LoadComponentAsync(new OsuTextFlowContainer(s => s.TextSize = 14)
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Colour = Color4.White.Opacity(0.75f),
                    Text = text
                }, loaded =>
                {
                    textFlow?.Expire();
                    textContainer.Add(textFlow = loaded);

                    // fade in if we haven't yet.
                    this.FadeIn(transition_duration);
                });
            }
        }

        private class DimmedLoadingAnimation : VisibilityContainer
        {
            private readonly LoadingAnimation loading;

            public DimmedLoadingAnimation()
            {
                Children = new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black.Opacity(0.5f),
                    },
                    loading = new LoadingAnimation(),
                };
            }

            protected override void PopIn()
            {
                this.FadeIn(transition_duration, Easing.OutQuint);
                loading.State = Visibility.Visible;
            }

            protected override void PopOut()
            {
                this.FadeOut(transition_duration, Easing.OutQuint);
                loading.State = Visibility.Hidden;
            }
        }
    }
}
