// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using System.Linq;
using osu.Game.Online.API;
using osu.Framework.Threading;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Extensions.Color4Extensions;
using osu.Game.Screens.Select.Details;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API.Requests;
using osu.Game.Rulesets;

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
        private readonly OsuScrollContainer metadataScroll;
        private readonly MetadataSection description, source, tags;
        private readonly Container failRetryContainer;
        private readonly FailRetryGraph failRetryGraph;
        private readonly LoadingLayer loading;

        [Resolved]
        private IAPIProvider api { get; set; }

        private ScheduledDelegate pendingBeatmapSwitch;

        [Resolved]
        private RulesetStore rulesets { get; set; }

        private BeatmapInfo beatmap;

        public BeatmapInfo Beatmap
        {
            get => beatmap;
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
            Container content;

            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Color4.Black.Opacity(0.5f),
                },
                content = new Container
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
                                metadataScroll = new OsuScrollContainer
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
                                        LayoutEasing = Easing.OutQuad,
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
                                    Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 14),
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
                loading = new LoadingLayer(content),
            };
        }

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            metadataScroll.Height = statsFlow.DrawHeight;
            failRetryContainer.Height = DrawHeight - Padding.TotalVertical - (top.DrawHeight + spacing / 2);
        }

        private void updateStatistics()
        {
            advanced.Beatmap = Beatmap;
            description.Text = Beatmap?.Version;
            source.Text = Beatmap?.Metadata?.Source;
            tags.Text = Beatmap?.Metadata?.Tags;

            // metrics may have been previously fetched
            if (Beatmap?.BeatmapSet?.Metrics != null && Beatmap?.Metrics != null)
            {
                updateMetrics();
                return;
            }

            // for now, let's early abort if an OnlineBeatmapID is not present (should have been populated at import time).
            if (Beatmap?.OnlineBeatmapID == null)
            {
                updateMetrics();
                return;
            }

            var requestedBeatmap = Beatmap;

            var lookup = new GetBeatmapRequest(requestedBeatmap);

            lookup.Success += res =>
            {
                Schedule(() =>
                {
                    if (beatmap != requestedBeatmap)
                        // the beatmap has been changed since we started the lookup.
                        return;

                    var b = res.ToBeatmap(rulesets);

                    if (requestedBeatmap.BeatmapSet == null)
                        requestedBeatmap.BeatmapSet = b.BeatmapSet;
                    else
                        requestedBeatmap.BeatmapSet.Metrics = b.BeatmapSet.Metrics;

                    requestedBeatmap.Metrics = b.Metrics;

                    updateMetrics();
                });
            };

            lookup.Failure += e =>
            {
                Schedule(() =>
                {
                    if (beatmap != requestedBeatmap)
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
            var hasRatings = beatmap?.BeatmapSet?.Metrics?.Ratings?.Any() ?? false;
            var hasRetriesFails = (beatmap?.Metrics?.Retries?.Any() ?? false) || (beatmap?.Metrics?.Fails?.Any() ?? false);

            if (hasRatings)
            {
                ratings.Metrics = beatmap.BeatmapSet.Metrics;
                ratingsContainer.FadeIn(transition_duration);
            }
            else
            {
                ratings.Metrics = new BeatmapSetMetrics { Ratings = new int[10] };
                ratingsContainer.FadeTo(0.25f, transition_duration);
            }

            if (hasRetriesFails)
            {
                failRetryGraph.Metrics = beatmap.Metrics;
                failRetryContainer.FadeIn(transition_duration);
            }
            else
            {
                failRetryGraph.Metrics = new BeatmapMetrics
                {
                    Fails = new int[100],
                    Retries = new int[100],
                };
                failRetryContainer.FadeOut(transition_duration);
            }

            loading.Hide();
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
                Alpha = 0;
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;

                InternalChild = textContainer = new FillFlowContainer
                {
                    Alpha = 0,
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
                                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 14),
                            },
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

                    this.FadeIn(transition_duration);

                    setTextAsync(value);
                }
            }

            private void setTextAsync(string text)
            {
                LoadComponentAsync(new OsuTextFlowContainer(s => s.Font = s.Font.With(size: 14))
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
                    textContainer.FadeIn(transition_duration);
                });
            }
        }
    }
}
