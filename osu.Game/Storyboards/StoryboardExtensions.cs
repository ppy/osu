// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Extensions.EnumExtensions;
using osu.Framework.Graphics;
using osuTK;

namespace osu.Game.Storyboards
{
    public static class StoryboardExtensions
    {
        /// <summary>
        /// Given an origin and a set of properties, adjust the origin to display the sprite/animation correctly.
        /// </summary>
        /// <param name="origin">The current origin.</param>
        /// <param name="vectorScale">The vector scale.</param>
        /// <param name="flipH">Whether the element is flipped horizontally.</param>
        /// <param name="flipV">Whether the element is flipped vertically.</param>
        /// <returns>The adjusted origin.</returns>
        public static Anchor AdjustOrigin(Anchor origin, Vector2 vectorScale, bool flipH, bool flipV)
        {
            // Either flip horizontally or negative X scale, but not both.
            if (flipH ^ (vectorScale.X < 0))
            {
                if (origin.HasFlagFast(Anchor.x0))
                    origin = Anchor.x2 | (origin & (Anchor.y0 | Anchor.y1 | Anchor.y2));
                else if (origin.HasFlagFast(Anchor.x2))
                    origin = Anchor.x0 | (origin & (Anchor.y0 | Anchor.y1 | Anchor.y2));
            }

            // Either flip vertically or negative Y scale, but not both.
            if (flipV ^ (vectorScale.Y < 0))
            {
                if (origin.HasFlagFast(Anchor.y0))
                    origin = Anchor.y2 | (origin & (Anchor.x0 | Anchor.x1 | Anchor.x2));
                else if (origin.HasFlagFast(Anchor.y2))
                    origin = Anchor.y0 | (origin & (Anchor.x0 | Anchor.x1 | Anchor.x2));
            }

            return origin;
        }
    }
}
