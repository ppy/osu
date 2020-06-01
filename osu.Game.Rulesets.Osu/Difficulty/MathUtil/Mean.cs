// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Difficulty.MathUtil
{
    public static class Mean
    {
        public static double PowerMean(double x, double y, double i)
        {
            return Math.Pow((Math.Pow(x, i) + Math.Pow(y, i)) / 2,
                            1 / i);
        }

        public static double PowerMean(IEnumerable<double> values, double i)
        {
            double sum = 0;
            int count = 0;
            foreach (var x in values)
            {
                sum += Math.Pow(x, i);
                count++;
            }
            return Math.Pow(sum / count, 1 / i);
        }
    }
}
