// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Screens.Ranking.Statistics
{
    /// <summary>
    /// Displays the unstable rate statistic for a given play.
    /// </summary>
    public class UnstableRate : SimpleStatisticItem<double>
    {
        /// <summary>
        /// Creates and computes an <see cref="UnstableRate"/> statistic.
        /// </summary>
        /// <param name="hitEvents">Sequence of <see cref="HitEvent"/>s to calculate the unstable rate based on.</param>
        public UnstableRate(IEnumerable<HitEvent> hitEvents)
            : base("Unstable Rate")
        {
            double[] timeOffsets = hitEvents.Where(e => !(e.HitObject.HitWindows is HitWindows.EmptyHitWindows) && e.Result.IsHit())
                                            .Select(ev => ev.TimeOffset).ToArray();
            Value = 10 * standardDeviation(timeOffsets);
        }

        private static double standardDeviation(double[] timeOffsets)
        {
            if (timeOffsets.Length == 0)
                return double.NaN;

            double mean = timeOffsets.Average();
            double squares = timeOffsets.Select(offset => Math.Pow(offset - mean, 2)).Sum();
            return Math.Sqrt(squares / timeOffsets.Length);
        }

        protected override string DisplayValue(double value) => double.IsNaN(value) ? "(not available)" : value.ToString("N2");
    }
}
