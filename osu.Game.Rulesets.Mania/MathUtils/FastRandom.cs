﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Rulesets.Mania.MathUtils
{
    /// <summary>
    /// A PRNG specified in http://heliosphan.org/fastrandom.html.
    /// </summary>
    internal class FastRandom
    {
        private const double uint_to_real = 1.0 / (uint.MaxValue + 1.0);
        private const uint int_mask = 0x7FFFFFFF;
        private const uint y = 842502087;
        private const uint z = 3579807591;
        private const uint w = 273326509;
        private uint _x, _y = y, _z = z, _w = w;

        public FastRandom(int seed)
        {
            _x = (uint)seed;
        }

        public FastRandom()
            : this(Environment.TickCount)
        {
        }

        /// <summary>
        /// Generates a random unsigned integer within the range [<see cref="uint.MinValue"/>, <see cref="uint.MaxValue"/>).
        /// </summary>
        /// <returns>The random value.</returns>
        public uint NextUInt()
        {
            uint t = _x ^ _x << 11;
            _x = _y;
            _y = _z;
            _z = _w;
            return _w = _w ^ _w >> 19 ^ t ^ t >> 8;
        }

        /// <summary>
        /// Generates a random integer value within the range [0, <see cref="int.MaxValue"/>).
        /// </summary>
        /// <returns>The random value.</returns>
        public int Next() => (int)(int_mask & NextUInt());

        /// <summary>
        /// Generates a random integer value within the range [0, <paramref name="upperBound"/>).
        /// </summary>
        /// <param name="upperBound">The upper bound.</param>
        /// <returns>The random value.</returns>
        public int Next(int upperBound) => (int)(NextDouble() * upperBound);

        /// <summary>
        /// Generates a random integer value within the range [<paramref name="lowerBound"/>, <paramref name="upperBound"/>).
        /// </summary>
        /// <param name="lowerBound">The lower bound of the range.</param>
        /// <param name="upperBound">The upper bound of the range.</param>
        /// <returns>The random value.</returns>
        public int Next(int lowerBound, int upperBound) => (int)(lowerBound + NextDouble() * (upperBound - lowerBound));

        /// <summary>
        /// Generates a random double value within the range [0, 1).
        /// </summary>
        /// <returns>The random value.</returns>
        public double NextDouble() => uint_to_real * NextUInt();

        private uint bitBuffer;
        private int bitIndex = 32;

        /// <summary>
        /// Generates a reandom boolean value. Cached such that a random value is only generated once in every 32 calls.
        /// </summary>
        /// <returns>The random value.</returns>
        public bool NextBool()
        {
            if (bitIndex == 32)
            {
                bitBuffer = NextUInt();
                bitIndex = 1;

                return (bitBuffer & 1) == 1;
            }

            bitIndex++;
            return ((bitBuffer >>= 1) & 1) == 1;
        }
    }
}
