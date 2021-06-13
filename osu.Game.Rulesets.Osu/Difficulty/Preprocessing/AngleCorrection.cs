// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Game.Rulesets.Osu.Difficulty.Utils.Interpolation;

namespace osu.Game.Rulesets.Osu.Difficulty.Preprocessing
{
    /// <summary>
    /// Class responsible for the calculation of angle-based corrections for difficulty of movement pairs.
    /// </summary>
    /// <remarks>
    /// These utilities aim to approximate the difficulty of particular movement pairs
    /// (pairs of circle-to-circle straight-line movements performed by the players).
    /// There are two kinds of movement pairs to consider:
    /// <list type="bullet">
    /// <item>the movement pair of 2nd last -> last -> target object,</item>
    /// <item>the movement pair of last -> target -> next object,</item>
    /// </list>
    /// as well as two disparate types of aim:
    /// <list type="bullet">
    /// <item>flow aim, in which the user moves the pointer at a steady, roughly constant pace,</item>
    /// <item>snap aim, in which the user rapidly snaps the pointer to the target point in a burst movement.</item>
    /// </list>
    /// Based on the length of each movement of the pair, as well as the angles of the movements involved,
    /// instances of this class interpolate the expected correction in difficulty for a given movement pair.
    /// </remarks>
    internal class AngleCorrection
    {
        /// <summary>
        /// An interpolator approximating the 3D correction function.
        /// Arguments are: previous movement distance, other movement distance, angle between movements.
        /// </summary>
        private readonly TricubicInterpolator correctionInterpolator;

        /// <summary>
        /// Computes the lower bound for the angle correction to be applied
        /// as a function of the length of the first movement.
        /// A <c>null</c> value will mean that the correction is bounded from the left by the constant function y(x) = 0.
        /// </summary>
        [CanBeNull]
        private readonly CubicInterpolator minimumCorrection;

        /// <summary>
        /// Computes the upper bound for the angle correction to be applied
        /// as a function of the length of the first movement.
        /// A <c>null</c> value means that the correction is bounded from the right by the constant function y(x) = 1.
        /// </summary>
        [CanBeNull]
        private readonly CubicInterpolator maximumCorrection;

        /// <summary>
        /// A function that computes the scaling factor for the length of the second movement.
        /// A null value means that no scaling is applied.
        /// </summary>
        [CanBeNull]
        private readonly Func<double, double> otherMovementScalingFactor;

        /// <summary>
        /// Interpolation nodes for the movement angle part of the correction function.
        /// </summary>
        private static readonly double[] angles =
            Enumerable.Range(0, 5)
                      .Select(i => (double)i / 4 * Math.PI)
                      .ToArray();

        /// <summary>
        /// Calculates a value of the difficulty correction function for a given movement.
        /// </summary>
        /// <param name="currentMovementDistance">The distance between the last note and the target note.</param>
        /// <param name="otherMovementDistance">
        /// The distance between the target note and the next note
        /// (in the case of <see cref="FLOW_NEXT"/> and <see cref="SNAP_NEXT"/>),
        /// or the distance between the second-to-last note and the last note
        /// (in the case of <see cref="FLOW_SECONDLAST"/> and <see cref="SNAP_SECONDLAST"/>).
        /// </param>
        /// <param name="angles">The angle between the note pairs that are part of this movement.</param>
        /// <param name="values">Values of the correction function at the interpolation nodes.</param>
        /// <param name="minimumCorrection">
        /// The lower bound for the correction, as a function of the first movement length.
        /// Defaults to the constant function y(x) = 0 if not given.
        /// </param>
        /// <param name="maximumCorrection">
        /// The upper bound for the correction, as a function of the first movement length.
        /// Defaults to the constant function y(x) = 1 if not given.
        /// </param>
        /// <param name="otherMovementScalingFactor">Optional scaling factor to apply to <paramref name="otherMovementDistance"/>.</param>
        private AngleCorrection(
            double[] currentMovementDistance,
            double[] otherMovementDistance,
            double[] angles,
            double[,,] values,
            CubicInterpolator minimumCorrection = null,
            CubicInterpolator maximumCorrection = null,
            Func<double, double> otherMovementScalingFactor = null)
        {
            this.otherMovementScalingFactor = otherMovementScalingFactor;
            this.minimumCorrection = minimumCorrection;
            this.maximumCorrection = maximumCorrection;
            correctionInterpolator = new TricubicInterpolator(currentMovementDistance, otherMovementDistance, angles, values, dzLower: 0, dzUpper: 0);
        }

