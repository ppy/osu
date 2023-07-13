// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.EnumExtensions;
using osuTK;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy
{
    internal class HitObjectPatternGenerator : PatternGenerator
    {
        public PatternType StairType { get; private set; }

        private readonly PatternType convertType;

        public HitObjectPatternGenerator(LegacyRandom random, HitObject hitObject, ManiaBeatmap beatmap, Pattern previousPattern, double previousTime, Vector2 previousPosition, double density,
                                         PatternType lastStair, IBeatmap originalBeatmap)
            : base(random, hitObject, beatmap, previousPattern, originalBeatmap)
        {
            StairType = lastStair;

            TimingControlPoint timingPoint = beatmap.ControlPointInfo.TimingPointAt(hitObject.StartTime);
            EffectControlPoint effectPoint = beatmap.ControlPointInfo.EffectPointAt(hitObject.StartTime);

            var positionData = hitObject as IHasPosition;

            float positionSeparation = ((positionData?.Position ?? Vector2.Zero) - previousPosition).Length;
            double timeSeparation = hitObject.StartTime - previousTime;

            if (timeSeparation <= 80)
            {
                // More than 187 BPM
                convertType |= PatternType.ForceNotStack | PatternType.KeepSingle;
            }
            else if (timeSeparation <= 95)
            {
                // More than 157 BPM
                convertType |= PatternType.ForceNotStack | PatternType.KeepSingle | lastStair;
            }
            else if (timeSeparation <= 105)
            {
                // More than 140 BPM
                convertType |= PatternType.ForceNotStack | PatternType.LowProbability;
            }
            else if (timeSeparation <= 125)
            {
                // More than 120 BPM
                convertType |= PatternType.ForceNotStack;
            }
            else if (timeSeparation <= 135 && positionSeparation < 20)
            {
                // More than 111 BPM stream
                convertType |= PatternType.Cycle | PatternType.KeepSingle;
            }
            else if (timeSeparation <= 150 && positionSeparation < 20)
            {
                // More than 100 BPM stream
                convertType |= PatternType.ForceStack | PatternType.LowProbability;
            }
            else if (positionSeparation < 20 && density >= timingPoint.BeatLength / 2.5)
            {
                // Low density stream
                convertType |= PatternType.Reverse | PatternType.LowProbability;
            }
            else if (density < timingPoint.BeatLength / 2.5 || effectPoint.KiaiMode)
            {
                // High density
            }
            else
                convertType |= PatternType.LowProbability;

            if (!convertType.HasFlagFast(PatternType.KeepSingle))
            {
                if (HitObject.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH) && TotalColumns != 8)
                    convertType |= PatternType.Mirror;
                else if (HitObject.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP))
                    convertType |= PatternType.Gathered;
            }
        }

        public override IEnumerable<Pattern> Generate()
        {
            Pattern generateCore()
            {
                var pattern = new Pattern();

                if (TotalColumns == 1)
                {
                    addToPattern(pattern, 0);
                    return pattern;
                }

                int lastColumn = PreviousPattern.HitObjects.FirstOrDefault()?.Column ?? 0;

                if (convertType.HasFlagFast(PatternType.Reverse) && PreviousPattern.HitObjects.Any())
                {
                    // Generate a new pattern by copying the last hit objects in reverse-column order
                    for (int i = RandomStart; i < TotalColumns; i++)
                    {
                        if (PreviousPattern.ColumnHasObject(i))
                            addToPattern(pattern, RandomStart + TotalColumns - i - 1);
                    }

                    return pattern;
                }

                if (convertType.HasFlagFast(PatternType.Cycle) && PreviousPattern.HitObjects.Count() == 1
                                                               // If we convert to 7K + 1, let's not overload the special key
                                                               && (TotalColumns != 8 || lastColumn != 0)
                                                               // Make sure the last column was not the centre column
                                                               && (TotalColumns % 2 == 0 || lastColumn != TotalColumns / 2))
                {
                    // Generate a new pattern by cycling backwards (similar to Reverse but for only one hit object)
                    int column = RandomStart + TotalColumns - lastColumn - 1;
                    addToPattern(pattern, column);

                    return pattern;
                }

                if (convertType.HasFlagFast(PatternType.ForceStack) && PreviousPattern.HitObjects.Any())
                {
                    // Generate a new pattern by placing on the already filled columns
                    for (int i = RandomStart; i < TotalColumns; i++)
                    {
                        if (PreviousPattern.ColumnHasObject(i))
                            addToPattern(pattern, i);
                    }

                    return pattern;
                }

                if (PreviousPattern.HitObjects.Count() == 1)
                {
                    if (convertType.HasFlagFast(PatternType.Stair))
                    {
                        // Generate a new pattern by placing on the next column, cycling back to the start if there is no "next"
                        int targetColumn = lastColumn + 1;
                        if (targetColumn == TotalColumns)
                            targetColumn = RandomStart;

                        addToPattern(pattern, targetColumn);
                        return pattern;
                    }

                    if (convertType.HasFlagFast(PatternType.ReverseStair))
                    {
                        // Generate a new pattern by placing on the previous column, cycling back to the end if there is no "previous"
                        int targetColumn = lastColumn - 1;
                        if (targetColumn == RandomStart - 1)
                            targetColumn = TotalColumns - 1;

                        addToPattern(pattern, targetColumn);
                        return pattern;
                    }
                }

                if (convertType.HasFlagFast(PatternType.KeepSingle))
                    return generateRandomNotes(1);

                if (convertType.HasFlagFast(PatternType.Mirror))
                {
                    if (ConversionDifficulty > 6.5)
                        return generateRandomPatternWithMirrored(0.12, 0.38, 0.12);
                    if (ConversionDifficulty > 4)
                        return generateRandomPatternWithMirrored(0.12, 0.17, 0);

                    return generateRandomPatternWithMirrored(0.12, 0, 0);
                }

                if (ConversionDifficulty > 6.5)
                {
                    if (convertType.HasFlagFast(PatternType.LowProbability))
                        return generateRandomPattern(0.78, 0.42, 0, 0);

                    return generateRandomPattern(1, 0.62, 0, 0);
                }

                if (ConversionDifficulty > 4)
                {
                    if (convertType.HasFlagFast(PatternType.LowProbability))
                        return generateRandomPattern(0.35, 0.08, 0, 0);

                    return generateRandomPattern(0.52, 0.15, 0, 0);
                }

                if (ConversionDifficulty > 2)
                {
                    if (convertType.HasFlagFast(PatternType.LowProbability))
                        return generateRandomPattern(0.18, 0, 0, 0);

                    return generateRandomPattern(0.45, 0, 0, 0);
                }

                return generateRandomPattern(0, 0, 0, 0);
            }

            var p = generateCore();

            foreach (var obj in p.HitObjects)
            {
                if (convertType.HasFlagFast(PatternType.Stair) && obj.Column == TotalColumns - 1)
                    StairType = PatternType.ReverseStair;
                if (convertType.HasFlagFast(PatternType.ReverseStair) && obj.Column == RandomStart)
                    StairType = PatternType.Stair;
            }

            return p.Yield();
        }

        /// <summary>
        /// Generates random notes.
        /// <para>
        /// This will generate as many as it can up to <paramref name="noteCount"/>, accounting for
        /// any stacks if <see cref="convertType"/> is forcing no stacks.
        /// </para>
        /// </summary>
        /// <param name="noteCount">The amount of notes to generate.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateRandomNotes(int noteCount)
        {
            var pattern = new Pattern();

            bool allowStacking = !convertType.HasFlagFast(PatternType.ForceNotStack);

            if (!allowStacking)
                noteCount = Math.Min(noteCount, TotalColumns - RandomStart - PreviousPattern.ColumnWithObjects);

            int nextColumn = GetColumn((HitObject as IHasXPosition)?.X ?? 0, true);

            for (int i = 0; i < noteCount; i++)
            {
                nextColumn = allowStacking
                    ? FindAvailableColumn(nextColumn, nextColumn: getNextColumn, patterns: pattern)
                    : FindAvailableColumn(nextColumn, nextColumn: getNextColumn, patterns: new[] { pattern, PreviousPattern });

                addToPattern(pattern, nextColumn);
            }

            return pattern;

            int getNextColumn(int last)
            {
                if (convertType.HasFlagFast(PatternType.Gathered))
                {
                    last++;
                    if (last == TotalColumns)
                        last = RandomStart;
                }
                else
                    last = GetRandomColumn();

                return last;
            }
        }

        /// <summary>
        /// Whether this hit object can generate a note in the special column.
        /// </summary>
        private bool hasSpecialColumn => HitObject.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP) && HitObject.Samples.Any(s => s.Name == HitSampleInfo.HIT_FINISH);

        /// <summary>
        /// Generates a random pattern.
        /// </summary>
        /// <param name="p2">Probability for 2 notes to be generated.</param>
        /// <param name="p3">Probability for 3 notes to be generated.</param>
        /// <param name="p4">Probability for 4 notes to be generated.</param>
        /// <param name="p5">Probability for 5 notes to be generated.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateRandomPattern(double p2, double p3, double p4, double p5)
        {
            var pattern = new Pattern();

            pattern.Add(generateRandomNotes(getRandomNoteCount(p2, p3, p4, p5)));

            if (RandomStart > 0 && hasSpecialColumn)
                addToPattern(pattern, 0);

            return pattern;
        }

        /// <summary>
        /// Generates a random pattern which has both normal and mirrored notes.
        /// </summary>
        /// <param name="centreProbability">The probability for a note to be added to the centre column.</param>
        /// <param name="p2">Probability for 2 notes to be generated.</param>
        /// <param name="p3">Probability for 3 notes to be generated.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateRandomPatternWithMirrored(double centreProbability, double p2, double p3)
        {
            if (convertType.HasFlagFast(PatternType.ForceNotStack))
                return generateRandomPattern(1 / 2f + p2 / 2, p2, (p2 + p3) / 2, p3);

            var pattern = new Pattern();

            int noteCount = getRandomNoteCountMirrored(centreProbability, p2, p3, out bool addToCentre);

            int columnLimit = (TotalColumns % 2 == 0 ? TotalColumns : TotalColumns - 1) / 2;
            int nextColumn = GetRandomColumn(upperBound: columnLimit);

            for (int i = 0; i < noteCount; i++)
            {
                nextColumn = FindAvailableColumn(nextColumn, upperBound: columnLimit, patterns: pattern);

                // Add normal note
                addToPattern(pattern, nextColumn);
                // Add mirrored note
                addToPattern(pattern, RandomStart + TotalColumns - nextColumn - 1);
            }

            if (addToCentre)
                addToPattern(pattern, TotalColumns / 2);

            if (RandomStart > 0 && hasSpecialColumn)
                addToPattern(pattern, 0);

            return pattern;
        }

        /// <summary>
        /// Generates a count of notes to be generated from a list of probabilities.
        /// </summary>
        /// <param name="p2">Probability for 2 notes to be generated.</param>
        /// <param name="p3">Probability for 3 notes to be generated.</param>
        /// <param name="p4">Probability for 4 notes to be generated.</param>
        /// <param name="p5">Probability for 5 notes to be generated.</param>
        /// <returns>The amount of notes to be generated.</returns>
        private int getRandomNoteCount(double p2, double p3, double p4, double p5)
        {
            switch (TotalColumns)
            {
                case 2:
                    p2 = 0;
                    p3 = 0;
                    p4 = 0;
                    p5 = 0;
                    break;

                case 3:
                    p2 = Math.Min(p2, 0.1);
                    p3 = 0;
                    p4 = 0;
                    p5 = 0;
                    break;

                case 4:
                    p2 = Math.Min(p2, 0.23);
                    p3 = Math.Min(p3, 0.04);
                    p4 = 0;
                    p5 = 0;
                    break;

                case 5:
                    p3 = Math.Min(p3, 0.15);
                    p4 = Math.Min(p4, 0.03);
                    p5 = 0;
                    break;
            }

            if (HitObject.Samples.Any(s => s.Name == HitSampleInfo.HIT_CLAP))
                p2 = 1;

            return GetRandomNoteCount(p2, p3, p4, p5);
        }

        /// <summary>
        /// Generates a count of notes to be generated from a list of probabilities.
        /// </summary>
        /// <param name="centreProbability">The probability for a note to be added to the centre column.</param>
        /// <param name="p2">Probability for 2 notes to be generated.</param>
        /// <param name="p3">Probability for 3 notes to be generated.</param>
        /// <param name="addToCentre">Whether to add a note to the centre column.</param>
        /// <returns>The amount of notes to be generated. The note to be added to the centre column will NOT be part of this count.</returns>
        private int getRandomNoteCountMirrored(double centreProbability, double p2, double p3, out bool addToCentre)
        {
            switch (TotalColumns)
            {
                case 2:
                    centreProbability = 0;
                    p2 = 0;
                    p3 = 0;
                    break;

                case 3:
                    centreProbability = Math.Min(centreProbability, 0.03);
                    p2 = 0;
                    p3 = 0;
                    break;

                case 4:
                    centreProbability = 0;

                    // Stable requires rngValue > x, which is an inverse-probability. Lazer uses true probability (1 - x).
                    // But multiplying this value by 2 (stable) is not the same operation as dividing it by 2 (lazer),
                    // so it needs to be converted to from a probability and then back after the multiplication.
                    p2 = 1 - Math.Max((1 - p2) * 2, 0.8);
                    p3 = 0;
                    break;

                case 5:
                    centreProbability = Math.Min(centreProbability, 0.03);
                    p3 = 0;
                    break;

                case 6:
                    centreProbability = 0;

                    // Stable requires rngValue > x, which is an inverse-probability. Lazer uses true probability (1 - x).
                    // But multiplying this value by 2 (stable) is not the same operation as dividing it by 2 (lazer),
                    // so it needs to be converted to from a probability and then back after the multiplication.
                    p2 = 1 - Math.Max((1 - p2) * 2, 0.5);
                    p3 = 1 - Math.Max((1 - p3) * 2, 0.85);
                    break;
            }

            // The stable values were allowed to exceed 1, which indicate <0% probability.
            // These values needs to be clamped otherwise GetRandomNoteCount() will throw an exception.
            p2 = Math.Clamp(p2, 0, 1);
            p3 = Math.Clamp(p3, 0, 1);

            double centreVal = Random.NextDouble();
            int noteCount = GetRandomNoteCount(p2, p3);

            addToCentre = TotalColumns % 2 != 0 && noteCount != 3 && centreVal > 1 - centreProbability;
            return noteCount;
        }

        /// <summary>
        /// Constructs and adds a note to a pattern.
        /// </summary>
        /// <param name="pattern">The pattern to add to.</param>
        /// <param name="column">The column to add the note to.</param>
        private void addToPattern(Pattern pattern, int column)
        {
            pattern.Add(new Note
            {
                StartTime = HitObject.StartTime,
                Samples = HitObject.Samples,
                Column = column
            });
        }
    }
}
