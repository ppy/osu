// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils.Interpolation
{
    public class BicubicInterp
    {
        public BicubicInterp(double[] x, double[] y, double[][] values,
            double? dxLower = null, double? dxUpper = null,
            double? dyLower = null, double? dyUpper = null)
        {
            xArray = x;
            this.dxLower = dxLower;
            this.dxUpper = dxUpper;

            cubicInterps = new List<CubicInterp>(x.Length);
            for (int i = 0; i < x.Length; ++i)
            {
                cubicInterps.Add(new CubicInterp(y, values[i], dyLower, dyUpper));
            }
        }

        public double Evaluate(double x, double y)
        {
            (int xIndex, int yIndex) = SplineIndex(x, y);

            return Evaluate(xIndex, yIndex, x, y);

        }

        public double Evaluate(int xIndex, int yIndex, double x, double y)
        {
            double x0 = xArray[xIndex];
            double x1 = xArray[xIndex + 1];

            double val0 = cubicInterps[xIndex].Evaluate(yIndex, y);
            double val1 = cubicInterps[xIndex + 1].Evaluate(yIndex, y);

            double d0, d1;
            if (xIndex == 0)
            {
                d0 = dxLower ?? ApproximateDerivative.FromTwoPoints(x0, val0, x1, val1);
            }
            else
            {
                double xPrev = xArray[xIndex - 1];
                double valPrev = cubicInterps[xIndex - 1].Evaluate(yIndex, y);
                d0 = ApproximateDerivative.FromThreePoints(xPrev, valPrev, x0, val0, x1, val1);
            }
            if (xIndex == cubicInterps.Count - 2)
            {
                d1 = dxUpper ?? ApproximateDerivative.FromTwoPoints(x0, val0, x1, val1);
            }
            else
            {
                double x2 = xArray[xIndex + 2];
                double val2 = cubicInterps[xIndex + 2].Evaluate(yIndex, y);

                d1 = ApproximateDerivative.FromThreePoints(x0, val0, x1, val1, x2, val2);
            }

            var spline = new HermiteSpline(x0, val0, d0, x1, val1, d1);

            return spline.Evaluate(x);
        }

        public (int, int) SplineIndex(double x, double y)
        {
            int xIndex = xArray.Length - 2;
            while (xIndex > 0 && xArray[xIndex] > x)
                --xIndex;
            return (xIndex, cubicInterps[0].SplineIndex(y));
        }

        private double[] xArray;
        private double? dxLower;
        private double? dxUpper;
        private List<CubicInterp> cubicInterps;
    }
}