        /// <summary>
        /// Evaluates the angle correction of a movement between three objects.
        /// </summary>
        /// <remarks>
        /// This method presumes that in the Cartesian 2D coordinates:
        /// <list type="bullet">
        /// <item>the first object is placed at (<paramref name="firstX"/>, <paramref name="firstY"/>),</item>
        /// <item>the second object is placed at (0, 0),</item>
        /// <item>the third object is placed at (<paramref name="thirdX"/>, 0).</item>
        /// </list>
        /// </remarks>
        public double Evaluate(double thirdX, double firstX, double firstY)
        {
            if (otherMovementScalingFactor != null)
            {
                // we want to scale the other movement's length up by the given factor,
                // so to fit the nodes given in the arrays below we actually need to scale the function argument *down* by the inverse.
                double scalingFactor = 1 / otherMovementScalingFactor(thirdX);
                (firstX, firstY) = (scalingFactor * firstX, scalingFactor * firstY);
            }

            double angle = Math.Abs(Math.Atan2(firstY, firstX));
            double distance2 = Math.Sqrt(firstX * firstX + firstY * firstY);
            double maxVal = maximumCorrection?.Evaluate(thirdX) ?? 1;
            double minVal = minimumCorrection?.Evaluate(thirdX) ?? 0;

            // rescale the correction onto the interval [minVal, maxVal]
            double scale = maxVal - minVal;
            return minVal + scale * Math.Clamp(correctionInterpolator.Evaluate(thirdX, distance2, angle), 0, 1);
        }

