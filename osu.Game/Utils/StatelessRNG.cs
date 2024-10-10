// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Utils
{
    /// <summary>
    /// Provides a fast stateless function that can be used in randomly-looking visual elements.
    /// </summary>
    public static class StatelessRNG
    {
        private static ulong mix(ulong x)
        {
            unchecked
            {
                x ^= x >> 33;
                x *= 0xff51afd7ed558ccd;
                x ^= x >> 33;
                x *= 0xc4ceb9fe1a85ec53;
                x ^= x >> 33;
                return x;
            }
        }

        /// <summary>
        /// Generate a random 64-bit unsigned integer from given seed.
        /// </summary>
        /// <param name="seed">
        /// The seed value of this random number generator.
        /// </param>
        /// <param name="series">
        /// The series number.
        /// Different values are computed for the same seed in different series.
        /// </param>
        public static ulong NextULong(int seed, int series = 0)
        {
            unchecked
            {
                ulong combined = ((ulong)(uint)series << 32) | (uint)seed;
                // The xor operation is to not map (0, 0) to 0.
                return mix(combined ^ 0x12345678);
            }
        }

        /// <summary>
        /// Generate a random integer in range [0, maxValue) from given seed.
        /// </summary>
        /// <param name="maxValue">
        /// The number of possible results.
        /// </param>
        /// <param name="seed">
        /// The seed value of this random number generator.
        /// </param>
        /// <param name="series">
        /// The series number.
        /// Different values are computed for the same seed in different series.
        /// </param>
        public static int NextInt(int maxValue, int seed, int series = 0)
        {
            ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxValue);

            return (int)(NextULong(seed, series) % (ulong)maxValue);
        }

        /// <summary>
        /// Compute a random floating point value between 0 and 1 (excluding 1) from given seed and series number.
        /// </summary>
        /// <param name="seed">
        /// The seed value of this random number generator.
        /// </param>
        /// <param name="series">
        /// The series number.
        /// Different values are computed for the same seed in different series.
        /// </param>
        public static float NextSingle(int seed, int series = 0) =>
            (float)(NextULong(seed, series) & ((1 << 24) - 1)) / (1 << 24); // float has 24-bit precision

        /// <summary>
        /// Compute a random floating point value between <paramref name="min"/> and <paramref name="max"/> from given seed and series number.
        /// </summary>
        public static float NextSingle(float min, float max, int seed, int series = 0) => min + NextSingle(seed, series) * (max - min);
    }
}
