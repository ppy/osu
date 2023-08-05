// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Graphics.Containers
{
    public partial class UserTrackingScrollContainer : UserTrackingScrollContainer<Drawable>
    {
        public UserTrackingScrollContainer()
        {
        }

        public UserTrackingScrollContainer(Direction direction)
            : base(direction)
        {
        }
    }

    public partial class UserTrackingScrollContainer<T> : OsuScrollContainer<T>
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
            UserScrolling = true;
            base.OnUserScroll(value, animated, distanceDecay);
        }

        public new void ScrollIntoView(Drawable target, bool animated = true)
        {
            UserScrolling = false;
            base.ScrollIntoView(target, animated);
        }

        public new void ScrollTo(float value, bool animated = true, double? distanceDecay = null)
        {
            UserScrolling = false;
            base.ScrollTo(value, animated, distanceDecay);
        }

        public new void ScrollToEnd(bool animated = true, bool allowDuringDrag = false)
        {
            UserScrolling = false;
            base.ScrollToEnd(animated, allowDuringDrag);
        }
    }
}
