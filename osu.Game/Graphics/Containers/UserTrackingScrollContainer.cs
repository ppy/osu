// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Graphics.Containers
{
    public class UserTrackingScrollContainer : UserTrackingScrollContainer<Drawable>
    {
        public UserTrackingScrollContainer()
        {
        }

        public UserTrackingScrollContainer(Direction direction)
            : base(direction)
        {
        }
    }

    public class UserTrackingScrollContainer<T> : OsuScrollContainer<T>
        where T : Drawable
    {
        /// <summary>
        /// Whether the last scroll event was user triggered, directly on the scroll container.
        /// </summary>
        public bool UserScrolling { get; private set; }

        public UserTrackingScrollContainer()
        {
        }

        public UserTrackingScrollContainer(Direction direction)
            : base(direction)
        {
        }

        protected override void OnUserScroll(float value, bool animated = true, double? distanceDecay = default)
        {
            base.OnUserScroll(value, animated, distanceDecay);
            OnScrollChange(true);
        }

        public new void ScrollIntoView(Drawable target, bool animated = true)
        {
            base.ScrollIntoView(target, animated);
            OnScrollChange(false);
        }

        public new void ScrollTo(float value, bool animated = true, double? distanceDecay = null)
        {
            base.ScrollTo(value, animated, distanceDecay);
            OnScrollChange(false);
        }

        public new void ScrollToStart(bool animated = true, bool allowDuringDrag = false)
        {
            base.ScrollToStart(animated, allowDuringDrag);
            OnScrollChange(false);
        }

        public new void ScrollToEnd(bool animated = true, bool allowDuringDrag = false)
        {
            base.ScrollToEnd(animated, allowDuringDrag);
            OnScrollChange(false);
        }

        /// <summary>
        /// Invoked when any scroll has been performed either automatically or by user.
        /// </summary>
        protected virtual void OnScrollChange(bool byUser) => UserScrolling = byUser;
    }
}
