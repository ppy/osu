// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A HitObject which has its hitsound at a specific time along its duration. Will be used for editor timeline display.
    /// </summary>
    public interface IHasHitsoundTiming : IHasDuration
    {
        /// <summary>
        /// The absolute time of when the hitsound occurs.
        /// Used for placement of the sample point in the editor timeline.
        /// </summary>
        double HitsoundTiming { get; }
    }
}
