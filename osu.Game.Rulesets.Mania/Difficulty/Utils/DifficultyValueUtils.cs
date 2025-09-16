// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Rulesets.Mania.Difficulty.Utils
{
    public static class DifficultyValueUtils
    {
        /// <summary>
        /// Calculates the mean of specific percentile values from a sorted array.
        /// This is used to get representative difficulty values from peak difficulty distributions.
        /// </summary>
        /// <param name="sortedPeaks">Array of difficulty peaks (must be sorted ascending)</param>
        /// <param name="percentiles">Array of percentile positions (0.0 to 1.0)</param>
        public static double CalculatePercentileMean(double[] sortedPeaks, double[] percentiles)
        {
            int peakCount = sortedPeaks.Length;
            if (peakCount == 0 || percentiles.Length == 0) return 0.0;

            double percentileSum = 0.0;
            int maxValidIndex = peakCount - 1;

            // Sum the values at each specified percentile
            for (int percentileIndex = 0; percentileIndex < percentiles.Length; percentileIndex++)
            {
                double percentilePosition = percentiles[percentileIndex];

                // Convert percentile to array index with proper rounding
                int targetIndex = (int)Math.Round(maxValidIndex * percentilePosition);

                // Ensure index is within valid bounds
                targetIndex = Math.Max(0, Math.Min(targetIndex, maxValidIndex));

                percentileSum += sortedPeaks[targetIndex];
            }

            return percentileSum / percentiles.Length;
        }

        /// <summary>
        /// Calculates the power mean (generalized mean) of an array of values.
        /// Power means are used to combine difficulty components with different emphasis.
        /// Higher lambda values emphasize larger values more strongly.
        /// </summary>
        public static double CalculatePowerMean(double[] values, double lambda)
        {
            if (values.Length == 0) return 0.0;

            int validValueCount = 0;
            double poweredSum = calculateGeneralPowerSum(values, lambda, ref validValueCount);
            return validValueCount > 0 ? Math.Pow(poweredSum / validValueCount, 1.0 / lambda) : 0.0;
        }

        /// <summary>
        /// General case calculation for arbitrary power sums.
        /// Uses Math.Pow for flexibility.
        /// </summary>
        private static double calculateGeneralPowerSum(double[] values, double lambda, ref int validValueCount)
        {
            double sum = 0.0;

            for (int i = 0; i < values.Length; i++)
            {
                double value = values[i];

                if (value > 0.0)
                {
                    sum += Math.Pow(value, lambda);
                    validValueCount++;
                }
            }

            return sum;
        }
    }
}