        /// <summary>
        /// Calculates the angle difficulty correction
        /// for the movement pair: second-to-last -> last -> target note
        /// in the case of flow aim.
        /// </summary>
        public static readonly AngleCorrection FLOW_SECONDLAST = new AngleCorrection(
            currentMovementDistance: new[] { 0.2, 0.6, 1, 1.3, 1.7, 2.1 },
            otherMovementDistance: new[] { 0.1, 0.6, 1, 1.3, 1.8, 3 },
            angles,
            new[,,]
            {
                {
                    // last -> target distance = 0.2
                    //   0,   45,   90,  135,  180 degrees
                    { 0.45, 0.44, 0.42, 0.39, 0.39 }, // 2nd last -> last distance = 0.1
                    { 0.89, 0.87, 0.80, 0.72, 0.67 }, // 2nd last -> last distance = 0.6
                    { 0.99, 0.99, 0.98, 0.97, 0.96 }, // 2nd last -> last distance = 1
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // 2nd last -> last distance = 1.3
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // 2nd last -> last distance = 1.8
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // 2nd last -> last distance = 3
                },
                {
                    // last -> target distance = 0.6
                    //   0,   45,   90,  135,  180 degrees
                    { 0.27, 0.26, 0.23, 0.20, 0.19 }, // 2nd last -> last distance = 0.1
                    { 0.75, 0.68, 0.44, 0.26, 0.20 }, // 2nd last -> last distance = 0.6
                    { 0.97, 0.94, 0.83, 0.59, 0.46 }, // 2nd last -> last distance = 1
                    { 0.99, 0.99, 0.96, 0.86, 0.77 }, // 2nd last -> last distance = 1.3
                    { 1.00, 1.00, 1.00, 0.99, 0.98 }, // 2nd last -> last distance = 1.8
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // 2nd last -> last distance = 3
                },
                {
                    // last -> target distance = 1
                    //   0,   45,   90,  135,  180 degrees
                    { 0.16, 0.16, 0.14, 0.13, 0.13 }, // 2nd last -> last distance = 0.1
                    { 0.37, 0.31, 0.19, 0.13, 0.11 }, // 2nd last -> last distance = 0.6
                    { 0.65, 0.55, 0.29, 0.20, 0.17 }, // 2nd last -> last distance = 1
                    { 0.83, 0.76, 0.51, 0.34, 0.28 }, // 2nd last -> last distance = 1.3
                    { 0.96, 0.94, 0.84, 0.69, 0.61 }, // 2nd last -> last distance = 1.8
                    { 1.00, 1.00, 1.00, 0.99, 0.99 }, // 2nd last -> last distance = 3
                },
                {
                    // last -> target distance = 1.3
                    //   0,   45,   90,  135,  180 degrees
                    { 0.29, 0.28, 0.26, 0.23, 0.23 }, // 2nd last -> last distance = 0.1
                    { 0.56, 0.48, 0.31, 0.19, 0.16 }, // 2nd last -> last distance = 0.6
                    { 0.80, 0.71, 0.41, 0.24, 0.18 }, // 2nd last -> last distance = 1
                    { 0.91, 0.85, 0.61, 0.34, 0.25 }, // 2nd last -> last distance = 1.3
                    { 0.98, 0.96, 0.87, 0.63, 0.49 }, // 2nd last -> last distance = 1.8
                    { 1.00, 1.00, 1.00, 0.99, 0.97 }, // 2nd last -> last distance = 3
                },
                {
                    // last -> target distance = 1.7
                    //   0,   45,   90,  135,  180 degrees
                    { 0.39, 0.38, 0.35, 0.32, 0.31 }, // 2nd last -> last distance = 0.1
                    { 0.66, 0.59, 0.39, 0.24, 0.20 }, // 2nd last -> last distance = 0.6
                    { 0.85, 0.78, 0.47, 0.24, 0.17 }, // 2nd last -> last distance = 1
                    { 0.93, 0.88, 0.62, 0.27, 0.18 }, // 2nd last -> last distance = 1.3
                    { 0.98, 0.97, 0.84, 0.45, 0.27 }, // 2nd last -> last distance = 1.8
                    { 1.00, 1.00, 0.99, 0.94, 0.85 }, // 2nd last -> last distance = 3
                },
                {
                    // last -> target distance = 2.1
                    //   0,   45,   90,  135,  180 degrees
                    { 0.94, 0.94, 0.93, 0.92, 0.92 }, // 2nd last -> last = 0.1
                    { 0.98, 0.97, 0.94, 0.89, 0.86 }, // 2nd last -> last = 0.6
                    { 0.99, 0.99, 0.96, 0.87, 0.82 }, // 2nd last -> last = 1
                    { 1.00, 0.99, 0.97, 0.87, 0.81 }, // 2nd last -> last = 1.3
                    { 1.00, 1.00, 0.99, 0.90, 0.84 }, // 2nd last -> last = 1.8
                    { 1.00, 1.00, 1.00, 0.99, 0.98 }, // 2nd last -> last = 3
                },
            });

