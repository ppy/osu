// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Osu.Difficulty.Utils.Interpolation
{
    /// <summary>
    /// Helper functions used to approximate the derivative of a function using the difference quotient method:
    /// https://mathworld.wolfram.com/DifferenceQuotient.html
    /// </summary>
    internal static class ApproximateDerivative
    {
        /// <summary>
        /// Approximates the derivative (slope) of a one-dimensional function using the difference quotient method with two points.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The accuracy of the approximation rises as <paramref name="x0"/> and <paramref name="x1"/> get closer to each other,
        /// although floating-point rounding and cancellation errors have to also be considered as they affect accuracy if the difference
        /// of the two is small enough.
        /// </para>
        /// <para>
        /// For approximating the derivative at point <c>x</c>, it is recommended to use the points <c>x - h</c>
        /// and <c>x + h</c>, where <c>h</c> is a small quantity (approaching 0) for smaller error.
        /// If that is not possible, using <c>x</c> and <c>x + h</c> is also viable.
        /// </para>
        /// </remarks>
        /// <param name="x0">The <c>x</c> coordinate of the first point to base the approximation on.</param>
        /// <param name="v0">The value of the function to approximate the derivative for at <paramref name="x0"/>.</param>
        /// <param name="x1">The <c>x</c> coordinate of the second point to base the approximation on.</param>
        /// <param name="v1">The value of the function to approximate the derivative for at <paramref name="x1"/>.</param>
        /// <returns>
        /// The approximation of the derivative at the interval [<paramref name="x0"/>, <paramref name="x1"/>]
        /// calculated as the secant of the function graph passing through points (<paramref name="x0"/>, <paramref name="v0"/>)
        /// and (<paramref name="x1"/>, <paramref name="v1"/>).
        /// </returns>
        public static double FromTwoPoints(double x0, double v0, double x1, double v1)
        {
            return (v1 - v0) / (x1 - x0);
        }

        /// <summary>
        /// Approximates the derivative (slope) of a one-dimensional function using the difference quotient method with three points.
        /// </summary>
        /// <remarks>
        /// This method differs from <see cref="FromTwoPoints"/> in that it adds weighting based on the distances of <paramref name="x0"/>
        /// and <paramref name="x2"/> from the midpoint <paramref name="x1"/>.
        /// Therefore this method is best-equipped to approximate the derivative at point <paramref name="x1"/> if values at
        /// <paramref name="x0"/> and <paramref name="x2"/> are available.
        /// Over a uniform grid, however, just taking <see cref="FromTwoPoints"/> over the marginal points is preferable.
        /// </remarks>
        /// <param name="x0">The <c>x</c> coordinate of the leftmost point to base the approximation on.</param>
        /// <param name="v0">The value of the function to approximate the derivative for at <paramref name="x0"/>.</param>
        /// <param name="x1">The <c>x</c> coordinate of the midpoint to calculate the approximation for.</param>
        /// <param name="v1">The value of the function to approximate the derivative for at <paramref name="x1"/>.</param>
        /// <param name="x2">The <c>x</c> coordinate of the rightmost point to base the approximation on.</param>
        /// <param name="v2">The value of the function to approximate the derivative for at <paramref name="x2"/>.</param>
        /// <returns>
        /// The approximation of the derivative at <paramref name="x1"/>, calculated as the weighted average of two-point approximations
        /// using the marginal points.
        /// </returns>
        /// <returns></returns>
        public static double FromThreePoints(double x0, double v0, double x1, double v1, double x2, double v2)
        {
            return ((x2 - x1) * FromTwoPoints(x0, v0, x1, v1) + (x1 - x0) * FromTwoPoints(x1, v1, x2, v2)) / (x2 - x0);
        }
    }
}
