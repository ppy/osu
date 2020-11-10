// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MathNet.Numerics;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
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

            return SpecialFunctions.Erf(2.066 / d * (FastExponent.Exp2(mt * tp) - 1) / Math.Sqrt(2));
        }
    }
}
