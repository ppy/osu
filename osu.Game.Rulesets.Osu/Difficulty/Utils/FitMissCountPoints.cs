// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    public class FitMissCountPoints
    {
        // A few operations are precomputed on the Vandermonde matrix of skill values (0, 0.05, 0.1, ..., 0.95, 1).
        // This is to make regression simpler and less resource heavy. https://discord.com/channels/546120878908506119/1203154944492830791/1232333184306512002
        private static double[][] precomputedOperationsMatrix => new[]
        {
            new[] { 0.0, -0.897868, -1.5122, -1.8745, -2.01626, -1.96901, -1.76423, -1.43344, -1.00813, -0.519818, 3.55271e-15, 0.519818, 1.00813, 1.43344, 1.76423, 1.96901, 2.01626, 1.8745, 1.5122, 0.897868, 0.0 },
            new[] { 0.0, 1.27555, 2.1333, 2.62049, 2.78439, 2.67226, 2.33134, 1.8089, 1.1522, 0.408475, -0.375002, -1.15098, -1.8722, -2.49141, -2.96135, -3.23476, -3.2644, -3.00299, -2.4033, -1.41805, 0.0 },
        };

        public static double[] GetPolynomialCoefficients(double[] missCounts)
        {
            double[] adjustedMissCounts = missCounts;

            // The polynomial will pass through the point (1, maxMisscount).
            double maxMissCount = missCounts.Max();

            for (int i = 0; i <= 20; i++)
            {
                adjustedMissCounts[i] -= maxMissCount * i / 20;
            }

            // The precomputed matrix assumes the misscounts go in order of greatest to least.
            // Temporary fix.
            adjustedMissCounts = adjustedMissCounts.Reverse().ToArray();

            double[] coefficients = new double[2];

            // Now we dot product the adjusted misscounts with the precomputed matrix.
            for (int row = 0; row < precomputedOperationsMatrix.Length; row++)
            {
                for (int column = 0; column < precomputedOperationsMatrix[row].Length; column++)
                {
                    coefficients[row] += precomputedOperationsMatrix[row][column] * adjustedMissCounts[column];
                }
            }

            return coefficients;
        }
    }
}
