// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Timing
{
    public class TimingSection
    {
        /// <summary>
        /// The time in milliseconds at which this timing section starts.
        /// </summary>
        public double Time;

        /// <summary>
        /// The length of one beat in milliseconds.
        /// </summary>
        public double BeatLength = 500;

        /// <summary>
        /// An arbitrary speed multiplier which should be used to when adjusting the visual representation of entities represented by this section.
        /// This is usually applied in adition to a multiplier based on the <see cref="BeatLength"/> relative to a constant.
        /// </summary>
        public double SpeedMultiplier = 1;
    }
}