        /// <summary>
        /// Calculates the angle difficulty correction
        /// for the movement: last -> target -> next note
        /// in the case of flow aim.
        /// </summary>
        public static readonly AngleCorrection FLOW_NEXT = new AngleCorrection(
            currentMovementDistance: new[] { 0.2, 0.6, 1, 1.5, 2.8 },
            otherMovementDistance: new[] { 0.1, 0.6, 1, 1.3, 1.8, 3 },
            angles,
            new[,,]
            {
                {
                    // last -> target distance = 0.2
                    //   0,   45,   90,  135,  180 degrees
                    { 0.02, 0.02, 0.02, 0.03, 0.03 }, // target -> next distance = 0.1
                    { 0.07, 0.08, 0.11, 0.13, 0.14 }, // target -> next distance = 0.6
                    { 0.24, 0.27, 0.32, 0.37, 0.39 }, // target -> next distance = 1
                    { 0.47, 0.50, 0.57, 0.62, 0.64 }, // target -> next distance = 1.3
                    { 0.84, 0.85, 0.88, 0.90, 0.91 }, // target -> next distance = 1.8
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // target -> next distance = 3
                },
                {
                    // last -> target distance = 0.6
                    // 0,   45,   90,   135,   180 degrees
                    { 0.03, 0.03, 0.03, 0.03, 0.04 }, // target -> next distance = 0.1
                    { 0.04, 0.06, 0.09, 0.13, 0.15 }, // target -> next distance = 0.6
                    { 0.09, 0.15, 0.25, 0.36, 0.41 }, // target -> next distance = 1
                    { 0.22, 0.30, 0.46, 0.60, 0.65 }, // target -> next distance = 1.3
                    { 0.60, 0.68, 0.81, 0.89, 0.91 }, // target -> next distance = 1.8
                    { 0.99, 0.99, 1.00, 1.00, 1.00 }, // target -> next distance = 3
                },
                {
                    // last -> target distance = 1
                    //  0,   45,   90,   135,   180 degrees
                    { 0.04, 0.04, 0.04, 0.05, 0.05 }, // target -> next distance = 0.1
                    { 0.05, 0.06, 0.10, 0.15, 0.18 }, // target -> next distance = 0.6
                    { 0.07, 0.12, 0.21, 0.37, 0.43 }, // target -> next distance = 1
                    { 0.11, 0.21, 0.38, 0.59, 0.66 }, // target -> next distance = 1.3
                    { 0.36, 0.52, 0.74, 0.87, 0.91 }, // target -> next distance = 1.8
                    { 0.96, 0.98, 0.99, 1.00, 1.00 }, // target -> next distance = 3
                },
                {
                    // last -> target distance = 1.5
                    //   0,   45,   90,  135,  180 degrees
                    { 0.07, 0.07, 0.07, 0.08, 0.08 }, // target -> next distance = 0.1
                    { 0.12, 0.14, 0.19, 0.26, 0.29 }, // target -> next distance = 0.6
                    { 0.20, 0.25, 0.38, 0.53, 0.59 }, // target -> next distance = 1
                    { 0.29, 0.40, 0.56, 0.74, 0.79 }, // target -> next distance = 1.3
                    { 0.56, 0.71, 0.84, 0.93, 0.95 }, // target -> next distance = 1.8
                    { 0.98, 0.99, 1.00, 1.00, 1.00 }, // target -> next distance = 3
                },
                {
                    // last -> target distance = 2.8
                    //   0,   45,   90,  135,  180 degrees
                    { 0.10, 0.10, 0.10, 0.10, 0.11 }, // target -> next distance = 0.1
                    { 0.36, 0.37, 0.38, 0.39, 0.40 }, // target -> next distance = 0.6
                    { 0.67, 0.68, 0.70, 0.72, 0.72 }, // target -> next distance = 1
                    { 0.85, 0.85, 0.86, 0.88, 0.88 }, // target -> next distance = 1.3
                    { 0.97, 0.97, 0.97, 0.98, 0.98 }, // target -> next distance = 1.8
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // target -> next distance = 3
                },
            });

        /// <summary>
        /// Calculates the maximum allowable value for the <see cref="SNAP_SECONDLAST"/> correction.
        /// </summary>
        private static readonly CubicInterpolator snap_secondlast_maximum = new CubicInterpolator(new[] { 0, 1.5, 2.5, 4, 6, 6.01 }, new[] { 1, 0.85, 0.6, 0.8, 1, 1 });

