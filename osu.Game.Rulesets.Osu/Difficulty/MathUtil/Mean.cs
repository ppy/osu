using System;
using System.Collections.Generic;
using System.Text;

namespace osu.Game.Rulesets.Osu.Difficulty.MathUtil
{
    class Mean
    {
        public Mean()
        {
        }

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
