// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Osu.Difficulty.Utils
{
    public struct Bin
    {
        public double Difficulty;
        public double Count;

        private const int bin_count = 32;

        /// <summary>
        /// Create an array of equally spaced bins. Count is linearly interpolated into each bin.
        /// For example, if we have bins with values [1,2,3,4,5] and want to insert the value 3.2,
        /// we will add 0.8 to the count of 3's and 0.2 to the count of 4's
        /// </summary>
        public static Bin[] CreateBins(List<double> difficulties)
        {
            double maxDifficulty = difficulties.Max();

            var bins = new Bin[bin_count];

            for (int i = 0; i < bin_count; i++)
            {
                bins[i].Difficulty = maxDifficulty * (i + 1) / bin_count;
            }

            foreach (double d in difficulties)
            {
                double binIndex = bin_count * (d / maxDifficulty) - 1;

                int lowerBound = (int)Math.Floor(binIndex);
                double t = binIndex - lowerBound;

                //This can be -1, corresponding to the zero difficulty bucket.
                //We don't store that since it doesn't contribute to difficulty
                if (lowerBound >= 0)
                {
                    bins[lowerBound].Count += (1 - t);
                }

                int upperBound = lowerBound + 1;

                // this can be == bin_count for the maximum difficulty object, in which case t will be 0 anyway
                if (upperBound < bin_count)
                {
                    bins[upperBound].Count += t;
                }
            }

            return bins;
        }
    }
}
