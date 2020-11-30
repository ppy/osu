// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
        /// Compute an integer from given seed and series number.
        /// </summary>
        /// <param name="seed">
        /// The seed value of this random number generator.
        /// </param>
        /// <param name="series">
        /// The series number.
        /// Different values are computed for the same seed in different series.
        /// </param>
        public static ulong Get(int seed, int series = 0) =>
            unchecked(mix(((ulong)(uint)series << 32) | ((uint)seed ^ 0x12345678)));

        /// <summary>
        /// Compute a floating point value between 0 and 1 (excluding 1) from given seed and series number.
        /// </summary>
        /// <param name="seed">
        /// The seed value of this random number generator.
        /// </param>
        /// <param name="series">
        /// The series number.
        /// Different values are computed for the same seed in different series.
        /// </param>
        public static float GetSingle(int seed, int series = 0) =>
            (float)(Get(seed, series) & ((1 << 24) - 1)) / (1 << 24); // float has 24-bit precision
    }
}
