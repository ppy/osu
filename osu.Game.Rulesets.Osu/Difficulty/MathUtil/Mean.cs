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
    }


    
}
