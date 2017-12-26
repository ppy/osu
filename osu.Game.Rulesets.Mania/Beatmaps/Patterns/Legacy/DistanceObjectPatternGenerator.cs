﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.MathUtils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Mania.Beatmaps.Patterns.Legacy
{
    /// <summary>
    /// A pattern generator for IHasDistance hit objects.
    /// </summary>
    internal class DistanceObjectPatternGenerator : PatternGenerator
    {
        /// <summary>
        /// Base osu! slider scoring distance.
        /// </summary>
        private const float osu_base_scoring_distance = 100;

        private readonly double endTime;
        private readonly double segmentDuration;
        private readonly int repeatCount;

        private PatternType convertType;

        public DistanceObjectPatternGenerator(FastRandom random, HitObject hitObject, Beatmap beatmap, int availableColumns, Pattern previousPattern)
            : base(random, hitObject, beatmap, availableColumns, previousPattern)
        {
            convertType = PatternType.None;
            if (Beatmap.ControlPointInfo.EffectPointAt(hitObject.StartTime).KiaiMode)
                convertType = PatternType.LowProbability;

            var distanceData = hitObject as IHasDistance;
            var repeatsData = hitObject as IHasRepeats;

            repeatCount = repeatsData?.RepeatCount ?? 1;

            TimingControlPoint timingPoint = beatmap.ControlPointInfo.TimingPointAt(hitObject.StartTime);
            DifficultyControlPoint difficultyPoint = beatmap.ControlPointInfo.DifficultyPointAt(hitObject.StartTime);

            // The true distance, accounting for any repeats
            double distance = (distanceData?.Distance ?? 0) * repeatCount;
            // The velocity of the osu! hit object - calculated as the velocity of a slider
            double osuVelocity = osu_base_scoring_distance * beatmap.BeatmapInfo.BaseDifficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier / timingPoint.BeatLength;
            // The duration of the osu! hit object
            double osuDuration = distance / osuVelocity;

            endTime = hitObject.StartTime + osuDuration;
            segmentDuration = (endTime - HitObject.StartTime) / repeatCount;
        }

        public override Pattern Generate()
        {
            if (repeatCount > 1)
            {
                if (segmentDuration <= 90)
                    return generateRandomHoldNotes(HitObject.StartTime, 1);

                if (segmentDuration <= 120)
                {
                    convertType |= PatternType.ForceNotStack;
                    return generateRandomNotes(HitObject.StartTime, repeatCount + 1);
                }

                if (segmentDuration <= 160)
                    return generateStair(HitObject.StartTime);

                if (segmentDuration <= 200 && ConversionDifficulty > 3)
                    return generateRandomMultipleNotes(HitObject.StartTime);

                double duration = endTime - HitObject.StartTime;
                if (duration >= 4000)
                    return generateNRandomNotes(HitObject.StartTime, 0.23, 0, 0);

                if (segmentDuration > 400 && repeatCount < AvailableColumns - 1 - RandomStart)
                    return generateTiledHoldNotes(HitObject.StartTime);

                return generateHoldAndNormalNotes(HitObject.StartTime);
            }

            if (segmentDuration <= 110)
            {
                if (PreviousPattern.ColumnWithObjects < AvailableColumns)
                    convertType |= PatternType.ForceNotStack;
                else
                    convertType &= ~PatternType.ForceNotStack;
                return generateRandomNotes(HitObject.StartTime, segmentDuration < 80 ? 1 : 2);
            }

            if (ConversionDifficulty > 6.5)
            {
                if ((convertType & PatternType.LowProbability) > 0)
                    return generateNRandomNotes(HitObject.StartTime, 0.78, 0.3, 0);
                return generateNRandomNotes(HitObject.StartTime, 0.85, 0.36, 0.03);
            }

            if (ConversionDifficulty > 4)
            {
                if ((convertType & PatternType.LowProbability) > 0)
                    return generateNRandomNotes(HitObject.StartTime, 0.43, 0.08, 0);
                return generateNRandomNotes(HitObject.StartTime, 0.56, 0.18, 0);
            }

            if (ConversionDifficulty > 2.5)
            {
                if ((convertType & PatternType.LowProbability) > 0)
                    return generateNRandomNotes(HitObject.StartTime, 0.3, 0, 0);
                return generateNRandomNotes(HitObject.StartTime, 0.37, 0.08, 0);
            }

            if ((convertType & PatternType.LowProbability) > 0)
                return generateNRandomNotes(HitObject.StartTime, 0.17, 0, 0);
            return generateNRandomNotes(HitObject.StartTime, 0.27, 0, 0);
        }

        /// <summary>
        /// Generates random hold notes that start at an span the same amount of rows.
        /// </summary>
        /// <param name="startTime">Start time of each hold note.</param>
        /// <param name="noteCount">Number of hold notes.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateRandomHoldNotes(double startTime, int noteCount)
        {
            // - - - -
            // ■ - ■ ■
            // □ - □ □
            // ■ - ■ ■

            var pattern = new Pattern();

            int usableColumns = AvailableColumns - RandomStart - PreviousPattern.ColumnWithObjects;
            int nextColumn = Random.Next(RandomStart, AvailableColumns);
            for (int i = 0; i < Math.Min(usableColumns, noteCount); i++)
            {
                while (pattern.ColumnHasObject(nextColumn) || PreviousPattern.ColumnHasObject(nextColumn))  //find available column
                    nextColumn = Random.Next(RandomStart, AvailableColumns);
                addToPattern(pattern, nextColumn, startTime, endTime);
            }

            // This is can't be combined with the above loop due to RNG
            for (int i = 0; i < noteCount - usableColumns; i++)
            {
                while (pattern.ColumnHasObject(nextColumn))
                    nextColumn = Random.Next(RandomStart, AvailableColumns);
                addToPattern(pattern, nextColumn, startTime, endTime);
            }

            return pattern;
        }

        /// <summary>
        /// Generates random notes, with one note per row and no stacking.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="noteCount">The number of notes.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateRandomNotes(double startTime, int noteCount)
        {
            // - - - -
            // x - - -
            // - - x -
            // - - - x
            // x - - -

            var pattern = new Pattern();

            int nextColumn = GetColumn((HitObject as IHasXPosition)?.X ?? 0, true);
            if ((convertType & PatternType.ForceNotStack) > 0 && PreviousPattern.ColumnWithObjects < AvailableColumns)
            {
                while (PreviousPattern.ColumnHasObject(nextColumn))
                    nextColumn = Random.Next(RandomStart, AvailableColumns);
            }

            int lastColumn = nextColumn;
            for (int i = 0; i < noteCount; i++)
            {
                addToPattern(pattern, nextColumn, startTime, startTime);
                while (nextColumn == lastColumn)
                    nextColumn = Random.Next(RandomStart, AvailableColumns);

                lastColumn = nextColumn;
                startTime += segmentDuration;
            }

            return pattern;
        }

        /// <summary>
        /// Generates a stair of notes, with one note per row.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateStair(double startTime)
        {
            // - - - -
            // x - - -
            // - x - -
            // - - x -
            // - - - x
            // - - x -
            // - x - -
            // x - - -

            var pattern = new Pattern();

            int column = GetColumn((HitObject as IHasXPosition)?.X ?? 0, true);
            bool increasing = Random.NextDouble() > 0.5;

            for (int i = 0; i <= repeatCount; i++)
            {
                addToPattern(pattern, column, startTime, startTime);
                startTime += segmentDuration;

                // Check if we're at the borders of the stage, and invert the pattern if so
                if (increasing)
                {
                    if (column >= AvailableColumns - 1)
                    {
                        increasing = false;
                        column--;
                    }
                    else
                        column++;
                }
                else
                {
                    if (column <= RandomStart)
                    {
                        increasing = true;
                        column++;
                    }
                    else
                        column--;
                }
            }

            return pattern;
        }

        /// <summary>
        /// Generates random notes with 1-2 notes per row and no stacking.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateRandomMultipleNotes(double startTime)
        {
            // - - - -
            // x - - -
            // - x x -
            // - - - x
            // x - x -

            var pattern = new Pattern();

            bool legacy = AvailableColumns >= 4 && AvailableColumns <= 8;
            int interval = Random.Next(1, AvailableColumns - (legacy ? 1 : 0));

            int nextColumn = GetColumn((HitObject as IHasXPosition)?.X ?? 0, true);
            for (int i = 0; i <= repeatCount; i++)
            {
                addToPattern(pattern, nextColumn, startTime, startTime);

                nextColumn += interval;
                if (nextColumn >= AvailableColumns - RandomStart)
                    nextColumn = nextColumn - AvailableColumns - RandomStart + (legacy ? 1 : 0);
                nextColumn += RandomStart;

                // If we're in 2K, let's not add many consecutive doubles
                if (AvailableColumns > 2)
                    addToPattern(pattern, nextColumn, startTime, startTime);

                nextColumn = Random.Next(RandomStart, AvailableColumns);
                startTime += segmentDuration;
            }

            return pattern;
        }

        /// <summary>
        /// Generates random hold notes. The amount of hold notes generated is determined by probabilities.
        /// </summary>
        /// <param name="startTime">The hold note start time.</param>
        /// <param name="p2">The probability required for 2 hold notes to be generated.</param>
        /// <param name="p3">The probability required for 3 hold notes to be generated.</param>
        /// <param name="p4">The probability required for 4 hold notes to be generated.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateNRandomNotes(double startTime, double p2, double p3, double p4)
        {
            // - - - -
            // ■ - ■ ■
            // □ - □ □
            // ■ - ■ ■

            switch (AvailableColumns)
            {
                case 2:
                    p2 = 0;
                    p3 = 0;
                    p4 = 0;
                    break;
                case 3:
                    p2 = Math.Max(p2, 0.1);
                    p3 = 0;
                    p4 = 0;
                    break;
                case 4:
                    p2 = Math.Max(p2, 0.3);
                    p3 = Math.Max(p3, 0.04);
                    p4 = 0;
                    break;
                case 5:
                    p2 = Math.Max(p2, 0.34);
                    p3 = Math.Max(p3, 0.1);
                    p4 = Math.Max(p4, 0.03);
                    break;
            }

            Func<SampleInfo, bool> isDoubleSample = sample => sample.Name == SampleInfo.HIT_CLAP && sample.Name == SampleInfo.HIT_FINISH;

            bool canGenerateTwoNotes = (convertType & PatternType.LowProbability) == 0;
            canGenerateTwoNotes &= HitObject.Samples.Any(isDoubleSample) || sampleInfoListAt(HitObject.StartTime).Any(isDoubleSample);

            if (canGenerateTwoNotes)
                p2 = 1;

            return generateRandomHoldNotes(startTime, GetRandomNoteCount(p2, p3, p4));
        }

        /// <summary>
        /// Generates tiled hold notes. You can think of this as a stair of hold notes.
        /// </summary>
        /// <param name="startTime">The first hold note start time.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateTiledHoldNotes(double startTime)
        {
            // - - - -
            // ■ ■ ■ ■
            // □ □ □ □
            // □ □ □ □
            // □ □ □ ■
            // □ □ ■ -
            // □ ■ - -
            // ■ - - -

            var pattern = new Pattern();

            int columnRepeat = Math.Min(repeatCount, AvailableColumns);

            int nextColumn = GetColumn((HitObject as IHasXPosition)?.X ?? 0, true);
            if ((convertType & PatternType.ForceNotStack) > 0 && PreviousPattern.ColumnWithObjects < AvailableColumns)
            {
                while (PreviousPattern.ColumnHasObject(nextColumn))
                    nextColumn = Random.Next(RandomStart, AvailableColumns);
            }

            for (int i = 0; i < columnRepeat; i++)
            {
                while (pattern.ColumnHasObject(nextColumn))
                    nextColumn = Random.Next(RandomStart, AvailableColumns);

                addToPattern(pattern, nextColumn, startTime, endTime);
                startTime += segmentDuration;
            }

            return pattern;
        }

        /// <summary>
        /// Generates a hold note alongside normal notes.
        /// </summary>
        /// <param name="startTime">The start time of notes.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateHoldAndNormalNotes(double startTime)
        {
            // - - - -
            // ■ x x -
            // ■ - x x
            // ■ x - x
            // ■ - x x

            var pattern = new Pattern();

            int holdColumn = GetColumn((HitObject as IHasXPosition)?.X ?? 0, true);
            if ((convertType & PatternType.ForceNotStack) > 0 && PreviousPattern.ColumnWithObjects < AvailableColumns)
            {
                while (PreviousPattern.ColumnHasObject(holdColumn))
                    holdColumn = Random.Next(RandomStart, AvailableColumns);
            }

            // Create the hold note
            addToPattern(pattern, holdColumn, startTime, endTime);

            int noteCount = 1;
            if (ConversionDifficulty > 6.5)
                noteCount = GetRandomNoteCount(0.63, 0);
            else if (ConversionDifficulty > 4)
                noteCount = GetRandomNoteCount(AvailableColumns < 6 ? 0.12 : 0.45, 0);
            else if (ConversionDifficulty > 2.5)
                noteCount = GetRandomNoteCount(AvailableColumns < 6 ? 0 : 0.24, 0);
            noteCount = Math.Min(AvailableColumns - 1, noteCount);

            bool ignoreHead = !sampleInfoListAt(startTime).Any(s => s.Name == SampleInfo.HIT_WHISTLE || s.Name == SampleInfo.HIT_FINISH || s.Name == SampleInfo.HIT_CLAP);
            int nextColumn = Random.Next(RandomStart, AvailableColumns);

            var rowPattern = new Pattern();
            for (int i = 0; i <= repeatCount; i++)
            {
                if (!(ignoreHead && startTime == HitObject.StartTime))
                {
                    for (int j = 0; j < noteCount; j++)
                    {
                        while (rowPattern.ColumnHasObject(nextColumn) || nextColumn == holdColumn)
                            nextColumn = Random.Next(RandomStart, AvailableColumns);
                        addToPattern(rowPattern, nextColumn, startTime, startTime);
                    }
                }

                pattern.Add(rowPattern);
                rowPattern.Clear();

                startTime += segmentDuration;
            }

            return pattern;
        }

        /// <summary>
        /// Retrieves the sample info list at a point in time.
        /// </summary>
        /// <param name="time">The time to retrieve the sample info list from.</param>
        /// <returns></returns>
        private List<SampleInfo> sampleInfoListAt(double time)
        {
            var curveData = HitObject as IHasCurve;

            if (curveData == null)
                return HitObject.Samples;

            double segmentTime = (endTime - HitObject.StartTime) / repeatCount;

            int index = (int)(segmentTime == 0 ? 0 : (time - HitObject.StartTime) / segmentTime);
            return curveData.RepeatSamples[index];
        }

        /// <summary>
        /// Constructs and adds a note to a pattern.
        /// </summary>
        /// <param name="pattern">The pattern to add to.</param>
        /// <param name="column">The column to add the note to.</param>
        /// <param name="startTime">The start time of the note.</param>
        /// <param name="endTime">The end time of the note (set to <paramref name="startTime"/> for a non-hold note).</param>
        private void addToPattern(Pattern pattern, int column, double startTime, double endTime)
        {
            ManiaHitObject newObject;

            if (startTime == endTime)
            {
                newObject = new Note
                {
                    StartTime = startTime,
                    Samples = sampleInfoListAt(startTime),
                    Column = column
                };
            }
            else
            {
                var holdNote = new HoldNote
                {
                    StartTime = startTime,
                    Column = column,
                    Duration = endTime - startTime,
                    Head = { Samples = sampleInfoListAt(startTime) },
                    Tail = { Samples = sampleInfoListAt(endTime) }
                };

                newObject = holdNote;
            }

            pattern.Add(newObject);
        }
    }
}
