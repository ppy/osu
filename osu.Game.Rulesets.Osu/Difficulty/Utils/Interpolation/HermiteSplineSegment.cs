// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Difficulty.Utils.Interpolation
{
    /// <summary>
    /// Represents a segment of a cubic Hermite spline.
    /// </summary>
    /// <remarks>
    /// On a single interval, the cubic Hermite spline segment is constructed using values of function being interpolated
    /// on both ends of the interval, as well as the values of its derivative at said both ends.
    /// </remarks>
    internal readonly struct HermiteSplineSegment
    {
        /// <summary>
        /// The left endpoint of this spline segment.
        /// </summary>
        public readonly double X0;

        /// <summary>
        /// The value of the interpolated function at <see cref="X0"/>.
        /// </summary>
        private readonly double v0;

        /// <summary>
        /// The value of the derivative of the interpolated function at <see cref="X0"/>.
        /// </summary>
        private readonly double d0;

        /// <summary>
        /// The right endpoint of this spline segment.
        /// </summary>
        public readonly double X1;

        /// <summary>
        /// The value of the interpolated function at <see cref="X1"/>.
        /// </summary>
        private readonly double v1;

        /// <summary>
        /// The value of the derivative of the interpolated function at <see cref="X1"/>.
        /// </summary>
        private readonly double d1;

        /// <summary>
        /// The coefficients of the Hermite cubic spline.
        /// </summary>
        private readonly double c0, c1, c2, c3;

        /// <summary>
        /// Constructs a segment of the Hermite spline over a single one-dimensional interval.
        /// </summary>
        /// <param name="x0">The left endpoint of the interpolation interval.</param>
        /// <param name="v0">The value of the interpolated function at <paramref name="x0"/>.</param>
        /// <param name="d0">The value of the derivative of the interpolated function at <paramref name="x0"/>.</param>
        /// <param name="x1">The right endpoint of the interpolation interval.</param>
        /// <param name="v1">The value of the interpolated function at <paramref name="x1"/>.</param>
        /// <param name="d1">The value of the derivative of the interpolated function at <paramref name="x1"/>.</param>
        public HermiteSplineSegment(double x0, double v0, double d0, double x1, double v1, double d1)
        {
            // scaling factor used to transform the [x0, x1] interval onto the unit interval [0, 1]
            double scale = 1 / (x1 - x0);
            double scaleSquared = scale * scale;

            X0 = x0;
            this.v0 = v0;
            this.d0 = d0;

            X1 = x1;
            this.v1 = v1;
            this.d1 = d1;

            // xref: https://mathworld.wolfram.com/CubicSpline.html
            // note that the coefficients are pre-scaled to avoid rescaling to the unit interval repeatedly in Evaluate()
            // also note that the derivatives are purposefully scaled one degree less
            c0 = v0;
            c1 = d0;
            c2 = (3 * (v1 - v0) * scale - (2 * d0 + d1)) * scale;
            c3 = (2 * (v0 - v1) * scale + d0 + d1) * scaleSquared;
        }

        /// <summary>
        /// Evaluates this segment of the Hermite spline for the given argument <paramref name="x"/>.
        /// </summary>
        /// <remarks>
        /// If a value outside of the range [<see cref="X0"/>, <see cref="X1"/>] is given, a linear extrapolation
        /// using the marginal function and derivative values will be performed.
        /// </remarks>
        /// <param name="x">The argument to perform interpolation for.</param>
        /// <returns>The value of the interpolated function at <paramref name="x"/>.</returns>
        public double Evaluate(double x)
        {
            if (x > X1)
                return (x - X1) * d1 + v1;

            if (x < X0)
                return (x - X0) * d0 + v0;

            double t = x - X0;
            double tSquared = t * t;
            double tCubed = tSquared * t;

            return c0 + c1 * t + c2 * tSquared + c3 * tCubed;
        }
    }
}
