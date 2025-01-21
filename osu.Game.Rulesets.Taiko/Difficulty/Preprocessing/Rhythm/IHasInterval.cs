// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Difficulty.Preprocessing.Rhythm
{
    /// <summary>
    /// The interface for hitobjects that provide an interval value.
    /// </summary>
    public interface IHasInterval
    {
        /// <summary>
        /// The interval between 2 objects start times.
        /// </summary>
        double Interval { get; }
    }
}
