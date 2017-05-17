// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.MathUtils;
using System;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    internal abstract class ObjectConversion
    {
        protected readonly int AvailableColumns;
        protected readonly int RandomStart;

        protected ObjectRow PreviousRow;
        protected readonly FastRandom Random;
        protected readonly Beatmap Beatmap;

        protected ObjectConversion(ObjectRow previousRow, FastRandom random, Beatmap beatmap)
        {
            PreviousRow = previousRow;
            Random = random;
            Beatmap = beatmap;

            AvailableColumns = (int)Math.Round(beatmap.BeatmapInfo.Difficulty.CircleSize);
            RandomStart = AvailableColumns == 8 ? 1 : 0;
        }

        /// <summary>
        /// Generates a new row filled with converted hit objects.
        /// </summary>
        /// <returns>The new row.</returns>
        public abstract ObjectRow GenerateConversion();

        /// <summary>
        /// Converts an x-position into a column.
        /// </summary>
        /// <param name="position">The x-position.</param>
        /// <param name="allowSpecial">Whether to treat as 7K + 1.</param>
        /// <returns>The column.</returns>
        protected int GetColumn(float position, bool allowSpecial = false)
        {
            if (allowSpecial && AvailableColumns == 8)
            {
                const float local_x_divisor = 512f / 7;
                return MathHelper.Clamp((int)Math.Floor(position / local_x_divisor), 0, 6) + 1;
            }

            float localXDivisor = 512f / AvailableColumns;
            return MathHelper.Clamp((int)Math.Floor(position / localXDivisor), 0, AvailableColumns - 1);
        }

        /// <summary>
        /// Generates a count of notes to be generated from probabilities.
        /// </summary>
        /// <param name="p2">Probability for 2 notes to be generated.</param>
        /// <param name="p3">Probability for 3 notes to be generated.</param>
        /// <param name="p4">Probability for 4 notes to be generated.</param>
        /// <param name="p5">Probability for 5 notes to be generated.</param>
        /// <param name="p6">Probability for 6 notes to be generated.</param>
        /// <returns>The amount of notes to be generated.</returns>
        protected int GetRandomNoteCount(double p2, double p3, double p4 = 1, double p5 = 1, double p6 = 1)
        {
            double val = Random.NextDouble();
            if (val >= 1 - p6)
                return 6;
            if (val >= 1 - p5)
                return 5;
            if (val >= 1 - p4)
                return 4;
            if (val >= 1 - p3)
                return 3;
            return val >= 1 - p2 ? 2 : 1;
        }
    }
}
