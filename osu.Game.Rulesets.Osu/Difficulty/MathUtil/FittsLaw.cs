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

        readonly static double[] coeffs = {
            1.0000000060371126,
            0.693146840098149,
            0.2402310826131064,
            0.05547894683131716,
            0.009686150703032881,
            0.0012382531241478965,
            0.00021871427263121524,
        };
        public static double Power2(double x)
        {
            int floor = (int)x;
            double frac = x - floor;
            double frac2 = frac * frac;
            double frac3 = frac * frac2;
            double frac4 = frac * frac3;
            double frac5 = frac * frac4;
            double frac6 = frac * frac4;

            return (1L << floor) * (coeffs[0] + coeffs[1] * frac + coeffs[2] * frac2
                + coeffs[3] * frac3 + coeffs[4] * frac4 + coeffs[5] * frac5 + coeffs[6] * frac6);
        }

        /*
        readonly static double[] coeffs = {
            1.0000072153251425,
            0.6929318839596742,
            0.24170895290576638,
            0.051667593566359506,
            0.01367667229501924,
        };
        public static double Power2(double x)
        {
            int floor = (int)x;
            double frac = x - floor;
            double frac2 = frac * frac;
            double frac3 = frac * frac2;
            double frac4 = frac * frac3;

            return (1L << floor) * (coeffs[0] + coeffs[1] * frac + coeffs[2] * frac2
                + coeffs[3] * frac3 + coeffs[4] * frac4);
        }
        */
    }



}
