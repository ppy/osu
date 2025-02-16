// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    /// <summary>
    /// Represents a polynomial fitted to the logarithm of a given set of points.
    /// The resulting polynomial is exponentiated, ensuring low residuals for
    /// small inputs while handling exponentially increasing trends in the data.
    /// This approach is useful for modelling the results of decreasing skill with few coefficients,
    /// as linear decreases in skill correspond with exponential increases in miss counts.
    /// </summary>
    public struct ExpPolynomial
    {
        private double[]? coefficients;

        // The matrix that minimizes the square error at X values [0.0, 0.30, 0.60, 0.80, 0.90, 0.95, 1.0].
        private static readonly double[][] matrix =
        {
            new[] { 0.0, 3.14395, 5.18439, 6.46975, 1.4638, -9.53526, 0.0 },
            new[] { 0.0, -4.85829, -8.09612, -10.4498, -3.84479, 12.1626, 0.0 }
        };

        /// <summary>
        /// Computes the coefficients of a quartic polynomial, starting at 0 and ending at the highest miss count in the array.
        /// </summary>
        /// <param name="missCounts">A list of miss counts, with X values [1, 0.95, 0.9, 0.8, 0.6, 0.3, 0] corresponding to their skill levels.</param>
        public void Fit(double[] missCounts)
        {
            List<double> logMissCounts = missCounts.Select(x => Math.Log(x + 1)).ToList();

            double endPoint = logMissCounts.Max();

            double[] penalties = { 1, 0.95, 0.9, 0.8, 0.6, 0.3, 0 };

            coefficients = new double[3];

            coefficients[2] = endPoint;

            // Now we dot product the adjusted miss counts with the matrix.
            for (int row = 0; row < matrix.Length; row++)
            {
                for (int column = 0; column < matrix[row].Length; column++)
                {
                    coefficients[row] += matrix[row][column] * (logMissCounts[column] - endPoint * (1 - penalties[column]));
                }

                coefficients[2] -= coefficients[row];
            }
        }

        /// <summary>
        /// Solve for the miss penalty at a specified miss count.
        /// </summary>
        public double GetPenaltyAt(double missCount)
        {
            if (coefficients is null)
                return 1;

            List<double> listCoefficients = coefficients.ToList();
            listCoefficients.Add(-Math.Log(missCount + 1));

            List<double?> xVals = DifficultyCalculationUtils.SolvePolynomialRoots(listCoefficients);

            const double max_error = 1e-7;

            // We find the largest value of x (corresponding to the penalty) found as a root of the function, with a fallback of a 100% penalty if no roots were found.
            double largestValue = xVals.Where(x => x >= 0 - max_error && x <= 1 + max_error).OrderDescending().FirstOrDefault() ?? 1;

            return Math.Clamp(largestValue, 0, 1);
        }
    }
}
