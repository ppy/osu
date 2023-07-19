// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    public static class DifficultyFunctions
    {
        /// <summary>
        /// Calculates the sum of (values[i] * weight^i)
        /// </summary>
        public static double ExponentialWeightedSum(this IEnumerable<double> values, double weight)
        {
            double cumulativeWeight = 1;
            double total = 0;

            foreach (double value in values)
            {
                total += value * cumulativeWeight;
                cumulativeWeight *= weight;
            }

            return total;
        }

        /// <summary>
        /// Calculates the sum of (values[i] * weight^i) after sorting values
        /// </summary>
        public static double SortedExponentialWeightedSum(this IEnumerable<double> values, double weight)
        {
            return values.Where(x => x > 0).OrderByDescending(x => x).ExponentialWeightedSum(weight);
        }

        public static double ApplyDecay(double value, double elapsedTimeMillis, double decayMultiplierPerSecond)
        {
            return value * Math.Pow(decayMultiplierPerSecond, elapsedTimeMillis / 1000);
        }
    }
}
