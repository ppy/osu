// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps.Timing;

namespace osu.Game.Rulesets.Mania.Timing
{
    /// <summary>
    /// A point in the map where the beat length or speed multiplier has changed .
    /// </summary>
    public class TimingSection
    {
        /// <summary>
        /// The time at which the change occurred.
        /// </summary>
        public double StartTime;
        /// <summary>
        /// The duration of this timing section - lasts until the next timing section.
        /// </summary>
        public double Duration;
        /// <summary>
        /// The beat length, includes any speed multiplier.
        /// </summary>
        public double BeatLength;
        /// <summary>
        /// The time signature of this timing section.
        /// </summary>
        public TimeSignatures TimeSignature;
    }
}