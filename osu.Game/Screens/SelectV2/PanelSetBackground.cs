// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Beatmaps;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class PanelSetBackground : Container
    {
        [Resolved]
        private BeatmapCarousel? beatmapCarousel { get; set; }

        private Sprite? sprite;

        private WorkingBeatmap? working;

        private CancellationTokenSource? loadCancellation;

        private double timeSinceUnpool;

        public WorkingBeatmap? Beatmap
        {
            get => working;
            set
            {
                if (working == null && value == null)
                    return;

                // this guard papers over excessive refreshes of the background asset which occur if `working == value` type guards are used.
                // the root cause of why `working == value` type guards fail here is that `SongSelect` will invalidate working beatmaps very often
                // (via https://github.com/ppy/osu/blob/d3ae20dd882381e109c20ca00ee5237e4dd1750d/osu.Game/Screens/SelectV2/SongSelect.cs#L506-L507),
                // due to a variety of causes, ranging from "someone typed a letter in the search box" (which triggers a refilter -> presentation of new items -> `ensureGlobalBeatmapValid()`),
                // to "someone just went into the editor and replaced every single file in the set, including the background".
                // the following guard approximates the most appropriate debounce criterion, which is the contents of the actual asset that is supposed to be displayed in the background,
                // i.e. if the hash of the new background file matches the old, then we do not bother updating the working beatmap here.
                //
                // note that this is basically a reimplementation of the caching scheme in `WorkingBeatmapCache.getBackgroundFromStore()`,
                // which cannot be used directly by retrieving the texture and checking texture reference equality,
                // because missing the cache would incur a synchronous texture load on the update thread.
                if (getBackgroundFileHash(working) == getBackgroundFileHash(value))
                    return;

                working = value;

                loadCancellation?.Cancel();
                loadCancellation = null;

                sprite?.Expire();
                sprite = null;

                timeSinceUnpool = 0;
            }
        }

        private static string? getBackgroundFileHash(WorkingBeatmap? working)
            => working?.BeatmapSetInfo.GetFile(working.Metadata.BackgroundFile)?.File.Hash;

        public PanelSetBackground()
        {
            RelativeSizeAxes = Axes.Both;
            CornerRadius = Panel.CORNER_RADIUS;
            Masking = true;

            // Add some level of smoothness around the rounded edges to give more visual polish (make it anti-aliased).
            MaskingSmoothness = 2f;
        }

        protected override void Update()
        {
            base.Update();

            loadContentIfRequired();
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    Depth = 1,
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientHorizontal(colourProvider.Background3, colourProvider.Background4),
                },
                new FillFlowContainer
                {
                    Depth = -1,
                    RelativeSizeAxes = Axes.Both,
                    Direction = FillDirection.Horizontal,
                    // This makes the gradient not be perfectly horizontal, but diagonal at a ~40° angle
                    Shear = new Vector2(0.8f, 0),
                    Children = new[]
                    {
                        // The left half with no gradient applied
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black.Opacity(0.5f),
                            Width = 0.4f,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.5f), Color4.Black.Opacity(0.3f)),
                            Width = 0.2f,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = ColourInfo.GradientHorizontal(Color4.Black.Opacity(0.3f), Color4.Black.Opacity(0.2f)),
                            // Slightly more than 1.0 in total to account for shear.
                            Width = 0.45f,
                        },
                    }
                },
            };
        }

        private void loadContentIfRequired()
        {
            // A load is already in progress if the cancellation token is non-null.
            if (loadCancellation != null || working == null)
                return;

            if (beatmapCarousel != null)
            {
                Quad containingSsdq = beatmapCarousel.ScreenSpaceDrawQuad;

                // One may ask why we are not using `DelayedLoadWrapper` for this delayed load logic.
                //
                // - Using `DelayedLoadWrapper` would only allow us to load content when on screen, but we want to preload while panels are off-screen.
                //   This allows a more seamless experience when a user is scrolling at a moderate speed, as we are loading in backgrounds before they
                //   enter the visible viewport.
                // - By using a slightly customised formula to decide when to start the load, we can coerce the loading of backgrounds into an order that
                //   prioritises panels which are closest to the centre of the screen. Basically, we want to load backgrounds "outwards" from the visual
                //   centre to give the user the best experience possible.
                float timeUpdatingBeforeLoad = 50 + Math.Abs(containingSsdq.Centre.Y - ScreenSpaceDrawQuad.Centre.Y) / containingSsdq.Height * 100;

                timeSinceUnpool += Time.Elapsed;

                // We only trigger a load after this set has been in an updating state for a set amount of time.
                if (timeSinceUnpool <= timeUpdatingBeforeLoad)
                    return;
            }

            loadCancellation = new CancellationTokenSource();

            LoadComponentAsync(new PanelBeatmapBackground(working)
            {
                RelativeSizeAxes = Axes.Both,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                FillMode = FillMode.Fill,
            }, s =>
            {
                AddInternal(sprite = s);
                bool spriteOnScreen = beatmapCarousel?.ScreenSpaceDrawQuad.Intersects(sprite.ScreenSpaceDrawQuad) != false;
                sprite.FadeInFromZero(spriteOnScreen ? 400 : 0, Easing.OutQuint);
            }, loadCancellation.Token);
        }

        public partial class PanelBeatmapBackground : Sprite
        {
            private readonly IWorkingBeatmap working;

            public PanelBeatmapBackground(IWorkingBeatmap working)
            {
                ArgumentNullException.ThrowIfNull(working);

                this.working = working;
            }

            [BackgroundDependencyLoader]
            private void load()
            {
                Texture = working.GetPanelBackground();
            }
        }
    }
}
