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
        /// <remarks>
        /// Uses <a href="https://en.wikipedia.org/wiki/Algorithms_for_calculating_variance#Welford's_online_algorithm">Welford's online algorithm</a>.
        /// </remarks>
        /// <returns>
        /// A non-null <see langword="double"/> value if unstable rate could be calculated,
        /// and <see langword="null"/> if unstable rate cannot be calculated due to <paramref name="hitEvents"/> being empty.
        /// </returns>
        public static double? CalculateUnstableRate(this IEnumerable<HitEvent> hitEvents)
        {
            Debug.Assert(hitEvents.All(ev => ev.GameplayRate != null));

            int count = 0;
            double mean = 0;
            double sumOfSquares = 0;

            foreach (var e in hitEvents)
            {
                if (!affectsUnstableRate(e))
                    continue;

                count++;

                // Division by gameplay rate is to account for TimeOffset scaling with gameplay rate.
                double currentValue = e.TimeOffset / e.GameplayRate!.Value;
                double nextMean = mean + (currentValue - mean) / count;
                sumOfSquares += (currentValue - mean) * (currentValue - nextMean);
                mean = nextMean;
            }

            if (count == 0)
                return null;

            return 10.0 * Math.Sqrt(sumOfSquares / count);
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
    }
}
