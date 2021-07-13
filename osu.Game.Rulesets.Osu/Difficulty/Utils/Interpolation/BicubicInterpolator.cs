// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Difficulty.Utils.Interpolation
{
    /// <summary>
    /// Performs bicubic interpolation of a two-dimensional function, using a collection of Hermite splines.
    /// Interpolation nodes are placed on a 2D grid.
    /// </summary>
    public class BicubicInterpolator
    {
        /// <summary>
        /// The <c>x</c> values of the 2D grid points to perform bicubic interpolation over.
        /// </summary>
        private readonly double[] xs;

        /// <summary>
        /// The value of the partial derivative over <c>x</c> for all points with the smallest <c>x</c> coordinate value on the 2D grid.
        /// If missing, will be calculated separately for each edge point on the grid.
        /// </summary>
        private readonly double? dxLower;

        /// <summary>
        /// The value of the partial derivative over <c>x</c> for all points with the largest <c>x</c> coordinate value on the 2D grid.
        /// If missing, will be calculated separately for each edge point on the grid.
        /// </summary>
        private readonly double? dxUpper;

        /// <summary>
        /// Array of 1D cubic interpolators, performing interpolation over slices of the 2D function.
        /// </summary>
        private readonly CubicInterpolator[] cubicInterpolators;

        /// <summary>
        /// Creates a new <see cref="BicubicInterpolator"/> using the supplied data.
        /// </summary>
        /// <remarks>
        /// Marginal partial derivative values, if not supplied in
        /// <paramref name="dxLower"/>, <paramref name="dxUpper"/>,
        /// <paramref name="dyLower"/>, <paramref name="dyUpper"/>,
        /// will be calculated independently for each point of the grid.
        /// </remarks>
        /// <param name="xs">1D array with the <c>x</c> coordinates of the interpolation grid nodes.</param>
        /// <param name="ys">1D array with the <c>y</c> coordinates of the interpolation grid nodes.</param>
        /// <param name="vs">
        /// 2D array with the values of the function for each node of the grid specified
        /// by <paramref name="xs"/> and <paramref name="ys"/>.
        /// </param>
        /// <param name="dxLower">The value of the partial derivative over <c>x</c> for all points with the smallest <c>x</c> coordinate value on the 2D grid.</param>
        /// <param name="dxUpper">The value of the partial derivative over <c>x</c> for all points with the largest <c>x</c> coordinate value on the 2D grid.</param>
        /// <param name="dyLower">The value of the partial derivative over <c>y</c> for all points with the smallest <c>y</c> coordinate value on the 2D grid.</param>
        /// <param name="dyUpper">The value of the partial derivative over <c>y</c> for all points with the largest <c>y</c> coordinate value on the 2D grid.</param>
        public BicubicInterpolator(double[] xs, double[] ys, double[][] vs,
                                   double? dxLower = null, double? dxUpper = null,
                                   double? dyLower = null, double? dyUpper = null)
        {
            this.xs = xs;
            this.dxLower = dxLower;
            this.dxUpper = dxUpper;

            cubicInterpolators = new CubicInterpolator[xs.Length];

            for (int i = 0; i < xs.Length; ++i)
            {
                cubicInterpolators[i] = new CubicInterpolator(ys, vs[i], dyLower, dyUpper);
            }
        }

        /// <summary>
        /// Returns the 2D index of the spline segment within which the point (<paramref name="x"/>, <paramref name="y"/>) lies.
        /// </summary>
        /// <param name="x">The <c>x</c> coordinate of the point to get the spline index for.</param>
        /// <param name="y">The <c>y</c> coordinate of the point to get the spline index for.</param>
        public (int xIndex, int yIndex) SplineSegmentAt(double x, double y)
        {
            int xIndex = xs.Length - 2;

            while (xIndex > 0 && xs[xIndex] > x)
                --xIndex;

            return (xIndex, cubicInterpolators[0].SplineSegmentAt(y));
        }

        /// <summary>
        /// Evaluates the bicubic Hermite spline at the point (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="x">The <c>x</c> coordinate of the point to get the interpolated value for.</param>
        /// <param name="y">The <c>y</c> coordinate of the point to get the interpolated value for.</param>
        public double Evaluate(double x, double y)
        {
            (int xIndex, int yIndex) = SplineSegmentAt(x, y);

            return Evaluate(xIndex, yIndex, x, y);
        }

        /// <summary>
        /// Evaluates the bicubic Hermite spline segment at indices (<paramref name="xIndex"/>, <paramref name="yIndex"/>)
        /// at the point (<paramref name="x"/>, <paramref name="y"/>).
        /// </summary>
        /// <param name="xIndex">The <c>x</c> index of the spline segment.</param>
        /// <param name="yIndex">The <c>y</c> index of the spline segment.</param>
        /// <param name="x">The <c>x</c> coordinate of the point to get the interpolated value for.</param>
        /// <param name="y">The <c>y</c> coordinate of the point to get the interpolated value for.</param>
        public double Evaluate(int xIndex, int yIndex, double x, double y)
        {
            double x1 = xs[xIndex];
            double x2 = xs[xIndex + 1];

            double v1 = cubicInterpolators[xIndex].Evaluate(yIndex, y);
            double v2 = cubicInterpolators[xIndex + 1].Evaluate(yIndex, y);

            double d1, d2;

            if (xIndex == 0)
            {
                // one of the marginal segments on the left - have to make do with only x1 and x2 to get the left-side partial derivative.
                d1 = dxLower ?? ApproximateDerivative.FromTwoPoints(x1, v1, x2, v2);
            }
            else
            {
                // there's a segment to the left, so we can use it to approximate the left-side derivative better.
                double x0 = xs[xIndex - 1];
                double v0 = cubicInterpolators[xIndex - 1].Evaluate(yIndex, y);
                d1 = ApproximateDerivative.FromThreePoints(x0, v0, x1, v1, x2, v2);
            }

            if (xIndex == cubicInterpolators.Length - 2)
            {
                // one of the marginal segments on the right - have to make do with only x1 and x2 to get the right-side partial derivative.
                d2 = dxUpper ?? ApproximateDerivative.FromTwoPoints(x1, v1, x2, v2);
            }
            else
            {
                // there's a segment to the right, so we can use it to approximate the right-side derivative better.
                double x3 = xs[xIndex + 2];
                double v3 = cubicInterpolators[xIndex + 2].Evaluate(yIndex, y);
                d2 = ApproximateDerivative.FromThreePoints(x1, v1, x2, v2, x3, v3);
            }

            var spline = new HermiteSplineSegment(x1, v1, d1, x2, v2, d2);

            return spline.Evaluate(x);
        }
    }
}
