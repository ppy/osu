// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    /// <summary>
    /// Helper methods to calculate the power mean of N numbers:
    /// https://mathworld.wolfram.com/PowerMean.html
    /// </summary>
    internal static class PowerMean
    {
        /// <summary>
        /// Calculates the power mean of degree <paramref name="p"/> of two real numbers:
        /// <paramref name="x"/> and <paramref name="y"/>.
        /// </summary>
        /// <param name="x">The first number.</param>
        /// <param name="y">The second number.</param>
        /// <param name="p">The degree of the power mean to calculate.</param>
        public static double Of(double x, double y, double p) =>
            Math.Pow((Math.Pow(x, p) + Math.Pow(y, p)) / 2, 1 / p);

        /// <summary>
        /// Calculates the power mean of degree <paramref name="p"/> of <paramref name="values"/>.
        /// </summary>
        /// <param name="values">A sequence of real numbers to calculate the power mean of.</param>
        /// <param name="p">The degree of the power mean to calculate.</param>
        public static double Of(IEnumerable<double> values, double p)
        {
            double sum = 0;
            int count = 0;

            foreach (var x in values)
            {
                sum += Math.Pow(x, p);
                count++;
            }

            return Math.Pow(sum / count, 1 / p);
        }
    }
}
