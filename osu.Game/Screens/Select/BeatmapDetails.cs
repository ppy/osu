// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Overlays.BeatmapSet;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Screens.Select.Details;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select
{
    public partial class BeatmapDetails : Container
    {
        private const float spacing = 10;
        private const float transition_duration = 250;

        private readonly UserRatings ratingsDisplay;
        private readonly MetadataSection description, source, tags;
        private readonly Container failRetryContainer;
        private readonly FailRetryGraph failRetryGraph;
        private readonly LoadingLayer loading;

        [Resolved]
        private IAPIProvider api { get; set; } = null!;

        [Resolved]
        private SongSelect? songSelect { get; set; }

        private IBeatmapInfo? beatmapInfo;

        private APIFailTimes? failTimes;

        private int[]? ratings;

        public IBeatmapInfo? BeatmapInfo
        {
            get => beatmapInfo;
            set
            {
                if (value == beatmapInfo) return;

                beatmapInfo = value;

                var onlineInfo = beatmapInfo as IBeatmapOnlineInfo;
                var onlineSetInfo = beatmapInfo?.BeatmapSet as IBeatmapSetOnlineInfo;

                failTimes = onlineInfo?.FailTimes;
                ratings = onlineSetInfo?.Ratings;

                Scheduler.AddOnce(updateStatistics);
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
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension()
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Horizontal,
                                        Children = new Drawable[]
                                        {
                                            new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Width = 0.5f,
                                                Spacing = new Vector2(spacing),
                                                Padding = new MarginPadding { Right = spacing / 2 },
                                                Children = new[]
                                                {
                                                    new DetailBox().WithChild(new OnlineViewContainer(string.Empty)
                                                    {
                                                        RelativeSizeAxes = Axes.X,
                                                        Height = 134,
                                                        Padding = new MarginPadding { Horizontal = spacing, Top = spacing },
                                                        Child = ratingsDisplay = new UserRatings
                                                        {
                                                            RelativeSizeAxes = Axes.Both,
                                                        },
                                                    }),
                                                },
                                            },
                                            new OsuScrollContainer
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Width = 0.5f,
                                                ScrollbarVisible = false,
                                                Padding = new MarginPadding { Left = spacing / 2 },
                                                Child = new FillFlowContainer
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                    AutoSizeAxes = Axes.Y,
                                                    LayoutDuration = transition_duration,
                                                    LayoutEasing = Easing.OutQuad,
                                                    Children = new[]
                                                    {
                                                        description = new MetadataSectionDescription(query => songSelect?.Search(query)),
                                                        source = new MetadataSectionSource(query => songSelect?.Search(query)),
                                                        tags = new MetadataSectionTags(query => songSelect?.Search(query)),
                                                    },
                                                },
                                            },
                                        },
                                    },
                                },
                                new Drawable[]
                                {
                                    failRetryContainer = new OnlineViewContainer("Sign in to view more details")
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            new OsuSpriteText
                                            {
                                                Text = BeatmapsetsStrings.ShowInfoPointsOfFailure,
                                                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 14),
                                            },
                                            failRetryGraph = new FailRetryGraph
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Padding = new MarginPadding { Top = 14 + spacing / 2 },
                                            },
                                        },
                                    },
                                }
                            }
                        },
                    },
                },
                loading = new LoadingLayer(true)
            };
        }

        private void updateStatistics()
        {
            description.Metadata = BeatmapInfo?.DifficultyName ?? string.Empty;
            source.Metadata = BeatmapInfo?.Metadata.Source ?? string.Empty;
            tags.Metadata = BeatmapInfo?.Metadata.Tags ?? string.Empty;

            // failTimes may have been previously fetched
            if (ratings != null && failTimes != null)
            {
                updateMetrics();
                return;
            }

            // for now, let's early abort if an OnlineID is not present (should have been populated at import time).
            if (BeatmapInfo == null || BeatmapInfo.OnlineID <= 0 || api.State.Value == APIState.Offline)
            {
                updateMetrics();
                return;
            }

            var requestedBeatmap = BeatmapInfo;

            var lookup = new GetBeatmapRequest(requestedBeatmap);

            lookup.Success += res =>
            {
                Schedule(() =>
                {
                    if (beatmapInfo != requestedBeatmap)
                        // the beatmap has been changed since we started the lookup.
                        return;

                    ratings = res.BeatmapSet?.Ratings;
                    failTimes = res.FailTimes;

                    updateMetrics();
                });
            };

            lookup.Failure += _ =>
            {
                Schedule(() =>
                {
                    if (beatmapInfo != requestedBeatmap)
                        // the beatmap has been changed since we started the lookup.
                        return;

                    updateMetrics();
                });
            };

            api.Queue(lookup);
            loading.Show();
        }

        private void updateMetrics()
        {
            bool hasMetrics = (failTimes?.Retries?.Any() ?? false) || (failTimes?.Fails?.Any() ?? false);

            if (ratings?.Any() ?? false)
            {
                ratingsDisplay.Ratings = ratings;
                ratingsDisplay.FadeIn(transition_duration);
            }
            else
            {
                // loading or just has no data server-side.
                ratingsDisplay.Ratings = new int[10];
                ratingsDisplay.FadeTo(0.25f, transition_duration);
            }

            if (hasMetrics)
            {
                failRetryGraph.FailTimes = failTimes;
                failRetryContainer.FadeIn(transition_duration);
            }
            else
            {
                failRetryGraph.FailTimes = new APIFailTimes
                {
                    Fails = new int[100],
                    Retries = new int[100],
                };
            }

            loading.Hide();
        }

        private partial class DetailBox : Container
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
    }
}
