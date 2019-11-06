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

            if (mt * tp > 100)
                return 1.0;

            if (mt <= 0.03)
                mt = 0.03;

            return SpecialFunctions.Erf(2.066 / d * (Math.Pow(2, (mt * tp)) - 1) / Math.Sqrt(2));
        }
    }
}
