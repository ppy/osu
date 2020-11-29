// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Graphics.Containers
{
    public class UserTrackingScrollContainer : OsuScrollContainer
    {
        /// <summary>
        /// Whether the last scroll event was user triggered, directly on the scroll container.
        /// </summary>
        public bool UserScrolling { get; private set; }

        protected override void OnUserScroll(float value, bool animated = true, double? distanceDecay = default)
        {
            UserScrolling = true;
            base.OnUserScroll(value, animated, distanceDecay);
        }

        public new void ScrollTo(float value, bool animated = true, double? distanceDecay = null)
        {
            UserScrolling = false;
            base.ScrollTo(value, animated, distanceDecay);
        }
    }
}