        /// <summary>
        /// Calculates the angle difficulty correction
        /// for the movement pair: second-to-last -> last -> target note
        /// in the case of snap aim.
        /// </summary>
        public static readonly AngleCorrection SNAP_SECONDLAST = new AngleCorrection(
            currentMovementDistance: new[] { 0.6, 1.5, 2.4, 3.5, 5, 6.5, 9 },
            otherMovementDistance: new[] { 0, 0.5, 1, 1.5, 2.5 },
            otherMovementScalingFactor: d1 => Math.Clamp(d1, 2, 5),
            angles: angles,
            maximumCorrection: snap_secondlast_maximum,
            values: new[,,]
            {
                {
                    // last -> target distance = 0.6
                    //   0,   45,   90,  135,  180 degrees
                    { 0.52, 0.52, 0.52, 0.52, 0.52 }, // 2nd last -> last distance = 0   * 2 = 0
                    { 0.34, 0.40, 0.56, 0.72, 0.77 }, // 2nd last -> last distance = 0.5 * 2 = 1
                    { 0.43, 0.52, 0.74, 0.88, 0.91 }, // 2nd last -> last distance = 1   * 2 = 2
                    { 0.68, 0.76, 0.89, 0.95, 0.97 }, // 2nd last -> last distance = 1.5 * 2 = 3
                    { 0.95, 0.97, 0.98, 0.98, 0.99 }, // 2nd last -> last distance = 2.5 * 2 = 5
                },
                {
                    // last -> target distance = 1.5
                    //   0,   45,   90,  135,  180 degrees
                    { 0.76, 0.76, 0.76, 0.76, 0.76 }, // 2nd last -> last distance = 0   * 2 = 0
                    { 0.37, 0.48, 0.65, 0.48, 0.94 }, // 2nd last -> last distance = 0.5 * 2 = 1
                    { 0.21, 0.36, 0.73, 0.55, 0.98 }, // 2nd last -> last distance = 1   * 2 = 2
                    { 0.32, 0.52, 0.92, 0.99, 1.00 }, // 2nd last -> last distance = 1.5 * 2 = 3
                    { 0.90, 0.96, 1.00, 1.00, 1.00 }, // 2nd last -> last distance = 2.5 * 2 = 5
                },
                {
                    // last -> target distance = 2.4
                    //   0,   45,   90,  135,  180 degrees
                    { 0.45, 0.45, 0.45, 0.45, 0.45 }, // 2nd last -> last distance = 0   * 2.4 = 0
                    { 0.12, 0.18, 0.35, 0.61, 0.81 }, // 2nd last -> last distance = 0.5 * 2.4 = 1.2
                    { 0.05, 0.11, 0.42, 0.73, 0.96 }, // 2nd last -> last distance = 1   * 2.4 = 2.4
                    { 0.07, 0.17, 0.60, 0.98, 1.00 }, // 2nd last -> last distance = 1.5 * 2.4 = 3.6
                    { 0.56, 0.77, 0.99, 1.00, 1.00 }, // 2nd last -> last distance = 2.5 * 2.4 = 6
                },
                {
                    // last -> target distance = 3.5
                    //   0,   45,   90,  135,  180 degrees
                    { 0.37, 0.37, 0.37, 0.37, 0.37 }, // 2nd last -> last distance = 0   * 3.5 = 0
                    { 0.07, 0.12, 0.38, 0.76, 0.88 }, // 2nd last -> last distance = 0.5 * 3.5 = 1.75
                    { 0.02, 0.08, 0.51, 0.96, 1.00 }, // 2nd last -> last distance = 1   * 3.5 = 3.5
                    { 0.03, 0.16, 0.81, 1.00, 1.00 }, // 2nd last -> last distance = 1.5 * 3.5 = 5.25
                    { 0.57, 0.87, 1.00, 1.00, 1.00 }, // 2nd last -> last distance = 2.5 * 3.5 = 8.75
                },
                {
                    // last -> target distance = 5
                    //   0,   45,   90,  135,  180 degrees
                    { 0.27, 0.27, 0.27, 0.27, 0.27 }, // 2nd last -> last distance = 0   * 5 = 0
                    { 0.08, 0.13, 0.31, 0.58, 0.69 }, // 2nd last -> last distance = 0.5 * 5 = 2.5
                    { 0.04, 0.14, 0.48, 0.84, 0.90 }, // 2nd last -> last distance = 1   * 5 = 5
                    { 0.16, 0.33, 0.78, 0.94, 0.96 }, // 2nd last -> last distance = 1.5 * 5 = 7.5
                    { 0.85, 0.92, 0.96, 0.97, 0.97 }, // 2nd last -> last distance = 2.5 * 5 = 12.5
                },
                {
                    // last -> target distance = 6.5
                    //   0,   45,   90,  135,   180 degrees
                    { 0.26, 0.26, 0.26, 0.26, 0.26 }, // 2nd last -> last distance = 0   * 5 = 0
                    { 0.13, 0.16, 0.27, 0.44, 0.53 }, // 2nd last -> last distance = 0.5 * 5 = 2.5
                    { 0.08, 0.15, 0.32, 0.65, 0.77 }, // 2nd last -> last distance = 1   * 5 = 5
                    { 0.17, 0.24, 0.49, 0.83, 0.90 }, // 2nd last -> last distance = 1.5 * 5 = 7.5
                    { 0.62, 0.71, 0.90, 0.98, 0.99 }, // 2nd last -> last distance = 2.5 * 5 = 12.5
                },
                {
                    // last -> target distance = 9
                    //   0,   45,   90,  135,  180 degrees
                    { 0.26, 0.26, 0.26, 0.26, 0.26 }, // 2nd last -> last distance = 0   * 5 = 0
                    { 0.13, 0.16, 0.27, 0.44, 0.53 }, // 2nd last -> last distance = 0.5 * 5 = 2.5
                    { 0.08, 0.15, 0.32, 0.65, 0.77 }, // 2nd last -> last distance = 1   * 5 = 5
                    { 0.17, 0.24, 0.49, 0.83, 0.90 }, // 2nd last -> last distance = 1.5 * 5 = 7.5
                    { 0.62, 0.71, 0.90, 0.98, 0.99 }, // 2nd last -> last distance = 2.5 * 5 = 12.5
                },
            });

