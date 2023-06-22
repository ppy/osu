// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A snap provider which given a proposed position for a hit object, potentially offers a more correct position and time value inferred from the context of the beatmap.
    /// </summary>
    [Cached]
    public interface IPositionSnapProvider
    {
        /// <summary>
        /// Given a position, find a valid time and position snap.
        /// </summary>
        /// <param name="screenSpacePosition">The screen-space position to be snapped.</param>
        /// <param name="snapType">The type of snapping to apply.</param>
        /// <returns>The time and position post-snapping.</returns>
        SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition, SnapType snapType = SnapType.All);
    }
}
