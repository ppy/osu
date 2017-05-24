// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Mania.Timing
{
    public class TimingChange
    {
        /// <summary>
        /// The time at which this timing change happened.
        /// </summary>
        public double Time;

        /// <summary>
        /// The beat length.
        /// </summary>
        public double BeatLength = 500;

        /// <summary>
        /// The speed multiplier.
        /// </summary>
        public double SpeedMultiplier = 1;
    }
}