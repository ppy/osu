// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Linq;
using JetBrains.Annotations;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;
using osu.Game.Utils;

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
        protected readonly LegacyRandom Random;

        /// <summary>
        /// The beatmap which <see cref="HitObject"/> is being converted from.
        /// </summary>
        protected readonly IBeatmap OriginalBeatmap;

        protected PatternGenerator(LegacyRandom random, HitObject hitObject, ManiaBeatmap beatmap, Pattern previousPattern, IBeatmap originalBeatmap)
            : base(hitObject, beatmap, previousPattern)
        {
            ArgumentNullException.ThrowIfNull(random);
            ArgumentNullException.ThrowIfNull(originalBeatmap);

            Random = random;
            OriginalBeatmap = originalBeatmap;

            RandomStart = TotalColumns == 8 ? 1 : 0;
        }

        /// <summary>
        /// Converts an x-position into a column.
        /// </summary>
        /// <param name="position">The x-position.</param>
        /// <param name="allowSpecial">Whether to treat as 7K + 1.</param>
        /// <returns>The column.</returns>
        protected int GetColumn(float position, bool allowSpecial = false)
        {
            // Casts to doubles are present here because, although code is originally written as float division,
            // the division actually appears to occur on doubles in osu!stable. This is likely a result of
            // differences in optimisations between .NET versions due to the presence of the double parameter type of Math.Floor().

            if (allowSpecial && TotalColumns == 8)
            {
                const float local_x_divisor = 512f / 7;
                return Math.Clamp((int)Math.Floor((double)position / local_x_divisor), 0, 6) + 1;
            }

            float localXDivisor = 512f / TotalColumns;
            return Math.Clamp((int)Math.Floor((double)position / localXDivisor), 0, TotalColumns - 1);
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
            if (p2 < 0 || p2 > 1) throw new ArgumentOutOfRangeException(nameof(p2));
            if (p3 < 0 || p3 > 1) throw new ArgumentOutOfRangeException(nameof(p3));
            if (p4 < 0 || p4 > 1) throw new ArgumentOutOfRangeException(nameof(p4));
            if (p5 < 0 || p5 > 1) throw new ArgumentOutOfRangeException(nameof(p5));
            if (p6 < 0 || p6 > 1) throw new ArgumentOutOfRangeException(nameof(p6));

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

                HitObject lastObject = OriginalBeatmap.HitObjects.LastOrDefault();
                HitObject firstObject = OriginalBeatmap.HitObjects.FirstOrDefault();

                // Drain time in seconds
                int drainTime = (int)(((lastObject?.StartTime ?? 0) - (firstObject?.StartTime ?? 0) - OriginalBeatmap.TotalBreakTime) / 1000);

                if (drainTime == 0)
                    drainTime = 10000;

                IBeatmapDifficultyInfo difficulty = OriginalBeatmap.Difficulty;
                conversionDifficulty = ((difficulty.DrainRate + Math.Clamp(difficulty.ApproachRate, 4, 7)) / 1.5 + (double)OriginalBeatmap.HitObjects.Count / drainTime * 9f) / 38f * 5f / 1.15;
                conversionDifficulty = Math.Min(conversionDifficulty.Value, 12);

                return conversionDifficulty.Value;
            }
        }

        /// <summary>
        /// Finds a new column in which a <see cref="HitObject"/> can be placed.
        /// This uses <see cref="GetRandomColumn"/> to pick the next candidate column.
        /// </summary>
        /// <param name="initialColumn">The initial column to test. This may be returned if it is already a valid column.</param>
        /// <param name="patterns">A list of patterns for which the validity of a column should be checked against.
        /// A column is not a valid candidate if a <see cref="HitObject"/> occupies the same column in any of the patterns.</param>
        /// <returns>A column for which there are no <see cref="HitObject"/>s in any of <paramref name="patterns"/> occupying the same column.</returns>
        /// <exception cref="NotEnoughColumnsException">If there are no valid candidate columns.</exception>
        protected int FindAvailableColumn(int initialColumn, params Pattern[] patterns)
            => FindAvailableColumn(initialColumn, null, patterns: patterns);

        /// <summary>
        /// Finds a new column in which a <see cref="HitObject"/> can be placed.
        /// </summary>
        /// <param name="initialColumn">The initial column to test. This may be returned if it is already a valid column.</param>
        /// <param name="nextColumn">A function to retrieve the next column. If null, a randomisation scheme will be used.</param>
        /// <param name="validation">A function to perform additional validation checks to determine if a column is a valid candidate for a <see cref="HitObject"/>.</param>
        /// <param name="lowerBound">The minimum column index. If null, <see cref="RandomStart"/> is used.</param>
        /// <param name="upperBound">The maximum column index. If null, <see cref="Patterns.PatternGenerator.TotalColumns">TotalColumns</see> is used.</param>
        /// <param name="patterns">A list of patterns for which the validity of a column should be checked against.
        /// A column is not a valid candidate if a <see cref="HitObject"/> occupies the same column in any of the patterns.</param>
        /// <returns>A column which has passed the <paramref name="validation"/> check and for which there are no
        /// <see cref="HitObject"/>s in any of <paramref name="patterns"/> occupying the same column.</returns>
        /// <exception cref="NotEnoughColumnsException">If there are no valid candidate columns.</exception>
        protected int FindAvailableColumn(int initialColumn, int? lowerBound = null, int? upperBound = null, Func<int, int> nextColumn = null, [InstantHandle] Func<int, bool> validation = null,
                                          params Pattern[] patterns)
        {
            lowerBound ??= RandomStart;
            upperBound ??= TotalColumns;
            nextColumn ??= _ => GetRandomColumn(lowerBound, upperBound);

            // Check for the initial column
            if (isValid(initialColumn))
                return initialColumn;

            // Ensure that we have at least one free column, so that an endless loop is avoided
            bool hasValidColumns = false;

            for (int i = lowerBound.Value; i < upperBound.Value; i++)
            {
                hasValidColumns = isValid(i);
                if (hasValidColumns)
                    break;
            }

            if (!hasValidColumns)
                throw new NotEnoughColumnsException();

            // Iterate until a valid column is found. This is a random iteration in the default case.
            do
            {
                initialColumn = nextColumn(initialColumn);
            } while (!isValid(initialColumn));

            return initialColumn;

            bool isValid(int column)
            {
                if (validation?.Invoke(column) == false)
                    return false;

                foreach (var p in patterns)
                {
                    if (p.ColumnHasObject(column))
                        return false;
                }

                return true;
            }
        }

        /// <summary>
        /// Returns a random column index in the range [<paramref name="lowerBound"/>, <paramref name="upperBound"/>).
        /// </summary>
        /// <param name="lowerBound">The minimum column index. If null, <see cref="RandomStart"/> is used.</param>
        /// <param name="upperBound">The maximum column index. If null, <see cref="Patterns.PatternGenerator.TotalColumns"/> is used.</param>
        protected int GetRandomColumn(int? lowerBound = null, int? upperBound = null) => Random.Next(lowerBound ?? RandomStart, upperBound ?? TotalColumns);

        /// <summary>
        /// Occurs when mania conversion is stuck in an infinite loop unable to find columns to place new hitobjects in.
        /// </summary>
        public class NotEnoughColumnsException : Exception
        {
            public NotEnoughColumnsException()
                : base("There were not enough columns to complete conversion.")
            {
            }
        }
    }
}
