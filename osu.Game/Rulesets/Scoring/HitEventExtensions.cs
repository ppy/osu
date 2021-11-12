// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
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
            double[] timeOffsets = hitEvents.Where(affectsUnstableRate).Select(ev => ev.TimeOffset).ToArray();
            return 10 * standardDeviation(timeOffsets);
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
