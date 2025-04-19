// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Audio
{
    /// <summary>
    /// Common functions and constants for implementing decibel scaling into sliders and meters.
    /// </summary>
    public static class DecibelScaling
    {
        /// <summary>
        /// Arbitrary silence threshold. Required for sliders, since the decibel scale is bottomless.
        /// </summary>
        public const double DB_MIN = -60;

        /// <summary>
        /// Decibel equivalent of full volume.
        /// </summary>
        public const double DB_MAX = 0;

        /// <summary>
        /// Decibel precision level.
        /// </summary>
        public const double DB_PRECISION = 0.5;

        /// <summary>
        /// Linear equivalent of <see cref="DB_MIN"/>
        /// </summary>
        private static readonly double cutoff = Math.Pow(10, DB_MIN / 20);

        /// <summary>
        /// Returns the decibel equivalent of a linear value.
        /// </summary>
        public static double DecibelFromLinear(double linear) => linear <= cutoff ? DB_MIN : 20 * Math.Log10(linear);

        /// <summary>
        /// Returns the linear equivalent of a decibel value.
        /// </summary>
        public static double LinearFromDecibel(double decibel) => decibel <= DB_MIN ? 0 : Math.Pow(10, decibel / 20);
    }
}
