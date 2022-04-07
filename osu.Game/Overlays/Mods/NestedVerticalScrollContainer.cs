// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Mods
{
    /// <summary>
    /// A scroll container that handles the case of vertically scrolling content inside a larger horizontally scrolling parent container.
    /// </summary>
    public class NestedVerticalScrollContainer : OsuScrollContainer
    {
        private OsuScrollContainer? parentScrollContainer;

        protected override void LoadComplete()
        {
            base.LoadComplete();

            parentScrollContainer = this.FindClosestParent<OsuScrollContainer>();
        }

        protected override bool OnScroll(ScrollEvent e)
        {
            if (parentScrollContainer == null)
                return base.OnScroll(e);

            bool topRightInView = parentScrollContainer.ScreenSpaceDrawQuad.Contains(ScreenSpaceDrawQuad.TopRight);
            bool bottomLeftInView = parentScrollContainer.ScreenSpaceDrawQuad.Contains(ScreenSpaceDrawQuad.BottomLeft);

            // If not completely on-screen, handle scroll but also allow parent to scroll at the same time (to hopefully bring our content into full view).
            if (!topRightInView || !bottomLeftInView)
                return false;

            bool scrollingPastEnd = e.ScrollDelta.Y < 0 && IsScrolledToEnd();
            bool scrollingPastStart = e.ScrollDelta.Y > 0 && Target <= 0;

            // If at either of our extents, delegate scroll to the horizontal parent container.
            if (scrollingPastStart || scrollingPastEnd)
                return false;

            return base.OnScroll(e);
        }
    }
}
