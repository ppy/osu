// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Utils;

namespace osu.Game.Rulesets.Difficulty.Aggregation
{
    public static class HarmonicSeries
    {
        public static (double difficulty, double weigthSum) Aggregate(List<double> difficulties, double harmonicScale = 1.0, double decayExponent = 0.9)
        {
            if (difficulties.Count == 0)
                return (0, 0);

            double difficulty = 0;
            int index = 0;
            double objectWeightSum = 0;

            foreach (double obj in difficulties.OrderDescending().Where(v => v > 0))
            {
                // Use a harmonic sum that considers each object of the map according to a predefined weight.
                double weight = (1 + (harmonicScale / (1 + index))) / (DiffUtils.Pow(index, decayExponent) + 1 + (harmonicScale / (1 + index)));

                objectWeightSum += weight;

                difficulty += obj * weight;
                index += 1;
            }

            return (difficulty, objectWeightSum);
        }
    }
}
