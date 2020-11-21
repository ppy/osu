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
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Overlays.BeatmapListing.Panels;
using osuTK;

namespace osu.Game.Overlays
{
    public class BeatmapListingOverlay : FullscreenOverlay<BeatmapListingHeader>
    {
        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        private Drawable currentContent;
        private LoadingLayer loadingLayer;
        private Container panelTarget;
        private FillFlowContainer<BeatmapPanel> foundContent;
        private NotFoundDrawable notFoundContent;

        private OverlayScrollContainer resultScrollContainer;

        public BeatmapListingOverlay()
            : base(OverlayColourScheme.Blue, new BeatmapListingHeader())
        {
        }

        private BeatmapListingFilterControl filterControl;

        [BackgroundDependencyLoader]
        private void load()
        {
            Children = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourProvider.Background6
                },
                resultScrollContainer = new OverlayScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false,
                    Child = new ReverseChildIDFillFlowContainer<Drawable>
                    {
                        AutoSizeAxes = Axes.Y,
                        RelativeSizeAxes = Axes.X,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            Header,
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
                                            loadingLayer = new LoadingLayer(panelTarget)
                                        }
                                    }
                                }
                            },
                        }
                    }
                }
            };
        }

        private void onTypingStarted()
        {
            // temporary until the textbox/header is updated to always stay on screen.
            resultScrollContainer.ScrollToStart();
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
                loadingLayer.Show();
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
            loadingLayer.Hide();
            lastFetchDisplayedTime = Time.Current;

            var lastContent = currentContent;

            if (lastContent != null)
            {
                lastContent.FadeOut(100, Easing.OutQuint).Expire();

                // Consider the case when the new content is smaller than the last content.
                // If the auto-size computation is delayed until fade out completes, the background remain high for too long making the resulting transition to the smaller height look weird.
                // At the same time, if the last content's height is bypassed immediately, there is a period where the new content is at Alpha = 0 when the auto-sized height will be 0.
                // To resolve both of these issues, the bypass is delayed until a point when the content transitions (fade-in and fade-out) overlap and it looks good to do so.
                lastContent.Delay(25).Schedule(() => lastContent.BypassAutoSizeAxes = Axes.Y).Then().Schedule(() => panelTarget.Remove(lastContent));
            }

            if (!content.IsAlive)
                panelTarget.Add(content);
            content.FadeIn(200, Easing.OutQuint);

            currentContent = content;
        }

        protected override void Dispose(bool isDisposing)
        {
            cancellationToken?.Cancel();
            base.Dispose(isDisposing);
        }

        private class NotFoundDrawable : CompositeDrawable
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
                                  && (resultScrollContainer.ScrollableExtent > 0 && resultScrollContainer.IsScrolledToEnd(pagination_scroll_distance));

            if (shouldShowMore)
                filterControl.FetchNextPage();
        }
    }
}
