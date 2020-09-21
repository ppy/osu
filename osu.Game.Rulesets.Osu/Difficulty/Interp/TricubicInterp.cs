using System.Collections.Generic;

namespace osu.Game.Rulesets.Osu.Difficulty.Interp
{
    public class TricubicInterp
    {
        public TricubicInterp(double[] x, double[] y, double[] z, double[][][] values,
            double dxLower = 0, double dxUpper = 0,
            double dyLower = 0, double dyUpper = 0,
            double dzLower = 0, double dzUpper = 0)
        {
            xArray = x;
            this.dxLower = dxLower;
            this.dxUpper = dxUpper;

            cubicInterps = new List<BicubicInterp>(x.Length);
            for (int i = 0; i < x.Length; ++i)
            {
                cubicInterps.Add(new BicubicInterp(y, z, values[i], dyLower, dyUpper, dzLower, dzUpper));
            }
        }

        public TricubicInterp(double[] x, double[] y, double[] z, double[,,] values,
            double dxLower = 0, double dxUpper = 0,
            double dyLower = 0, double dyUpper = 0,
            double dzLower = 0, double dzUpper = 0)
            : this(x, y, z, makeJagged(values), dxLower, dxUpper, dyLower, dyUpper, dzLower, dzUpper)
        {
        }

        private static double[][][] makeJagged(double[,,] values)
        {
            int xSize = values.GetLength(0);
            int ySize = values.GetLength(1);
            int zSize = values.GetLength(2);

            var result = new double[xSize][][];
            for (int i=0; i<xSize; ++i)
            {
                result[i] = new double[ySize][];
                for (int j=0; j<ySize; ++j)
                {
                    result[i][j] = new double[zSize];
                    for (int k = 0; k < zSize; ++k)
                    {
                        result[i][j][k] = values[i, j, k];
                    }
                }
            }

            return result;
        }

        public double Evaluate(double x, double y, double z)
        {
            int xIndex = SplineIndex(x);
            (int yIndex, int zIndex) = cubicInterps[0].SplineIndex(y, z);

            double x0 = xArray[xIndex];
            double x1 = xArray[xIndex + 1];

            double val0 = cubicInterps[xIndex].Evaluate(yIndex, zIndex, y, z);
            double val1 = cubicInterps[xIndex + 1].Evaluate(yIndex, zIndex, y, z);

            double d0, d1;
            if (xIndex == 0)
            {
                d0 = dxLower;
            }
            else
            {
                double xPrev = xArray[xIndex - 1];
                double valPrev = cubicInterps[xIndex - 1].Evaluate(yIndex, zIndex, y, z);
                d0 = CubicInterp.ThreePointDerivative(xPrev, valPrev, x0, val0, x1, val1);
            }
            if (xIndex == cubicInterps.Count - 2)
            {
                d1 = dxUpper;
            }
            else
            {
                double x2 = xArray[xIndex + 2];
                double val2 = cubicInterps[xIndex + 2].Evaluate(yIndex, zIndex, y, z);

                d1 = CubicInterp.ThreePointDerivative(x0, val0, x1, val1, x2, val2);
            }

            var spline = new HermiteSpline(x0, val0, d0, x1, val1, d1);
            return spline.Evaluate(x);
        }

        public int SplineIndex(double x)
        {
            int i = xArray.Length - 2;
            while (i > 0 && xArray[i] > x)
                --i;
            return i;
        }

        private double[] xArray;
        private double dxLower;
        private double dxUpper;
        private List<BicubicInterp> cubicInterps;
    }
}
