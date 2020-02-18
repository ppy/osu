using System;

using MathNet.Numerics;

namespace osu.Game.Rulesets.Osu.Difficulty.MathUtil
{
    class FittsLaw
    {
        public FittsLaw()
        {
        }

        public static double CalculateIP(double relativeD, double mt)
        {
            return Math.Log(relativeD + 1, 2) / (mt + 1e-10);
        }

        public static double CalculateHitProb(double d, double mt, double tp)
        {
            if (d == 0)
                return 1.0;

            if (mt * tp > 50)
                return 1.0;

            if (mt <= 0.03)
                mt = 0.03;

            return SpecialFunctions.Erf(2.066 / d * (Power2(mt * tp) - 1) / Math.Sqrt(2));
        }

        /// <summary>
        /// Fast approximation of 2^x. Accurate to around 9-10 significant figures, around 6x faster than Math.Pow or Math.Exp
        /// Calculates the integer part using a bit shift and fraction part using a polynomial approximation
        /// </summary>
        public static double Power2(double x)
        {
            if (x<0)
            {
                return 1 / Power2(-x);
            }
            if (x>60)
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

        readonly static double[] coeffs = {
            1.0000000060371126,
            0.693146840098149,
            0.2402310826131064,
            0.05547894683131716,
            0.009686150703032881,
            0.0012382531241478965,
            0.00021871427263121524,
        };
    }



}
