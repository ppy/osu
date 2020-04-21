// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays.BeatmapListing;
using osu.Game.Overlays.Direct;
using osuTK;

namespace osu.Game.Overlays
{
    public class BeatmapListingOverlay : FullscreenOverlay
    {
        [Resolved]
        private PreviewTrackManager previewTrackManager { get; set; }

        private Drawable currentContent;
        private LoadingLayer loadingLayer;
        private Container panelTarget;

        public BeatmapListingOverlay()
            : base(OverlayColourScheme.Blue)
        {
        }

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
                new OverlayScrollContainer
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
                            new BeatmapListingHeader(),
                            new BeatmapListingFilterControl
                            {
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
                                        Padding = new MarginPadding { Horizontal = 20 }
                                    },
                                    loadingLayer = new LoadingLayer(panelTarget)
                                }
                            },
                        }
                    }
                }
            };
        }

        private CancellationTokenSource cancellationToken;

        private void onSearchStarted()
        {
            cancellationToken?.Cancel();

            previewTrackManager.StopAnyPlaying(this);

            if (panelTarget.Any())
                loadingLayer.Show();
        }

        private void onSearchFinished(List<BeatmapSetInfo> beatmaps)
        {
            if (!beatmaps.Any())
            {
                LoadComponentAsync(new NotFoundDrawable(), addContentToPlaceholder, (cancellationToken = new CancellationTokenSource()).Token);
                return;
            }

            var newPanels = new FillFlowContainer<DirectPanel>
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Spacing = new Vector2(10),
                Alpha = 0,
                Margin = new MarginPadding { Vertical = 15 },
                ChildrenEnumerable = beatmaps.Select<BeatmapSetInfo, DirectPanel>(b => new DirectGridPanel(b)
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                })
            };

            LoadComponentAsync(newPanels, addContentToPlaceholder, (cancellationToken = new CancellationTokenSource()).Token);
        }

        private void addContentToPlaceholder(Drawable content)
        {
            loadingLayer.Hide();

            var lastContent = currentContent;

            if (lastContent != null)
            {
                lastContent.FadeOut(100, Easing.OutQuint).Expire();

                // Consider the case when the new content is smaller than the last content.
                // If the auto-size computation is delayed until fade out completes, the background remain high for too long making the resulting transition to the smaller height look weird.
                // At the same time, if the last content's height is bypassed immediately, there is a period where the new content is at Alpha = 0 when the auto-sized height will be 0.
                // To resolve both of these issues, the bypass is delayed until a point when the content transitions (fade-in and fade-out) overlap and it looks good to do so.
                lastContent.Delay(25).Schedule(() => lastContent.BypassAutoSizeAxes = Axes.Y);
            }

            panelTarget.Add(currentContent = content);
            currentContent.FadeIn(200, Easing.OutQuint);
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
                            Text = @"呃...啥都没找到",
                        }
                    }
                });
            }
        }
    }
}