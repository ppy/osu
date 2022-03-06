// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Framework.Utils;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Chat
{
    /// <summary>
    /// An <see cref="OsuScrollContainer"/> with functionality to automatically scroll whenever the maximum scrollable distance increases.
    /// </summary>
    public class ChannelScrollContainer : OsuScrollContainer
    {
        /// <summary>
        /// The chat will be automatically scrolled to end if and only if
        /// the distance between the current scroll position and the end of the scroll
        /// is less than this value.
        /// </summary>
        private const float auto_scroll_leniency = 10f;

        private bool trackNewContent = true;

        protected override void Update()
        {
            base.Update();

            // If our behaviour hasn't been overriden and there has been new content added to the container, we should update our scroll position to track it.
            bool requiresScrollUpdate = trackNewContent && !IsScrolledToEnd();

            if (requiresScrollUpdate)
            {
                // Schedule required to allow FillFlow to be the correct size.
                Schedule(() =>
                {
                    if (trackNewContent)
                    {
                        if (Current < ScrollableExtent)
                            ScrollToEnd();
                    }
                });
            }
        }

        private void updateTrackState() => trackNewContent = IsScrolledToEnd(auto_scroll_leniency);

        // todo: we may eventually want this encapsulated in a "OnScrollChange" event handler method provided by ScrollContainer.
        // important to note that this intentionally doesn't consider OffsetScrollPosition, but could make it do so with side changes.

        #region Scroll handling

        protected override void OnUserScroll(float value, bool animated = true, double? distanceDecay = null)
        {
            base.OnUserScroll(value, animated, distanceDecay);
            updateTrackState();
        }

        public new void ScrollIntoView(Drawable d, bool animated = true)
        {
            base.ScrollIntoView(d, animated);
            updateTrackState();
        }

        public new void ScrollToStart(bool animated = true, bool allowDuringDrag = false)
        {
            base.ScrollToStart(animated, allowDuringDrag);
            updateTrackState();
        }

        public new void ScrollToEnd(bool animated = true, bool allowDuringDrag = false)
        {
            base.ScrollToEnd(animated, allowDuringDrag);
            updateTrackState();
        }

        #endregion
    }
}
