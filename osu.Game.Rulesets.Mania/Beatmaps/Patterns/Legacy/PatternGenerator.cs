// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.Mania.MathUtils;
using osu.Game.Rulesets.Objects;
using OpenTK;

namespace osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy
{
    /// <summary>
    /// A pattern generator for legacy hit objects.
    /// </summary>
    internal abstract class PatternGenerator : Patterns.PatternGenerator
    {
        /// <summary>
        /// The column index at which to start generating random notes.
        /// </summary>
        protected readonly int RandomStart;

        /// <summary>
        /// The random number generator to use.
        /// </summary>
        protected readonly FastRandom Random;

        protected PatternGenerator(FastRandom random, HitObject hitObject, Beatmap beatmap, Pattern previousPattern)
            : base(hitObject, beatmap, previousPattern)
        {
            Random = random;

            RandomStart = AvailableColumns == 8 ? 1 : 0;
        }

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
        protected int GetRandomNoteCount(double p2, double p3, double p4 = 0, double p5 = 0, double p6 = 0)
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

        private double? conversionDifficulty;
        /// <summary>
        /// A difficulty factor used for various conversion methods from osu!stable.
        /// </summary>
        protected double ConversionDifficulty
        {
            get
            {
                if (conversionDifficulty != null)
                    return conversionDifficulty.Value;

                HitObject lastObject = Beatmap.HitObjects.LastOrDefault();
                HitObject firstObject = Beatmap.HitObjects.FirstOrDefault();

                double drainTime = (lastObject?.StartTime ?? 0) - (firstObject?.StartTime ?? 0);
                drainTime -= Beatmap.EventInfo.TotalBreakTime;

                if (drainTime == 0)
                    drainTime = 10000;

                BeatmapDifficulty difficulty = Beatmap.BeatmapInfo.Difficulty;
                conversionDifficulty = ((difficulty.DrainRate + MathHelper.Clamp(difficulty.ApproachRate, 4, 7)) / 1.5 + Beatmap.HitObjects.Count / drainTime * 9f) / 38f * 5f / 1.15;
                conversionDifficulty = Math.Min(conversionDifficulty.Value, 12);

                return conversionDifficulty.Value;
            }
        }
    }
}
