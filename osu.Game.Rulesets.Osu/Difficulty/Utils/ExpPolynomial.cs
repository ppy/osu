// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    public struct ExpPolynomial
    {
        private double[]? coefficients;

        private static readonly double[][] matrix =
        {
            new[] { 0.0, 3.14395, 5.18439, 6.46975, 1.4638, -9.53526, 0.0 },
            new[] { 0.0, -4.85829, -8.09612, -10.4498, -3.84479, 12.1626, 0.0 }
        };

        /// <summary>
        /// Computes a quartic or cubic function that starts at 0 and ends at the highest judgement count in the array.
        /// </summary>
        /// <param name="judgementCounts">A list of judgements, with X values [0.0, 0.05, ..., 0.95, 1.0].</param>
        public void Fit(double[] judgementCounts)
        {
            List<double> logMissCounts = judgementCounts.Select(x => Math.Log(x + 1)).ToList();

            // The polynomial will pass through the point (1, endPoint).
            double endPoint = logMissCounts.Max();

            double[] penalties = { 1, 0.95, 0.9, 0.8, 0.6, 0.3, 0 };

            for (int i = 0; i < logMissCounts.Count; i++)
            {
                logMissCounts[i] -= endPoint * (1 - penalties[i]);
            }

            // The precomputed matrix assumes the misscounts go in order of greatest to least.
            logMissCounts.Reverse();

            coefficients = new double[3];

            coefficients[2] = endPoint;

            // Now we dot product the adjusted misscounts with the precomputed matrix.
            for (int row = 0; row < matrix.Length; row++)
            {
                for (int column = 0; column < matrix[row].Length; column++)
                {
                    coefficients[row] += matrix[row][column] * logMissCounts[column];
                }

                coefficients[2] -= coefficients[row];
            }
        }

        /// <summary>
        /// Solve for the miss penalty at a specified miss count.
        /// </summary>
        /// <returns>The penalty value at the specified miss count.</returns>
        public double GetPenaltyAt(double missCount)
        {
            if (coefficients is null)
                return 1;

            List<double> listCoefficients = coefficients.ToList();
            listCoefficients.Add(-Math.Log(missCount + 1));

            List<double?> xVals = SpecialFunctions.SolvePolynomialRoots(listCoefficients);

            const double max_error = 1e-7;
            double? largestValue = xVals.Where(x => x >= 0 - max_error && x <= 1 + max_error).OrderDescending().FirstOrDefault();

            return largestValue != null ? Math.Clamp(largestValue.Value, 0, 1) : 1;
        }
    }
}
