// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Screens.Play;

namespace osu.Game.Beatmaps.Timing
{
    public class BreakPeriod
    {
        /// <summary>
        /// The minimum duration required for a break to have any effect.
        /// </summary>
        public const double MIN_BREAK_DURATION = 650;

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
        /// Whether the break has any effect.
        /// </summary>
        public bool HasEffect => Duration >= MIN_BREAK_DURATION;

        /// <summary>
        /// Constructs a new break period.
        /// </summary>
        /// <param name="startTime">The start time of the break period.</param>
        /// <param name="endTime">The end time of the break period.</param>
        public BreakPeriod(double startTime, double endTime)
        {
            StartTime = startTime;
            EndTime = endTime;
        }

        /// <summary>
        /// Whether this break contains a specified time.
        /// </summary>
        /// <param name="time">The time to check in milliseconds.</param>
        /// <returns>Whether the time falls within this <see cref="BreakPeriod"/>.</returns>
        public bool Contains(double time) => time >= StartTime && time <= EndTime - BreakOverlay.BREAK_FADE_DURATION;
    }
}
