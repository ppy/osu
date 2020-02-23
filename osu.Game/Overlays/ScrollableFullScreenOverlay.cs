// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;

namespace osu.Game.Overlays
{
    public class ScrollableFullScreenOverlay : FullscreenOverlay
    {
        protected override Container<Drawable> Content => ScrollFlow;

        protected OverlayScrollContainer ScrollFlow { get; }

        protected Box Background { get; }

        public ScrollableFullScreenOverlay(OverlayColourScheme colourScheme)
            : base(colourScheme)
        {
            base.Content.AddRange(new Drawable[]
            {
                Background = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },
                ScrollFlow = new OverlayScrollContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    ScrollbarVisible = false
                }
            });
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Background.Colour = ColourProvider.Background6;
        }
    }
}
