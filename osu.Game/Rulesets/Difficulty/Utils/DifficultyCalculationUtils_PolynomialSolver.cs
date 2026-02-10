// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Utils;

namespace osu.Game.Rulesets.Difficulty.Utils
{
    public partial class DifficultyCalculationUtils
    {
        private const double pi_mult_2 = 6.28318530717958647692528676655900576d;

        /// <summary>
        /// Solve for the exact real roots of any polynomial up to degree 4.
        /// </summary>
        /// <param name="coefficients">The coefficients of the polynomial, in ascending order ([1, 3, 5] -> x^2 + 3x + 5).</param>
        /// <returns>The real roots of the polynomial, and null if the root does not exist.</returns>
        public static List<double?> SolvePolynomialRoots(List<double> coefficients)
        {
            List<double?> xVals = new List<double?>();

            switch (coefficients.Count)
            {
                case 5:
                    xVals = solveP4(coefficients[0], coefficients[1], coefficients[2], coefficients[3], coefficients[4], out int _).ToList();
                    break;

                case 4:
                    xVals = solveP3(coefficients[0], coefficients[1], coefficients[2], coefficients[3], out int _).ToList();
                    break;

                case 3:
                    xVals = solveP2(coefficients[0], coefficients[1], coefficients[2], out int _).ToList();
                    break;

                case 2:
                    xVals = solveP2(0, coefficients[1], coefficients[2], out int _).ToList();
                    break;
            }

            return xVals;
        }

        // https://github.com/sasamil/Quartic/blob/master/quartic.cpp
        private static double?[] solveP4(double a, double b, double c, double d, double e, out int nRoots)
        {
            double?[] xVals = new double?[4];

            nRoots = 0;

            if (a == 0)
            {
                double?[] xValsCubic = solveP3(b, c, d, e, out nRoots);

                xVals[0] = xValsCubic[0];
                xVals[1] = xValsCubic[1];
                xVals[2] = xValsCubic[2];
                xVals[3] = null;

                return xVals;
            }

            b /= a;
            c /= a;
            d /= a;
            e /= a;

            double a3 = -c;
            double b3 = b * d - 4 * e;
            double c3 = -b * b * e - d * d + 4 * c * e;

            double?[] x3 = solveP3(1, a3, b3, c3, out int iZeroes);

            double q1, q2, p1, p2, sqD;

            double y = x3[0]!.Value;

            // Get the y value with the highest absolute value.
            if (iZeroes != 1)
            {
                if (Math.Abs(x3[1]!.Value) > Math.Abs(y))
                    y = x3[1]!.Value;
                if (Math.Abs(x3[2]!.Value) > Math.Abs(y))
                    y = x3[2]!.Value;
            }

            double upperD = y * y - 4 * e;

            if (Precision.AlmostEquals(upperD, 0))
            {
                q1 = q2 = y * 0.5;

                upperD = b * b - 4 * (c - y);

                if (Precision.AlmostEquals(upperD, 0))
                    p1 = p2 = b * 0.5;

                else
                {
                    sqD = Math.Sqrt(upperD);
                    p1 = (b + sqD) * 0.5;
                    p2 = (b - sqD) * 0.5;
                }
            }
            else
            {
                sqD = Math.Sqrt(upperD);
                q1 = (y + sqD) * 0.5;
                q2 = (y - sqD) * 0.5;

                p1 = (b * q1 - d) / (q1 - q2);
                p2 = (d - b * q2) / (q1 - q2);
            }

            // solving quadratic eq. - x^2 + p1*x + q1 = 0
            upperD = p1 * p1 - 4 * q1;

            if (upperD >= 0)
            {
                nRoots += 2;

                sqD = Math.Sqrt(upperD);
                xVals[0] = (-p1 + sqD) * 0.5;
                xVals[1] = (-p1 - sqD) * 0.5;
            }

            // solving quadratic eq. - x^2 + p2*x + q2 = 0
            upperD = p2 * p2 - 4 * q2;

            if (upperD >= 0)
            {
                nRoots += 2;

                sqD = Math.Sqrt(upperD);
                xVals[2] = (-p2 + sqD) * 0.5;
                xVals[3] = (-p2 - sqD) * 0.5;
            }

            // Put the null roots at the end of the array.
            var nonNulls = xVals.Where(x => x != null);
            var nulls = xVals.Where(x => x == null);
            xVals = nonNulls.Concat(nulls).ToArray();

            return xVals;
        }

        private static double?[] solveP3(double a, double b, double c, double d, out int nRoots)
        {
            double?[] xVals = new double?[3];

            nRoots = 0;

            if (a == 0)
            {
                double?[] xValsQuadratic = solveP2(b, c, d, out nRoots);

                xVals[0] = xValsQuadratic[0];
                xVals[1] = xValsQuadratic[1];
                xVals[2] = null;

                return xVals;
            }

            b /= a;
            c /= a;
            d /= a;

            double a2 = b * b;
            double q = (a2 - 3 * c) / 9;
            double q3 = q * q * q;
            double r = (b * (2 * a2 - 9 * c) + 27 * d) / 54;
            double r2 = r * r;

            if (r2 < q3)
            {
                nRoots = 3;

                double t = r / Math.Sqrt(q3);
                t = Math.Clamp(t, -1, 1);
                t = Math.Acos(t);
                b /= 3;
                q = -2 * Math.Sqrt(q);

                xVals[0] = q * Math.Cos(t / 3) - b;
                xVals[1] = q * Math.Cos((t + pi_mult_2) / 3) - b;
                xVals[2] = q * Math.Cos((t - pi_mult_2) / 3) - b;

                return xVals;
            }

            double upperA = -Math.Cbrt(Math.Abs(r) + Math.Sqrt(r2 - q3));

            if (r < 0)
                upperA = -upperA;

            double upperB = upperA == 0 ? 0 : q / upperA;
            b /= 3;

            xVals[0] = upperA + upperB - b;

            if (Precision.AlmostEquals(0.5 * Math.Sqrt(3) * (upperA - upperB), 0))
            {
                nRoots = 2;
                xVals[1] = -0.5 * (upperA + upperB) - b;

                return xVals;
            }

            nRoots = 1;

            return xVals;
        }

        private static double?[] solveP2(double a, double b, double c, out int nRoots)
        {
            double?[] xVals = new double?[2];

            nRoots = 0;

            if (a == 0)
            {
                if (b == 0)
                    return xVals;

                nRoots = 1;
                xVals[0] = -c / b;
            }

            double discriminant = b * b - 4 * a * c;

            switch (discriminant)
            {
                case < 0:
                    break;

                case 0:
                    nRoots = 1;
                    xVals[0] = -b / (2 * a);
                    break;

                default:
                    nRoots = 2;
                    xVals[0] = (-b + Math.Sqrt(discriminant)) / (2 * a);
                    xVals[1] = (-b - Math.Sqrt(discriminant)) / (2 * a);
                    break;
            }

            return xVals;
        }
    }
}
