// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Utils;

namespace osu.Game.Screens.Select.Carousel
{
    public static class CarouselHelper
    {
        /// <summary>
        /// Finds a new Y position for some panel given its target and current Y positions
        /// </summary>
        /// <param name="targetY">The panel's final Y position as defined by the carousel</param>
        /// <param name="currentY">The panel's current Y position</param>
        /// <param name="elapsed">The time elapsed since last time this function was used for the panel</param>
        /// <returns>New Y positon for the panel</returns>
        public static float FindPanelYPosition(float targetY, float currentY, double elapsed)
        {
            if (elapsed < 0)
                return currentY;

            // algorithm for this is taken from ScrollContainer.
            // while it doesn't necessarily need to match 1:1, as we are emulating scroll in some cases this feels most correct.
            return (float)Interpolation.Lerp(targetY, currentY, Math.Exp(-0.01 * elapsed));
        }
    }
}
