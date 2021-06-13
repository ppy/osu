// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    /// <summary>
    /// Helper methods used to approximate exponential functions.
    /// </summary>
    internal static class FastExponent
    {
        /// <summary>
        /// The coefficients of the polynomial approximating the exponential function.
        /// </summary>
        /// <remarks>
        /// The coefficients below were fit using a least-squares approximation algorithm.
        /// They are roughly equal to the first seven terms of the Taylor series expansion for 2<sup>x</sup> at x = 0,
        /// but aren't exactly the same as the error of the Taylor expansion gets bigger further away from 0
        /// (the perturbations below give better approximations over the [0,1] interval, within which this function
        /// will be primarily used).
        /// </remarks>
        private static readonly double[] coefficients =
        {
            1.0000000060371126,
            0.693146840098149,
            0.2402310826131064,
            0.05547894683131716,
            0.009686150703032881,
            0.0012382531241478965,
            0.00021871427263121524,
        };

        /// <summary>
        /// Calculates a fast approximation of the exponential function base 2 (2<sup>x</sup>).
        /// </summary>
        /// <remarks>
        /// Accurate to around 9-10 significant figures.
        /// Yields about a 15-20% reduction in runtime when compared to <see cref="System.Math.Pow"/>.
        /// The integral part is calculated using a bit-shift, while the fractional part uses
        /// the Taylor polynomial expansion of 2<sup>x</sup>.
        /// Values above 2<sup>60</sup> are coerced to infinity.
        /// </remarks>
        /// <param name="x">The argument for the exponential function.</param>
        public static double Exp2(double x)
        {
            if (x < 0)
                return 1 / Exp2(-x);

            if (x > 60)
                return double.PositiveInfinity;

            // integral part of the argument
            int floor = (int)x;

            // fractional part of the argument (and its consecutive powers)
            double frac = x - floor;
            double frac2 = frac * frac;
            double frac3 = frac * frac2;
            double frac4 = frac * frac3;
            double frac5 = frac * frac4;
            double frac6 = frac * frac5;

            long integral = 1L << floor;
            double fractional = coefficients[0]
                                + coefficients[1] * frac
                                + coefficients[2] * frac2
                                + coefficients[3] * frac3
                                + coefficients[4] * frac4
                                + coefficients[5] * frac5
                                + coefficients[6] * frac6;

            // identity used: 2^(a + b) = 2^a * 2^b
            return integral * fractional;
        }
    }
}
