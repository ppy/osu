// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy
{
    /// <summary>
    /// The type of pattern to generate. Used for legacy patterns.
    /// </summary>
    [Flags]
    internal enum PatternType
    {
        None = 0,

        /// <summary>
        /// Keep the same as last row.
        /// </summary>
        ForceStack = 1,

        /// <summary>
        /// Keep different from last row.
        /// </summary>
        ForceNotStack = 1 << 1,

        /// <summary>
        /// Keep as single note at its original position.
        /// </summary>
        KeepSingle = 1 << 2,

        /// <summary>
        /// Use a lower random value.
        /// </summary>
        LowProbability = 1 << 3,

        /// <summary>
        /// Reserved.
        /// </summary>
        Alternate = 1 << 4,

        /// <summary>
        /// Ignore the repeat count.
        /// </summary>
        ForceSigSlider = 1 << 5,

        /// <summary>
        /// Convert slider to circle.
        /// </summary>
        ForceNotSlider = 1 << 6,

        /// <summary>
        /// Notes gathered together.
        /// </summary>
        Gathered = 1 << 7,
        Mirror = 1 << 8,

        /// <summary>
        /// Change 0 -> 6.
        /// </summary>
        Reverse = 1 << 9,

        /// <summary>
        /// 1 -> 5 -> 1 -> 5 like reverse.
        /// </summary>
        Cycle = 1 << 10,

        /// <summary>
        /// Next note will be at column + 1.
        /// </summary>
        Stair = 1 << 11,

        /// <summary>
        /// Next note will be at column - 1.
        /// </summary>
        ReverseStair = 1 << 12
    }
}
