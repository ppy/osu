// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Beatmaps.Timing
{
    public class BreakPeriod
    {
        /// <summary>
        /// The minimum duration required for a break to have any effect.
        /// </summary>
        private const double min_break_duration = 650;

        /// <summary>
        /// The break start time.
        /// </summary>
        public double StartTime;

        /// <summary>
        /// The break end time.
        /// </summary>
        public double EndTime;

        /// <summary>
        /// The break duration.
        /// </summary>
        public double Duration => EndTime - StartTime;

        /// <summary>
        /// Whether the break has any effect. Breaks that are too short are culled before they are added to the beatmap.
        /// </summary>
        public bool HasEffect => Duration >= min_break_duration;
    }
}
