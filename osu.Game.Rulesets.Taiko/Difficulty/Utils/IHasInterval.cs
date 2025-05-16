// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Taiko.Difficulty.Utils
{
    /// <summary>
    /// The interface for objects that provide an interval value.
    /// </summary>
    public interface IHasInterval
    {
        /// <summary>
        /// The interval – ie delta time – between this object and a known previous object.
        /// </summary>
        double Interval { get; }
    }
}
