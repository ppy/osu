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
        public double Time;
        public double NoteCount;

        /// <summary>
        /// Creates a 2D grid of bins using bilinear interpolation.
        /// Notes are distributed across neighboring bins weighted by their fractional position.
        /// </summary>
        public static List<Bin> CreateBins(List<double> difficulties, List<double> times, int difficultyDimensionLength, int timeDimensionLength)
        {
            double maxDifficulty = difficulties.Max();
            double endTime = times.Max();

            var binsArray = new Bin[timeDimensionLength * difficultyDimensionLength];

            for (int timeIndex = 0; timeIndex < timeDimensionLength; timeIndex++)
            {
                double time = endTime * timeIndex / (timeDimensionLength - 1);

                for (int diffIndex = 0; diffIndex < difficultyDimensionLength; diffIndex++)
                {
                    int binIndex = difficultyDimensionLength * timeIndex + diffIndex;

                    binsArray[binIndex].Time = time;

                    // We don't create a 0 difficulty bin because 0 difficulty notes don't contribute to star rating.
                    binsArray[binIndex].Difficulty = maxDifficulty * (diffIndex + 1) / difficultyDimensionLength;
                }
            }

            for (int noteIndex = 0; noteIndex < difficulties.Count; noteIndex++)
            {
                double timeBinIndex = timeDimensionLength * (times[noteIndex] / endTime);
                double difficultyBinIndex = difficultyDimensionLength * (difficulties[noteIndex] / maxDifficulty) - 1;

                int timeLower = Math.Min((int)timeBinIndex, timeDimensionLength - 1);
                int timeUpper = Math.Min(timeLower + 1, timeDimensionLength - 1);
                double timeWeight = timeBinIndex - timeLower;

                int difficultyLower = fastFloor(difficultyBinIndex);
                int difficultyUpper = Math.Min(difficultyLower + 1, difficultyDimensionLength - 1);
                double difficultyWeight = difficultyBinIndex - difficultyLower;

                // The lower bound of difficulty can be -1, corresponding to buckets with 0 difficulty.
                // We don't store those since they don't contribute to star rating.
                if (difficultyLower >= 0)
                {
                    binsArray[difficultyDimensionLength * timeLower + difficultyLower].NoteCount += (1 - timeWeight) * (1 - difficultyWeight);
                    binsArray[difficultyDimensionLength * timeUpper + difficultyLower].NoteCount += timeWeight * (1 - difficultyWeight);
                }

                binsArray[difficultyDimensionLength * timeLower + difficultyUpper].NoteCount += (1 - timeWeight) * difficultyWeight;
                binsArray[difficultyDimensionLength * timeUpper + difficultyUpper].NoteCount += timeWeight * difficultyWeight;
            }

            var binsList = binsArray.ToList();

            return binsList;
        }

        // Faster implementation of the floor function to speed up binning times.
        private static int fastFloor(double x) => x >= 0 || x == -1 ? (int)x : (int)(x - 1);
    }
}
