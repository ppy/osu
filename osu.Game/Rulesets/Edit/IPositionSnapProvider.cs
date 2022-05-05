// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// A snap provider which given a proposed position for a hit object, potentially offers a more correct position and time value inferred from the context of the beatmap.
    /// Provided values are inferred in an isolated context, without consideration of other nearby hit objects.
    /// </summary>
    [Cached]
    public interface IPositionSnapProvider
    {
        /// <summary>
        /// Given a position, find a valid time and position snap.
        /// </summary>
        /// <remarks>
        /// This call should be equivalent to running <see cref="FindSnappedPosition"/> with any additional logic that can be performed without the time immutability restriction.
        /// </remarks>
        /// <param name="screenSpacePosition">The screen-space position to be snapped.</param>
        /// <returns>The time and position post-snapping.</returns>
        SnapResult FindSnappedPositionAndTime(Vector2 screenSpacePosition);

        /// <summary>
        /// Given a position, find a valid position snap, without changing the time value.
        /// </summary>
        /// <param name="screenSpacePosition">The screen-space position to be snapped.</param>
        /// <returns>The position post-snapping. Time will always be null.</returns>
        SnapResult FindSnappedPosition(Vector2 screenSpacePosition);
    }
}
