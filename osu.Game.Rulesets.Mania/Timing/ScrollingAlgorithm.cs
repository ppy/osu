// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Mania.Timing
{
    public enum ScrollingAlgorithm
    {
        /// <summary>
        /// Basic scrolling algorithm based on the timing section time. This is the default algorithm.
        /// </summary>
        Basic,
        /// <summary>
        /// Emulating a form of gravity where hit objects speed up over time.
        /// </summary>
        Gravity
    }
}
