// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.Chat
{
    /// <summary>
    /// An <see cref="OsuScrollContainer"/> with functionality to automatically scroll whenever the maximum scrollable distance increases.
    /// </summary>
    public partial class ChannelScrollContainer : OsuScrollContainer
    {
        /// <summary>
        /// The chat will be automatically scrolled to end if and only if
        /// the distance between the current scroll position and the end of the scroll
        /// is less than this value.
        /// </summary>
        private const float auto_scroll_leniency = 10f;

        /// <summary>
        /// Whether to keep this container scrolled to end on new content.
        /// </summary>
        /// <remarks>
        /// This is specifically controlled by whether the latest scroll operation made the container scrolled to end.
        /// </remarks>
        private bool trackNewContent = true;

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            if (trackNewContent && !IsScrolledToEnd())
                ScrollToEnd();
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

        public new void ScrollTo(float value, bool animated = true, double? distanceDecay = null)
        {
            base.ScrollTo(value, animated, distanceDecay);
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
