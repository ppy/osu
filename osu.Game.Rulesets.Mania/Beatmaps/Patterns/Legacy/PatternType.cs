// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
        ForceNotStack = 2,
        /// <summary>
        /// Keep as single note at its original position.
        /// </summary>
        KeepSingle = 4,
        /// <summary>
        /// Use a lower random value.
        /// </summary>
        LowProbability = 8,
        /// <summary>
        /// Reserved.
        /// </summary>
        Alternate = 16,
        /// <summary>
        /// Ignore the repeat count.
        /// </summary>
        ForceSigSlider = 32,
        /// <summary>
        /// Convert slider to circle.
        /// </summary>
        ForceNotSlider = 64,
        /// <summary>
        /// Notes gathered together.
        /// </summary>
        Gathered = 128,
        Mirror = 256,
        /// <summary>
        /// Change 0 -> 6.
        /// </summary>
        Reverse = 512,
        /// <summary>
        /// 1 -> 5 -> 1 -> 5 like reverse.
        /// </summary>
        Cycle = 1024,
        /// <summary>
        /// Next note will be at column + 1.
        /// </summary>
        Stair = 2048,
        /// <summary>
        /// Next note will be at column - 1.
        /// </summary>
        ReverseStair = 4096
    }
}
