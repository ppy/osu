// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;

namespace osu.Game.Rulesets.Osu.Difficulty.Interp
{
    /// <summary>
    /// Finite difference cubic hermite interpolation
    /// </summary>
    public class CubicInterp
    {
        public static double TwoPointDerivative(double x, double val, double xNext, double valNext)
        {
            return (valNext - val) / (xNext - x);
        }

        public static double ThreePointDerivative(double xPrev, double valPrev, double x, double val, double xNext, double valNext)
        {
            return (
                (xNext - x) * TwoPointDerivative(xPrev, valPrev, x, val)
                + (x - xPrev) * TwoPointDerivative(x, val, xNext, valNext)
                ) / (xNext - xPrev);
        }

        public CubicInterp(double[] x, double[] values, double? lowerBoundDerivative = null, double? upperBoundDerivative = null)
        {
            Debug.Assert(x.Length == values.Length);
            double[] derivatives = new double[values.Length];
            for (int i = 1; i < x.Length - 1; ++i)
            {
                derivatives[i] = ThreePointDerivative(x[i - 1], values[i - 1], x[i], values[i], x[i + 1], values[i + 1]);
            }
            int last = x.Length - 1;
            derivatives[0] = lowerBoundDerivative ?? TwoPointDerivative(x[0], values[0], x[1], values[1]);
            derivatives[last] = upperBoundDerivative ?? TwoPointDerivative(x[last], values[last], x[last - 1], values[last - 1]);

            splines = new List<HermiteSpline>(x.Length);

            for (int i = 0; i < x.Length - 1; ++i)
            {
                splines.Add(new HermiteSpline(x[i], values[i], derivatives[i], x[i + 1], values[i + 1], derivatives[i + 1]));
            }
        }

        public int SplineIndex(double x)
        {
            int i = splines.Count - 1;

            while (i > 0 && splines[i].X0 > x)
                i--;
            return i;
        }

        public double Evaluate(double x)
        {
            return splines[SplineIndex(x)].Evaluate(x);
        }

        public double Evaluate(int index, double x)
        {
            return splines[index].Evaluate(x);
        }

        private List<HermiteSpline> splines;
    }
}
