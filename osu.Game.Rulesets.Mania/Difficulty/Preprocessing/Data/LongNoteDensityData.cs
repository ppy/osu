// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Data
{
    public class LongNoteDensityData
    {
        /// <summary>Time points where density values change</summary>
        public required double[] TimePoints;

        /// <summary>Density values at each time point</summary>
        public required double[] DensityValues;

        /// <summary>Cumulative sum for efficient range queries</summary>
        public required double[] CumulativeSum;

        /// <summary>
        /// Calculates the total density contribution between two time points.
        /// This is used to determine how much long note influence exists in a time range.
        /// </summary>
        public double SumBetween(int startTime, int endTime)
        {
            if (TimePoints.Length == 0) return 0.0;

            int startIndex = findTimeIndex(startTime);
            int endIndex = findTimeIndex(endTime);

            if (startIndex == endIndex)
            {
                // Range falls within a single density segment
                return (endTime - startTime) * DensityValues[startIndex];
            }

            double totalContribution = 0.0;

            // Add contribution from the first partial segment
            totalContribution += (TimePoints[startIndex + 1] - startTime) * DensityValues[startIndex];

            // Add contributions from complete segments in between
            for (int i = startIndex + 1; i < endIndex; i++)
                totalContribution += (TimePoints[i + 1] - TimePoints[i]) * DensityValues[i];

            // Add contribution from the last partial segment
            totalContribution += (endTime - TimePoints[endIndex]) * DensityValues[endIndex];

            return totalContribution;
        }

        /// <summary>
        /// Finds the index of the density segment that contains the given time.
        /// Uses binary search for efficient lookup.
        /// </summary>
        private int findTimeIndex(int time)
        {
            int index = Array.BinarySearch(TimePoints, time);
            if (index < 0) index = ~index - 1;
            return Math.Max(0, Math.Min(index, DensityValues.Length - 1));
        }
    }
}
