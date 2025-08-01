// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Edit.Checks.Components
{
    public static class TimingCheckUtils
    {
        // Small tolerance for floating point comparison
        public const double TIMING_TOLERANCE = 0.01;

        /// <summary>
        /// Finds a timing control point that starts at approximately the same time (within 1ms after rounding).
        /// </summary>
        /// <param name="timingPoints">The collection of timing points to search.</param>
        /// <param name="time">The time to match against.</param>
        /// <returns>The matching timing control point, or null if none found.</returns>
        public static TimingControlPoint? FindMatchingTimingPoint(IEnumerable<TimingControlPoint> timingPoints, double time)
        {
            return timingPoints.FirstOrDefault(tp => Precision.AlmostEquals(tp.Time, Math.Round(time), 1.0));
        }

        /// <summary>
        /// Finds a timing control point that starts at precisely the same time (within timing tolerance).
        /// </summary>
        /// <param name="timingPoints">The collection of timing points to search.</param>
        /// <param name="time">The time to match against.</param>
        /// <returns>The exact matching timing control point, or null if none found.</returns>
        public static TimingControlPoint? FindExactMatchingTimingPoint(IEnumerable<TimingControlPoint> timingPoints, double time)
        {
            return timingPoints.FirstOrDefault(tp => Precision.AlmostEquals(tp.Time, time, TIMING_TOLERANCE));
        }
    }
}
