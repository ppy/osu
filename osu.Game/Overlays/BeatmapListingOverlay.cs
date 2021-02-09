// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Overlays.BeatmapListing.Panels;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays
{
    public class BeatmapListingOverlay : OnlineOverlay<BeatmapListingHeader>
    {
        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        private Drawable currentContent;
        private Container panelTarget;
        private FillFlowContainer<BeatmapPanel> foundContent;
        private NotFoundDrawable notFoundContent;
        private BeatmapListingFilterControl filterControl;

        public BeatmapListingOverlay()
            : base(OverlayColourScheme.Blue)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Child = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                Children = new Drawable[]
                {
                    filterControl = new BeatmapListingFilterControl
                    {
                        TypingStarted = onTypingStarted,
                        SearchStarted = onSearchStarted,
                        SearchFinished = onSearchFinished,
                    },
                    new Container
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Children = new Drawable[]
                        {
                            new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = ColourProvider.Background4,
                            },
                            panelTarget = new Container
                            {
                                AutoSizeAxes = Axes.Y,
                                RelativeSizeAxes = Axes.X,
                                Padding = new MarginPadding { Horizontal = 20 },
                                Children = new Drawable[]
                                {
                                    foundContent = new FillFlowContainer<BeatmapPanel>(),
                                    notFoundContent = new NotFoundDrawable(),
                                }
                            }
                        },
                    },
                }
            };
        }

        protected override BeatmapListingHeader CreateHeader() => new BeatmapListingHeader();

        protected override Color4 BackgroundColour => ColourProvider.Background6;

        private void onTypingStarted()
        {
            // temporary until the textbox/header is updated to always stay on screen.
            ScrollFlow.ScrollToStart();
        }

        protected override void OnFocus(FocusEvent e)
        {
            base.OnFocus(e);

            filterControl.TakeFocus();
        }

        private CancellationTokenSource cancellationToken;

        private void onSearchStarted()
        {
            cancellationToken?.Cancel();

            previewTrackManager.StopAnyPlaying(this);

            if (panelTarget.Any())
                Loading.Show();
        }

        private Task panelLoadDelegate;

        private void onSearchFinished(List<BeatmapSetInfo> beatmaps)
        {
            var newPanels = beatmaps.Select<BeatmapSetInfo, BeatmapPanel>(b => new GridBeatmapPanel(b)
            {
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
            });

            if (filterControl.CurrentPage == 0)
            {
                //No matches case
                if (!newPanels.Any())
                {
                    LoadComponentAsync(notFoundContent, addContentToPlaceholder, (cancellationToken = new CancellationTokenSource()).Token);
                    return;
                }

                // spawn new children with the contained so we only clear old content at the last moment.
                var content = new FillFlowContainer<BeatmapPanel>
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(10),
                    Alpha = 0,
                    Margin = new MarginPadding { Vertical = 15 },
                    ChildrenEnumerable = newPanels
                };

                panelLoadDelegate = LoadComponentAsync(foundContent = content, addContentToPlaceholder, (cancellationToken = new CancellationTokenSource()).Token);
            }
            else
            {
                panelLoadDelegate = LoadComponentsAsync(newPanels, loaded =>
                {
                    lastFetchDisplayedTime = Time.Current;
                    foundContent.AddRange(loaded);
                    loaded.ForEach(p => p.FadeIn(200, Easing.OutQuint));
                });
            }
        }

        private void addContentToPlaceholder(Drawable content)
        {
            Loading.Hide();
            lastFetchDisplayedTime = Time.Current;

            if (content == currentContent)
                return;

            var lastContent = currentContent;

            if (lastContent != null)
            {
                var transform = lastContent.FadeOut(100, Easing.OutQuint);

                if (lastContent == notFoundContent)
                {
                    // not found display may be used multiple times, so don't expire/dispose it.
                    transform.Schedule(() => panelTarget.Remove(lastContent));
                }
                else
                {
                    // Consider the case when the new content is smaller than the last content.
                    // If the auto-size computation is delayed until fade out completes, the background remain high for too long making the resulting transition to the smaller height look weird.
                    // At the same time, if the last content's height is bypassed immediately, there is a period where the new content is at Alpha = 0 when the auto-sized height will be 0.
                    // To resolve both of these issues, the bypass is delayed until a point when the content transitions (fade-in and fade-out) overlap and it looks good to do so.
                    lastContent.Delay(25).Schedule(() => lastContent.BypassAutoSizeAxes = Axes.Y).Then().Schedule(() => lastContent.Expire());
                }
            }

            if (!content.IsAlive)
                panelTarget.Add(content);

            content.FadeInFromZero(200, Easing.OutQuint);
            currentContent = content;
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }

        public class NotFoundDrawable : CompositeDrawable
        {
            public NotFoundDrawable()
            {
                RelativeSizeAxes = Axes.X;
                Height = 250;
                Alpha = 0;
                Margin = new MarginPadding { Top = 15 };
            }

            [BackgroundDependencyLoader]
            private void load(TextureStore textures)
            {
                AddInternal(new FillFlowContainer
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    RelativeSizeAxes = Axes.Y,
                    AutoSizeAxes = Axes.X,
                    Direction = FillDirection.Horizontal,
                    Spacing = new Vector2(10, 0),
                    Children = new Drawable[]
                    {
                        new Sprite
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            RelativeSizeAxes = Axes.Both,
                            FillMode = FillMode.Fit,
                            Texture = textures.Get(@"Online/not-found")
                        },
                        new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Text = @"... nope, nothing found.",
                        }
                    }
                });
            }
        }

        private const double time_between_fetches = 500;

        private double lastFetchDisplayedTime;

        protected override void Update()
        {
            base.Update();

            const int pagination_scroll_distance = 500;

            bool shouldShowMore = panelLoadDelegate?.IsCompleted != false
                                  && Time.Current - lastFetchDisplayedTime > time_between_fetches
                                  && (ScrollFlow.ScrollableExtent > 0 && ScrollFlow.IsScrolledToEnd(pagination_scroll_distance));

            if (shouldShowMore)
                filterControl.FetchNextPage();
        }
    }
}
