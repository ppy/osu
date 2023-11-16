// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace osu.Game.Rulesets.Scoring
{
    public static class HitEventExtensions
    {
        /// <summary>
        /// Calculates the "unstable rate" for a sequence of <see cref="HitEvent"/>s.
        /// </summary>
        /// <returns>
        /// A non-null <see langword="double"/> value if unstable rate could be calculated,
        /// and <see langword="null"/> if unstable rate cannot be calculated due to <paramref name="hitEvents"/> being empty.
        /// </returns>
        public static double? CalculateUnstableRate(this IEnumerable<HitEvent> hitEvents)
        {
            Debug.Assert(!hitEvents.Any(ev => ev.GameplayRate == null));

            // Division by gameplay rate is to account for TimeOffset scaling with gameplay rate.
            double[] timeOffsets = hitEvents.Where(affectsUnstableRate).Select(ev => ev.TimeOffset / ev.GameplayRate!.Value).ToArray();
            return 10 * standardDeviation(timeOffsets);
        }

        /// <summary>
        /// Calculates the average hit offset/error for a sequence of <see cref="HitEvent"/>s, where negative numbers mean the user hit too early on average.
        /// </summary>
        /// <returns>
        /// A non-null <see langword="double"/> value if unstable rate could be calculated,
        /// and <see langword="null"/> if unstable rate cannot be calculated due to <paramref name="hitEvents"/> being empty.
        /// </returns>
        public static double? CalculateAverageHitError(this IEnumerable<HitEvent> hitEvents)
        {
            double[] timeOffsets = hitEvents.Where(affectsUnstableRate).Select(ev => ev.TimeOffset).ToArray();

            if (timeOffsets.Length == 0)
                return null;

            return timeOffsets.Average();
        }

        private static bool affectsUnstableRate(HitEvent e) => !(e.HitObject.HitWindows is HitWindows.EmptyHitWindows) && e.Result.IsHit();

        private static double? standardDeviation(double[] timeOffsets)
        {
            if (timeOffsets.Length == 0)
                return null;

            double mean = timeOffsets.Average();
            double squares = timeOffsets.Select(offset => Math.Pow(offset - mean, 2)).Sum();
            return Math.Sqrt(squares / timeOffsets.Length);
        }
    }
}