        /// <summary>
        /// Calculates the angle difficulty correction
        /// for the movement: last -> target -> next note
        /// in the case of snap aim.
        /// </summary>
        public static readonly AngleCorrection SNAP_NEXT = new AngleCorrection(
            currentMovementDistance: new[] { 0.6, 1.5, 2.4, 3.5, 5, 6.5, 9 },
            otherMovementDistance: new[] { 0, 0.5, 1, 1.5, 2.5 },
            otherMovementScalingFactor: d1 => Math.Clamp(d1, 2, 5),
            angles: angles,
            values: new[,,]
            {
                {
                    // last -> target distance = 0.6
                    //   0,   45,   90,  135,  180 degrees
                    { 0.62, 0.62, 0.62, 0.62, 0.62 }, // target -> next distance = 0   * 2 = 0
                    { 0.80, 0.77, 0.66, 0.54, 0.49 }, // target -> next distance = 0.5 * 2 = 1
                    { 0.92, 0.89, 0.78, 0.59, 0.50 }, // target -> next distance = 1   * 2 = 2
                    { 0.97, 0.96, 0.90, 0.76, 0.66 }, // target -> next distance = 1.5 * 2 = 3
                    { 1.00, 1.00, 0.99, 0.96, 0.94 }, // target -> next distance = 2.5 * 2 = 5
                },
                {
                    // last -> target distance = 1.5
                    //   0,   45,   90,  135,  180 degrees
                    { 0.62, 0.62, 0.62, 0.62, 0.62 }, // target -> next distance = 0   * 2 = 0
                    { 0.76, 0.72, 0.66, 0.54, 0.49 }, // target -> next distance = 0.5 * 2 = 1
                    { 0.88, 0.82, 0.78, 0.59, 0.50 }, // target -> next distance = 1   * 2 = 2
                    { 0.97, 0.96, 0.90, 0.76, 0.66 }, // target -> next distance = 1.5 * 2 = 3
                    { 1.00, 1.00, 0.99, 0.96, 0.94 }, // target -> next distance = 2.5 * 2 = 5
                },
                {
                    // last -> target distance = 2.4
                    //   0,   45,   90,  135,  180 degrees
                    { 0.12, 0.12, 0.12, 0.12, 0.12 }, // target -> next distance = 0   * 2.4 = 0
                    { 0.50, 0.35, 0.27, 0.16, 0.13 }, // target -> next distance = 0.5 * 2.4 = 1.2
                    { 0.80, 0.62, 0.49, 0.24, 0.17 }, // target -> next distance = 1   * 2.4 = 2.4
                    { 0.95, 0.91, 0.74, 0.43, 0.31 }, // target -> next distance = 1.5 * 2.4 = 3.6
                    { 1.00, 0.99, 0.97, 0.88, 0.80 }, // target -> next distance = 2.5 * 2.4 = 6
                },
                {
                    // last -> target distance = 3.5
                    //   0,   45,   90,  135,  180 degrees
                    { 0.08, 0.08, 0.08, 0.08, 0.08 }, // target -> next distance = 0   * 3.5 = 0
                    { 0.68, 0.53, 0.25, 0.09, 0.05 }, // target -> next distance = 0.5 * 3.5 = 1.75
                    { 0.94, 0.88, 0.64, 0.18, 0.08 }, // target -> next distance = 1   * 3.5 = 3.5
                    { 1.00, 0.99, 0.93, 0.57, 0.31 }, // target -> next distance = 1.5 * 3.5 = 5.25
                    { 1.00, 1.00, 1.00, 0.99, 0.97 }, // target -> next distance = 2.5 * 3.5 = 8.75
                },
                {
                    // last -> target distance = 5
                    //   0,   45,   90,  135,  180 degrees
                    { 0.11, 0.11, 0.11, 0.11, 0.11 }, // target -> next distance = 0   * 5 = 0
                    { 0.88, 0.77, 0.39, 0.10, 0.05 }, // target -> next distance = 0.5 * 5 = 2.5
                    { 0.99, 0.99, 0.86, 0.29, 0.07 }, // target -> next distance = 1   * 5 = 5
                    { 1.00, 1.00, 0.99, 0.83, 0.53 }, // target -> next distance = 1.5 * 5 = 7.5
                    { 1.00, 1.00, 1.00, 1.00, 1.00 }, // target -> next distance = 2.5 * 5 = 12.5
                },
                {
                    // last -> target distance = 6.5
                    //   0,   45,   90,  135,  180 degrees
                    { 0.09, 0.09, 0.09, 0.09, 0.09 }, // target -> next distance = 0   * 5 = 0
                    { 0.79, 0.66, 0.32, 0.10, 0.06 }, // target -> next distance = 0.5 * 5 = 2.5
                    { 0.98, 0.96, 0.76, 0.22, 0.07 }, // target -> next distance = 1   * 5 = 5
                    { 1.00, 1.00, 0.97, 0.66, 0.29 }, // target -> next distance = 1.5 * 5 = 7.5
                    { 1.00, 1.00, 1.00, 0.99, 0.98 }, // target -> next distance = 2.5 * 5 = 12.5
                },
                {
                    // last -> target distance = 9
                    //   0,   45,   90,  135,  180 degrees
                    { 0.09, 0.09, 0.09, 0.09, 0.09 }, // target -> next distance = 0   * 5 = 0
                    { 0.79, 0.66, 0.32, 0.10, 0.06 }, // target -> next distance = 0.5 * 5 = 2.5
                    { 0.98, 0.96, 0.76, 0.22, 0.07 }, // target -> next distance = 1   * 5 = 5
                    { 1.00, 1.00, 0.97, 0.66, 0.29 }, // target -> next distance = 1.5 * 5 = 7.5
                    { 1.00, 1.00, 1.00, 0.99, 0.98 }, // target -> next distance = 2.5 * 5 = 12.5
                },
            });
    }
}
