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
        /// The current hitsound timing of this hit object.
        /// 0 means at the start of the object, 1 means at the end of the object.
        /// </summary>
        double HitsoundTiming { get; }
    }
}
