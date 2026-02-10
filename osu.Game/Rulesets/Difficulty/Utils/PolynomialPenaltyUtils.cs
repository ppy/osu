// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    /// <summary>
    /// Represents a polynomial fitted to a given set of points.
    /// </summary>
    public static class PolynomialPenaltyUtils
    {
        /// <summary>
        /// The proportions of skill that this polynomial will fit a curve to the miss counts of.
        /// If you want to change these, you need to recompute the <see cref="matrix"/> as it is precomputed to fit these specific values.
        /// </summary>
        public static readonly double[] SKILL_PROPORTIONS = [1, 0.95, 0.9, 0.8, 0.6, 0.3, 0];

        // Pre-calculated matrix used for curve fitting.
        // It's derived using least-squares regression to find the best-fit polynomial through our data points.
        private static readonly double[][] matrix =
        {
            new[] { 0.0, -25.8899, -32.6909, -11.9147, 48.8588, -26.8943, 0.0 },
            new[] { 0.0, 51.7787, 66.595, 28.8517, -90.3185, 40.9864, 0.0 },
            new[] { 0.0, -31.5028, -41.7398, -22.7118, 46.438, -15.5156, 0.0 }
        };

        /// <summary>
        /// Creates a polynomial curve that maps miss counts to miss penalties.
        /// Used to smoothly interpolate between miss counts, with 0 misses fixed to 0% penalty, and all misses fixed to 100% penalty.
        /// </summary>
        /// <param name="missCounts">
        /// A dictionary of miss counts, with keys representing skill proportions and values representing the miss count a player would achieve at that skill proportion.
        /// See comment on <see cref="SKILL_PROPORTIONS"/> if you want to use custom skill proportions.
        /// </param>
        public static double[] GetPenaltyCoefficients(Dictionary<double, double> missCounts)
        {
            double endPoint = missCounts.Values.Max();

            double[] sortedSkillProportions = missCounts.Keys.OrderByDescending(k => k).ToArray();

            double[] coefficients = new double[4];

            coefficients[3] = endPoint;

            // Now we dot product the adjusted miss counts with the matrix.
            for (int row = 0; row < matrix.Length; row++)
            {
                for (int column = 0; column < matrix[row].Length; column++)
                {
                    double skillProportion = sortedSkillProportions[column];
                    double missCountAtSkill = missCounts[skillProportion];

                    coefficients[row] += matrix[row][column] * (missCountAtSkill - endPoint * (1 - skillProportion));
                }

                coefficients[3] -= coefficients[row];
            }

            return coefficients;
        }

        /// <summary>
        /// Calculates what percentage penalty the player should receive.
        /// </summary>
        /// <param name="coefficients">The coefficients to achieve the penalty at</param>
        /// <param name="missCount">The number of misses the player got</param>
        /// <returns>A value between 0 and 1 representing the penalty percentage (0 = no penalty, 1 = full penalty)</returns>
        public static double GetPenaltyAt(double[] coefficients, double missCount)
        {
            // Our first coefficients are the ones derived from the skill proportion miss counts,
            // and subtracting missCount for the last one sets our root to the corresponding penalty.
            List<double> listCoefficients = [..coefficients, -missCount];

            List<double?> xVals = DifficultyCalculationUtils.SolvePolynomialRoots(listCoefficients);

            const double max_error = 1e-7;

            // This will never happen (it is physically impossible for there to not be a root),
            // but in the interest of sanity we fall back to a 100% penalty if no roots were found.
            double largestValue = xVals.Where(x => x >= 0 - max_error && x <= 1 + max_error).OrderDescending().FirstOrDefault() ?? 1;

            return Math.Clamp(largestValue, 0, 1);
        }
    }
}
