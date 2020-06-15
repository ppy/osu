// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MathNet.Numerics;

namespace osu.Game.Rulesets.Osu.Difficulty.MathUtil
{
    public static class FittsLaw
    {
        /// <summary>
        /// Calculates the index of performance for the distance and the movement time specified.
        /// Index of performance is the difficulty of a movement.
        /// </summary>
        public static double CalculateIp(double d, double mt)
        {
            return Math.Log(d + 1, 2) / (mt + 1e-10);
        }

        /// <summary>
        /// Calculates the probability that the target is hit successfully.
        /// </summary>
        public static double CalculateHitProb(double d, double mt, double tp)
        {
            if (d == 0)
                return 1.0;

            if (mt * tp > 50)
                return 1.0;

            if (mt <= 0.03)
                mt = 0.03;

            return SpecialFunctions.Erf(2.066 / d * (Exp2(mt * tp) - 1) / Math.Sqrt(2));
        }
 
        // calculated using python:
        // import numpy as np
        // x=np.linspace(0,1,1000)
        // np.polyfit(x,2**x,6)
        private static readonly double[] coeffs =
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
        /// Fast approximation of 2^x. Accurate to around 9-10 significant figures, several times faster than Math.Pow or Math.Exp
        /// Calculates the integer part using a bit shift and fraction part using a polynomial approximation
        /// </summary>
        public static double Exp2(double x)
        {
            // 2020-06-15 whole program benchmark for command: PerformanceCalculator.exe difficulty map_folder
            // implementation                  time (ms)           avg     percent
            // --------------------------------------------------------------------
            // Exp2 (this function)            31627 31606 31439   31557   100.00 %
            // Exp2Loop (same but with loop)   32241 32219 32441   32300   102.35 %
            // Math.Exp(Math.Log(2)*x)         34506 34543 34484   34511   109.36 %
            // Math.Pow(2,x)                   38248 38598 38038   38294   121.35 %

            if (x < 0)
            {
                return 1 / Exp2(-x);
            }

            if (x > 60)
            {
                return double.PositiveInfinity;
            }

            int floor = (int)x;
            double frac = x - floor;
            double frac2 = frac * frac;
            double frac3 = frac * frac2;
            double frac4 = frac * frac3;
            double frac5 = frac * frac4;
            double frac6 = frac * frac5;

            return (1L << floor) * (coeffs[0] + coeffs[1] * frac + coeffs[2] * frac2
                                    + coeffs[3] * frac3 + coeffs[4] * frac4 + coeffs[5] * frac5 + coeffs[6] * frac6);
        }
    }
}
