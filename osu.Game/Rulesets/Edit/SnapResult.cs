// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.UI;
using osuTK;

namespace osu.Game.Rulesets.Edit
{
    /// <summary>
    /// The result of a position/time snapping process.
    /// </summary>
    public class SnapResult
    {
        /// <summary>
        /// The screen space position, potentially altered for snapping.
        /// </summary>
        public Vector2 ScreenSpacePosition;

        /// <summary>
        /// The resultant time for snapping, if a value could be attained.
        /// </summary>
        public double? Time;

        public readonly Playfield Playfield;

        public SnapResult(Vector2 screenSpacePosition, double? time, Playfield playfield = null)
        {
            ScreenSpacePosition = screenSpacePosition;
            Time = time;
            Playfield = playfield;
        }
    }
}
