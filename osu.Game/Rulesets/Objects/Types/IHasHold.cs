// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Objects.Types
{
    /// <summary>
    /// A special type of HitObject, mostly used for legacy conversion of "holds".
    /// </summary>
    public interface IHasHold
    {
        /// <summary>
        /// The time at which the hold ends.
        /// </summary>
        double EndTime { get; }
    }
}
