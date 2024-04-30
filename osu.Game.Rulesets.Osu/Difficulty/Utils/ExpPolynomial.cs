// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    public struct ExpPolynomial
    {
        private static double[]? coefficients;

        // The product of this matrix with 21 computed points at X values [0.0, 0.05, ..., 0.95, 1.0] returns the least squares fit polynomial coefficients.
        private static double[][] quarticMatrix => new[]
        {
            new[] { 0.0, -6.99428, -9.87548, -9.76922, -7.66867, -4.43461, -0.795376, 2.65313, 5.4474, 7.2564, 7.88146, 7.2564, 5.4474, 2.65313, -0.795376, -4.43461, -7.66867, -9.76922, -9.87548, -6.99428, 0.0 },
            new[] { 0.0, 13.0907, 18.2388, 17.6639, 13.3211, 6.90022, -0.173479, -6.73969, -11.9029, -15.0326, -15.7629, -13.993, -9.88668, -3.87281, 3.35498, 10.8382, 17.3536, 21.4129, 21.2632, 14.8864, 0.0 },
            new[] { 0.0, -7.21754, -9.85841, -9.24217, -6.5276, -2.71265, 1.36553, 5.03057, 7.76692, 9.21984, 9.19538, 7.66039, 4.74253, 0.730255, -3.92717, -8.61967, -12.5764, -14.8657, -14.395, -9.91114, 0.0 }
        };

        private static double[][] cubicMatrix => new[]
        {
            new[] { 0.0, -0.897868, -1.5122, -1.8745, -2.01626, -1.96901, -1.76423, -1.43344, -1.00813, -0.519818, 3.55271e-15, 0.519818, 1.00813, 1.43344, 1.76423, 1.96901, 2.01626, 1.8745, 1.5122, 0.897868, 0.0 },
            new[] { 0.0, 1.27555, 2.1333, 2.62049, 2.78439, 2.67226, 2.33134, 1.8089, 1.1522, 0.408475, -0.375002, -1.15098, -1.8722, -2.49141, -2.96135, -3.23476, -3.2644, -3.00299, -2.4033, -1.41805, 0.0 },
        };

        /// <summary>
        /// Computes a quartic or cubic function that starts at 0 and ends at the highest judgement count in the array.
        /// </summary>
        /// <param name="judgementCounts">A list of judgements, with X values [0.0, 0.05, ..., 0.95, 1.0].</param>
        /// <param name="degree">The degree of the polynomial. Only supports cubic and quintic functions.</param>
        public void Compute(double[] judgementCounts, int degree)
        {
            if (degree != 3 && degree != 4)
                return;

            double[] adjustedMissCounts = judgementCounts;

            // The polynomial will pass through the point (1, maxMisscount).
            double maxMissCount = judgementCounts.Max();

            for (int i = 0; i <= 20; i++)
            {
                adjustedMissCounts[i] -= maxMissCount * i / 20;
            }

            // The precomputed matrix assumes the misscounts go in order of greatest to least.
            // Temporary fix.
            adjustedMissCounts = adjustedMissCounts.Reverse().ToArray();

            double[][] matrix = degree == 4 ? quarticMatrix : cubicMatrix;
            coefficients = new double[degree];

            coefficients[degree - 1] = maxMissCount;

            // Now we dot product the adjusted misscounts with the precomputed matrix.
            for (int row = 0; row < matrix.Length; row++)
            {
                for (int column = 0; column < matrix[row].Length; column++)
                {
                    coefficients[row] += matrix[row][column] * adjustedMissCounts[column];
                }

                coefficients[degree - 1] -= coefficients[row];
            }
        }

        /// <summary>
        /// Solve for the largest corresponding x value of a polynomial within x = 0 and x = 1 at a specified y value.
        /// </summary>
        /// <param name="y">A value between 0 and 1, inclusive, to solve the polynomial at.</param>
        /// <returns>The x value at the specified y value, and null if no value exists.</returns>
        public double? SolveBetweenZeroAndOne(double y)
        {
            if (coefficients is null)
                return null;

            List<double> listCoefficients = coefficients.ToList();
            listCoefficients.Add(-Math.Log(y + 1));

            List<double?> xVals = SpecialFunctions.SolvePolynomialRoots(listCoefficients);

            const double max_error = 1e-7;
            double? largestValue = xVals.Where(x => x >= 0 - max_error && x <= 1 + max_error).OrderDescending().FirstOrDefault();

            return largestValue != null ? Math.Clamp(largestValue.Value, 0, 1) : null;
        }
    }
}
