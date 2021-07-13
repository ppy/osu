// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils.Interpolation
{
    /// <summary>
    /// Performs cubic interpolation of a one-dimensional function, using a Hermite spline.
    /// </summary>
    internal class CubicInterpolator
    {
        /// <summary>
        /// An array of all segments of the Hermite spline used for interpolation.
        /// </summary>
        private readonly HermiteSplineSegment[] splineSegments;

        /// <summary>
        /// Creates a new <see cref="CubicInterpolator"/> using the supplied data.
        /// </summary>
        /// <param name="xs">The arguments of the function being interpolated.</param>
        /// <param name="vs">The values of the function being interpolated. Must match <paramref name="xs"/> in length.</param>
        /// <param name="lowerBoundDerivative">
        /// The derivative of the function at the first point from <paramref name="xs"/>.
        /// Will be approximated using the first and second points from <paramref name="xs"/> and <paramref name="vs"/> if not given.
        /// </param>
        /// <param name="upperBoundDerivative">
        /// The derivative of the function at the last point from <paramref name="xs"/>.
        /// Will be approximated using the second-to-last and last points from <paramref name="xs"/> and <paramref name="vs"/> if not given.
        /// </param>
        public CubicInterpolator(double[] xs, double[] vs, double? lowerBoundDerivative = null, double? upperBoundDerivative = null)
        {
            Debug.Assert(xs.Length == vs.Length);
            double[] derivatives = new double[vs.Length];

            for (int i = 1; i < xs.Length - 1; ++i)
            {
                derivatives[i] = ApproximateDerivative.FromThreePoints(xs[i - 1], vs[i - 1], xs[i], vs[i], xs[i + 1], vs[i + 1]);
            }

            int last = xs.Length - 1;
            derivatives[0] = lowerBoundDerivative ?? ApproximateDerivative.FromTwoPoints(xs[0], vs[0], xs[1], vs[1]);
            derivatives[last] = upperBoundDerivative ?? ApproximateDerivative.FromTwoPoints(xs[last], vs[last], xs[last - 1], vs[last - 1]);

            splineSegments = new HermiteSplineSegment[xs.Length - 1];

            for (int i = 0; i < xs.Length - 1; ++i)
            {
                splineSegments[i] = new HermiteSplineSegment(xs[i], vs[i], derivatives[i], xs[i + 1], vs[i + 1], derivatives[i + 1]);
            }
        }

        /// <summary>
        /// Returns the index of the spline segment within which <paramref name="x"/> lies.
        /// </summary>
        /// <param name="x">The coordinate of the point to get the spline index for.</param>
        public int SplineSegmentAt(double x)
        {
            int i = splineSegments.Length - 1;

            while (i > 0 && splineSegments[i].X0 > x)
                i--;

            return i;
        }

        /// <summary>
        /// Evaluates the cubic Hermite spline at the point <paramref name="x"/>.
        /// </summary>
        /// <param name="x">The coordinate of the point to get the interpolated value for.</param>
        public double Evaluate(double x)
        {
            return splineSegments[SplineSegmentAt(x)].Evaluate(x);
        }

        /// <summary>
        /// Evaluates the cubic Hermite spline segment with the given <paramref name="index"/> at point <paramref name="x"/>.
        /// </summary>
        /// <param name="index">The index of the spline segment to evaluate value of.</param>
        /// <param name="x">The coordinate of the point to get the interpolated value for.</param>
        public double Evaluate(int index, double x)
        {
            return splineSegments[index].Evaluate(x);
        }
    }
}
