// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Data
{
    /// <summary>
    /// Represents density contribution from long notes over time.
    /// Stores time points where density changes and corresponding density values.
    /// </summary>
    public class LongNoteDensityData
    {
        /// <summary>
        /// Time points where density values change (must be sorted).
        /// </summary>
        public required double[] TimePoints;

        /// <summary>
        /// Density values corresponding to each time point in <see cref="TimePoints"/>.
        /// </summary>
        public required double[] DensityValues;

        /// <summary>
        /// Calculates total density contribution between two time points.
        /// Integrates density values over the specified time range.
        /// </summary>
        /// <returns>Total density contribution in the time range</returns>
        public double SumBetween(int startTime, int endTime)
        {
            if (TimePoints.Length == 0) return 0.0;

            int startIndex = findTimeIndex(startTime);
            int endIndex = findTimeIndex(endTime);

            // Single segment case
            if (startIndex == endIndex)
            {
                return (endTime - startTime) * DensityValues[startIndex];
            }

            double totalContribution = 0.0;

            // First partial segment
            totalContribution += (TimePoints[startIndex + 1] - startTime) * DensityValues[startIndex];

            // Full segments in between
            for (int i = startIndex + 1; i < endIndex; i++)
                totalContribution += (TimePoints[i + 1] - TimePoints[i]) * DensityValues[i];

            // Last partial segment
            totalContribution += (endTime - TimePoints[endIndex]) * DensityValues[endIndex];

            return totalContribution;
        }

        /// <summary>
        /// Finds the density segment index containing the specified time.
        /// </summary>
        /// <returns>Index of the density segment containing the time</returns>
        private int findTimeIndex(int time)
        {
            int index = Array.BinarySearch(TimePoints, time);
            if (index < 0) index = ~index - 1;
            return Math.Max(0, Math.Min(index, DensityValues.Length - 1));
        }
    }
}
