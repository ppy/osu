// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Difficulty.Utils.Interpolation
{
    /// <summary>
    /// Performs tricubic interpolation of a three-dimensional function, using a collection of Hermite splines.
    /// Interpolation nodes are placed on a 3D grid.
    /// </summary>
    internal class TricubicInterpolator
    {
        /// <summary>
        /// The <c>x</c> values of the 3D grid points to perform tricubic interpolation over.
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
        /// Array of 2D bicubic interpolators, performing interpolation over slices of the 3D function.
        /// </summary>
        private readonly BicubicInterpolator[] bicubicInterpolators;

        /// <summary>
        /// Creates a new <see cref="TricubicInterpolator"/> using the supplied data.
        /// </summary>
        /// <remarks>
        /// Marginal partial derivative values, if not supplied in
        /// <paramref name="dxLower"/>, <paramref name="dxUpper"/>,
        /// <paramref name="dyLower"/>, <paramref name="dyUpper"/>,
        /// <paramref name="dzLower"/>, <paramref name="dzUpper"/>,
        /// will be calculated independently for each point of the grid.
        /// </remarks>
        /// <param name="xs">1D array with the <c>x</c> coordinates of the interpolation grid nodes.</param>
        /// <param name="ys">1D array with the <c>y</c> coordinates of the interpolation grid nodes.</param>
        /// <param name="zs">1D array with the <c>z</c> coordinates of the interpolation grid nodes.</param>
        /// <param name="vs">
        /// 3D array with the values of the function for each node of the grid specified
        /// by <paramref name="xs"/>, <paramref name="ys"/> and <paramref name="zs"/>.
        /// </param>
        /// <param name="dxLower">The value of the partial derivative over <c>x</c> for all points with the smallest <c>x</c> coordinate value on the 2D grid.</param>
        /// <param name="dxUpper">The value of the partial derivative over <c>x</c> for all points with the largest <c>x</c> coordinate value on the 2D grid.</param>
        /// <param name="dyLower">The value of the partial derivative over <c>y</c> for all points with the smallest <c>y</c> coordinate value on the 2D grid.</param>
        /// <param name="dyUpper">The value of the partial derivative over <c>y</c> for all points with the largest <c>y</c> coordinate value on the 2D grid.</param>
        /// <param name="dzLower">The value of the partial derivative over <c>z</c> for all points with the smallest <c>z</c> coordinate value on the 2D grid.</param>
        /// <param name="dzUpper">The value of the partial derivative over <c>z</c> for all points with the largest <c>z</c> coordinate value on the 2D grid.</param>
        public TricubicInterpolator(double[] xs, double[] ys, double[] zs, double[,,] vs,
                                    double? dxLower = 0, double? dxUpper = 0,
                                    double? dyLower = 0, double? dyUpper = 0,
                                    double? dzLower = 0, double? dzUpper = 0)
            : this(xs, ys, zs, vs.ToJagged(), dxLower, dxUpper, dyLower, dyUpper, dzLower, dzUpper)
        {
        }

        private TricubicInterpolator(double[] xs, double[] ys, double[] zs, double[][][] vs,
                                     double? dxLower = null, double? dxUpper = null,
                                     double? dyLower = null, double? dyUpper = null,
                                     double? dzLower = null, double? dzUpper = null)
        {
            this.xs = xs;
            this.dxLower = dxLower;
            this.dxUpper = dxUpper;

            bicubicInterpolators = new BicubicInterpolator[xs.Length];

            for (int i = 0; i < xs.Length; ++i)
            {
                bicubicInterpolators[i] = new BicubicInterpolator(ys, zs, vs[i], dyLower, dyUpper, dzLower, dzUpper);
            }
        }

        /// <summary>
        /// Returns the 3D index of the spline segment within which the point
        /// (<paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>) lies.
        /// </summary>
        /// <param name="x">The <c>x</c> coordinate of the point to get the spline index for.</param>
        /// <param name="y">The <c>y</c> coordinate of the point to get the spline index for.</param>
        /// <param name="z">The <c>z</c> coordinate of the point to get the spline index for.</param>
        private (int xIndex, int yIndex, int zIndex) splineSegmentAt(double x, double y, double z)
        {
            int xIndex = xs.Length - 2;

            while (xIndex > 0 && xs[xIndex] > x)
                --xIndex;

            (int yIndex, int zIndex) = bicubicInterpolators[xIndex].SplineSegmentAt(y, z);
            return (xIndex, yIndex, zIndex);
        }

        /// <summary>
        /// Evaluates the tricubic Hermite spline at the point (<paramref name="x"/>, <paramref name="y"/>, <paramref name="z"/>).
        /// </summary>
        /// <param name="x">The <c>x</c> coordinate of the point to get the interpolated value for.</param>
        /// <param name="y">The <c>y</c> coordinate of the point to get the interpolated value for.</param>
        /// <param name="z">The <c>z</c> coordinate of the point to get the interpolated value for.</param>
        public double Evaluate(double x, double y, double z)
        {
            (int xIndex, int yIndex, int zIndex) = splineSegmentAt(x, y, z);

            double x1 = xs[xIndex];
            double x2 = xs[xIndex + 1];

            double v1 = bicubicInterpolators[xIndex].Evaluate(yIndex, zIndex, y, z);
            double v2 = bicubicInterpolators[xIndex + 1].Evaluate(yIndex, zIndex, y, z);

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
                double v0 = bicubicInterpolators[xIndex - 1].Evaluate(yIndex, zIndex, y, z);
                d1 = ApproximateDerivative.FromThreePoints(x0, v0, x1, v1, x2, v2);
            }

            if (xIndex == bicubicInterpolators.Length - 2)
            {
                // one of the marginal segments on the right - have to make do with only x1 and x2 to get the right-side partial derivative.
                d2 = dxUpper ?? ApproximateDerivative.FromTwoPoints(x1, v1, x2, v2);
            }
            else
            {
                // there's a segment to the right, so we can use it to approximate the right-side derivative better.
                double x3 = xs[xIndex + 2];
                double v3 = bicubicInterpolators[xIndex + 2].Evaluate(yIndex, zIndex, y, z);
                d2 = ApproximateDerivative.FromThreePoints(x1, v1, x2, v2, x3, v3);
            }

            var spline = new HermiteSplineSegment(x1, v1, d1, x2, v2, d2);
            return spline.Evaluate(x);
        }
    }
}
