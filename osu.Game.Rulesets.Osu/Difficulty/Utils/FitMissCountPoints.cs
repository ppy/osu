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
            new[] { 0.0, -6.99428, -9.87548, -9.76922, -7.66867, -4.43461, -0.795376, 2.65313, 5.4474, 7.2564, 7.88146, 7.2564, 5.4474, 2.65313, -0.795376, -4.43461, -7.66867, -9.76922, -9.87548, -6.99428, 0.0 },
            new[] { 0.0, 13.0907, 18.2388, 17.6639, 13.3211, 6.90022, -0.173479, -6.73969, -11.9029, -15.0326, -15.7629, -13.993, -9.88668, -3.87281, 3.35498, 10.8382, 17.3536, 21.4129, 21.2632, 14.8864, 0.0 },
            new[] { 0.0, -7.21754, -9.85841, -9.24217, -6.5276, -2.71265, 1.36553, 5.03057, 7.76692, 9.21984, 9.19538, 7.66039, 4.74253, 0.730255, -3.92717, -8.61967, -12.5764, -14.8657, -14.395, -9.91114, 0.0 }
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

            double[] coefficients = new double[3];

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
