// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Timing;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using System;
using osu.Game.Rulesets.Mania.MathUtils;
using System.Linq;
using OpenTK;
using osu.Game.Database;
using osu.Game.Audio;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    internal class DistanceObjectConversion : ObjectConversion
    {
        private readonly HitObject originalObject;

        private readonly double endTime;
        private readonly int repeatCount;

        private LegacyConvertType convertType;

        public DistanceObjectConversion(HitObject originalObject, ObjectRow previousRow, FastRandom random, Beatmap beatmap)
            : base(previousRow, random, beatmap)
        {
            this.originalObject = originalObject;

            ControlPoint overridePoint;
            ControlPoint controlPoint = Beatmap.TimingInfo.TimingPointAt(originalObject.StartTime, out overridePoint);

            convertType = LegacyConvertType.None;
            if ((overridePoint ?? controlPoint)?.KiaiMode == false)
                convertType = LegacyConvertType.LowProbability;

            var distanceData = originalObject as IHasDistance;
            var repeatsData = originalObject as IHasRepeats;

            endTime = distanceData?.EndTime ?? 0;
            repeatCount = repeatsData?.RepeatCount ?? 1;
        }

        public override ObjectRow GenerateConversion()
        {
            double segmentDuration = endTime / repeatCount;

            if (repeatCount > 1)
            {
                if (segmentDuration <= 90)
                    return generateRandomHoldNotes(originalObject.StartTime, endTime, 1);

                if (segmentDuration <= 120)
                {
                    convertType |= LegacyConvertType.ForceNotStack;
                    return addRandomNotes(originalObject.StartTime, segmentDuration, repeatCount);
                }

                if (segmentDuration <= 160)
                    return addStair(originalObject.StartTime, segmentDuration, repeatCount);

                if (segmentDuration <= 200 && conversionDifficulty > 3)
                    return addMultipleNotes(originalObject.StartTime, segmentDuration, repeatCount);

                double duration = endTime - originalObject.StartTime;
                if (duration >= 4000)
                    return addNRandomNotes(originalObject.StartTime, endTime, 0.23, 0, 0);

                if (segmentDuration > 400 && duration < 4000 && repeatCount < AvailableColumns - 1 - RandomStart)
                    return generateTiledHoldNotes(originalObject.StartTime, segmentDuration, repeatCount);

                return generateLongAndNormalNotes(originalObject.StartTime, segmentDuration);
            }

            if (segmentDuration <= 110)
            {
                if (PreviousRow.Columns < AvailableColumns)
                    convertType |= LegacyConvertType.ForceNotStack;
                else
                    convertType &= ~LegacyConvertType.ForceNotStack;
                return addRandomNotes(originalObject.StartTime, segmentDuration, segmentDuration < 80 ? 0 : 1);
            }

            if (conversionDifficulty > 6.5)
            {
                if ((convertType & LegacyConvertType.LowProbability) > 0)
                    return addNRandomNotes(originalObject.StartTime, endTime, 0.78, 0.3, 0);
                return addNRandomNotes(originalObject.StartTime, endTime, 0.85, 0.36, 0.03);
            }

            if (conversionDifficulty > 4)
            {
                if ((convertType & LegacyConvertType.LowProbability) > 0)
                    return addNRandomNotes(originalObject.StartTime, endTime, 0.43, 0.08, 0);
                return addNRandomNotes(originalObject.StartTime, endTime, 0.56, 0.18, 0);
            }

            if (conversionDifficulty > 2.5)
            {
                if ((convertType & LegacyConvertType.LowProbability) > 0)
                    return addNRandomNotes(originalObject.StartTime, endTime, 0.3, 0, 0);
                return addNRandomNotes(originalObject.StartTime, endTime, 0.37, 0.08, 0);
            }

            if ((convertType & LegacyConvertType.LowProbability) > 0)
                return addNRandomNotes(originalObject.StartTime, endTime, 0.17, 0, 0);
            return addNRandomNotes(originalObject.StartTime, endTime, 0.27, 0, 0);
        }

        /// <summary>
        /// Adds random hold notes.
        /// </summary>
        /// <param name="count">Number of hold notes.</param>
        /// <param name="startTime">Start time of each hold note.</param>
        /// <param name="endTime">End time of the hold notes.</param>
        /// <returns>The new row.</returns>
        private ObjectRow generateRandomHoldNotes(double startTime, double endTime, int count)
        {
            var newRow = new ObjectRow();

            int usableColumns = AvailableColumns - RandomStart - PreviousRow.Columns;
            int nextColumn = Random.Next(RandomStart, AvailableColumns);
            for (int i = 0; i < Math.Min(usableColumns, count); i++)
            {
                while (newRow.IsTaken(nextColumn) || PreviousRow.IsTaken(nextColumn))  //find available column
                    nextColumn = Random.Next(RandomStart, AvailableColumns);
                add(newRow, nextColumn, startTime, endTime, count);
            }

            // This is can't be combined with the above loop due to RNG
            for (int i = 0; i < count - usableColumns; i++)
            {
                while (newRow.IsTaken(nextColumn))
                    nextColumn = Random.Next(RandomStart, AvailableColumns);
                add(newRow, nextColumn, startTime, endTime, count);
            }

            return newRow;
        }

        /// <summary>
        /// Adds random notes, with one note per row. No stacking.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="separationTime">The separation of notes between rows.</param>
        /// <param name="repeatCount">The number of rows.</param>
        /// <returns>The new row.</returns>
        private ObjectRow addRandomNotes(double startTime, double separationTime, int repeatCount)
        {
            var newRow = new ObjectRow();

            int nextColumn = GetColumn((originalObject as IHasXPosition)?.X ?? 0, true);
            if ((convertType & LegacyConvertType.ForceNotStack) > 0 && PreviousRow.Columns < AvailableColumns)
            {
                while (PreviousRow.IsTaken(nextColumn))
                    nextColumn = Random.Next(RandomStart, AvailableColumns);
            }

            int lastColumn = nextColumn;
            for (int i = 0; i <= repeatCount; i++)
            {
                add(newRow, nextColumn, startTime, startTime);
                while (nextColumn == lastColumn)
                    nextColumn = Random.Next(RandomStart, AvailableColumns);

                lastColumn = nextColumn;
                startTime += separationTime;
            }

            return newRow;
        }

        /// <summary>
        /// Creates a stair of notes, with one note per row.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="separationTime">The separation of notes between rows.</param>
        /// <param name="repeatCount">The number of rows/notes.</param>
        /// <returns>The new row.</returns>
        private ObjectRow addStair(double startTime, double separationTime, int repeatCount)
        {
            var newRow = new ObjectRow();

            int column = GetColumn((originalObject as IHasXPosition)?.X ?? 0, true);
            bool increasing = Random.NextDouble() > 0.5;

            for (int i = 0; i <= repeatCount; i++)
            {
                add(newRow, column, startTime, startTime);
                startTime += separationTime;
                
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

            return newRow;
        }

        /// <summary>
        /// Adds random notes, with 1-2 notes per row. No stacking.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="separationTime">The separation of notes between rows.</param>
        /// <param name="repeatCount">The number of rows.</param>
        /// <returns>The new row.</returns>
        private ObjectRow addMultipleNotes(double startTime, double separationTime, int repeatCount)
        {
            var newRow = new ObjectRow();

            bool legacy = AvailableColumns >= 4 && AvailableColumns <= 8;
            int interval = Random.Next(1, AvailableColumns - (legacy ? 1 : 0));

            int nextColumn = GetColumn((originalObject as IHasXPosition)?.X ?? 0, true);
            for (int i = 0; i <= repeatCount; i++)
            {
                add(newRow, nextColumn, startTime, startTime, 2);

                nextColumn += interval;
                if (nextColumn >= AvailableColumns - RandomStart)
                    nextColumn = nextColumn - AvailableColumns - RandomStart + (legacy ? 1 : 0);
                nextColumn += RandomStart;

                // If we're in 2K, let's not add many consecutive doubles
                if (AvailableColumns > 2)
                    add(newRow, nextColumn, startTime, startTime, 2);

                nextColumn = Random.Next(RandomStart, AvailableColumns);
                startTime += separationTime;
            }

            return newRow;
        }

        /// <summary>
        /// Generates random hold notes. The amount of hold notes generated is determined by probabilities.
        /// </summary>
        /// <param name="startTime">The hold note start time.</param>
        /// <param name="endTime">The hold note end time.</param>
        /// <param name="p2">The probability required for 2 hold notes to be generated.</param>
        /// <param name="p3">The probability required for 3 hold notes to be generated.</param>
        /// <param name="p4">The probability required for 4 hold notes to be generated.</param>
        /// <returns>The new row.</returns>
        private ObjectRow addNRandomNotes(double startTime, double endTime, double p2, double p3, double p4)
        {
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

            bool canGenerateTwoNotes = (convertType & LegacyConvertType.LowProbability) == 0;
            canGenerateTwoNotes &= originalObject.Samples.Any(isDoubleSample) || sampleInfoListAt(originalObject.StartTime, originalObject.StartTime - endTime).Any(isDoubleSample);

            if (canGenerateTwoNotes)
                p2 = 0;

            return generateRandomHoldNotes(startTime, endTime, GetRandomNoteCount(p2, p3, p4));
        }

        private ObjectRow generateTiledHoldNotes(double startTime, double separationTime, int noteCount)
        {
            var newRow = new ObjectRow();

            int columnRepeat = Math.Min(noteCount, AvailableColumns);

            int nextColumn = GetColumn((originalObject as IHasXPosition)?.X ?? 0, true);
            if ((convertType & LegacyConvertType.ForceNotStack) > 0 && PreviousRow.Columns < AvailableColumns)
            {
                while (PreviousRow.IsTaken(nextColumn))
                    nextColumn = Random.Next(RandomStart, AvailableColumns);
            }

            for (int i = 0; i < columnRepeat; i++)
            {
                while (newRow.IsTaken(nextColumn))
                    nextColumn = Random.Next(RandomStart, AvailableColumns);

                add(newRow, nextColumn, startTime, endTime, noteCount);
                startTime += separationTime;
            }

            return newRow;
        }

        private ObjectRow generateLongAndNormalNotes(double startTime, double separationTime)
        {
            var newRow = new ObjectRow();

            int holdColumn = GetColumn((originalObject as IHasXPosition)?.X ?? 0, true);
            if ((convertType & LegacyConvertType.ForceNotStack) > 0 && PreviousRow.Columns < AvailableColumns)
            {
                while (PreviousRow.IsTaken(holdColumn))
                    holdColumn = Random.Next(RandomStart, AvailableColumns);
            }

            // Create the hold note
            add(newRow, holdColumn, startTime, separationTime * repeatCount);

            int noteCount = 0;
            if (conversionDifficulty > 6.5)
                noteCount = GetRandomNoteCount(0.63, 0);
            else if (conversionDifficulty > 4)
                noteCount = GetRandomNoteCount(AvailableColumns < 6 ? 0.12 : 0.45, 0);
            else if (conversionDifficulty > 2.5)
                noteCount = GetRandomNoteCount(AvailableColumns < 6 ? 0 : 0.24, 0);
            noteCount = Math.Min(AvailableColumns - 1, noteCount);

            bool ignoreHead = !sampleInfoListAt(startTime, separationTime).Any(s => s.Name == SampleInfo.HIT_WHISTLE || s.Name == SampleInfo.HIT_FINISH || s.Name == SampleInfo.HIT_CLAP);
            int nextColumn = Random.Next(RandomStart, AvailableColumns);

            var tempRow = new ObjectRow();
            for (int i = 0; i <= repeatCount; i++)
            {
                if (!(ignoreHead && startTime == originalObject.StartTime))
                {
                    for (int j = 0; j < noteCount; j++)
                    {
                        while (tempRow.IsTaken(nextColumn) || nextColumn == holdColumn)
                            nextColumn = Random.Next(RandomStart, AvailableColumns);
                        add(tempRow, nextColumn, startTime, startTime, noteCount + 1);
                    }
                }

                foreach (ManiaHitObject obj in tempRow.HitObjects)
                    newRow.Add(obj);

                tempRow.Clear();
                startTime += separationTime;
            }

            return newRow;
        }

        private void add(ObjectRow row, int column, double startTime, double endTime, int siblings = 1)
        {
            ManiaHitObject newObject;

            if (startTime == endTime)
            {
                newObject = new Note
                {
                    StartTime = startTime,
                    Samples = originalObject.Samples,
                    Column = column
                };
            }
            else
            {
                newObject = new HoldNote
                {
                    StartTime = startTime,
                    Samples = originalObject.Samples,
                    Column = column,
                    Duration = endTime - startTime
                };
            }
            
            // Todo: Consider siblings and write sample volumes (probably at ManiaHitObject level)

            row.Add(newObject);
        }

        private SampleInfoList sampleInfoListAt(double time, double separationTime)
        {
            var curveData = originalObject as IHasCurve;

            if (curveData == null)
                return originalObject.Samples;

            int index = (int)(separationTime == 0 ? 0 : (time - originalObject.StartTime) / separationTime);
            return curveData.RepeatSamples[index];
        }

        private double? _conversionDifficulty;
        private double conversionDifficulty
        {
            get
            {
                if (_conversionDifficulty != null)
                    return _conversionDifficulty.Value;

                HitObject lastObject = Beatmap.HitObjects.LastOrDefault();
                HitObject firstObject = Beatmap.HitObjects.FirstOrDefault();

                double drainTime = (lastObject?.StartTime ?? 0) - (firstObject?.StartTime ?? 0);
                drainTime -= Beatmap.EventInfo.TotalBreakTime;

                if (drainTime == 0)
                    drainTime = 10000;

                BeatmapDifficulty difficulty = Beatmap.BeatmapInfo.Difficulty;
                _conversionDifficulty = ((difficulty.DrainRate + MathHelper.Clamp(difficulty.ApproachRate, 4, 7)) / 1.5 + Beatmap.HitObjects.Count / drainTime * 9f) / 38f * 5f / 1.15;
                _conversionDifficulty = Math.Min(_conversionDifficulty.Value, 12);

                return _conversionDifficulty.Value;
            }
        }
    }
}
