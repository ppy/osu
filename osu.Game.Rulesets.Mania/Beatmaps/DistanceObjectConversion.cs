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

namespace osu.Game.Rulesets.Mania.Beatmaps
{
    internal class DistanceObjectConversion : ObjectConversion
    {
        private readonly HitObject originalObject;

        private readonly double endTime;
        private readonly int repeatCount;

        private LegacyConvertType convertType;

        public DistanceObjectConversion(HitObject originalObject, ObjectList previousObjects, FastRandom random, Beatmap beatmap)
            : base(previousObjects, random, beatmap)
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

        public override ObjectList Generate()
        {
            double segmentDuration = endTime / repeatCount;

            if (repeatCount > 1)
            {
                if (segmentDuration <= 90)
                    return generateRandomHoldNotes(originalObject.StartTime, endTime, 1);

                if (segmentDuration <= 120)
                {
                    convertType |= LegacyConvertType.ForceNotStack;
                    return generateRandomNotes(originalObject.StartTime, segmentDuration, repeatCount);
                }

                if (segmentDuration <= 160)
                    return generateStair(originalObject.StartTime, segmentDuration, repeatCount);

                if (segmentDuration <= 200 && conversionDifficulty > 3)
                    return generateRandomMultipleNotes(originalObject.StartTime, segmentDuration, repeatCount);

                double duration = endTime - originalObject.StartTime;
                if (duration >= 4000)
                    return generateNRandomNotes(originalObject.StartTime, endTime, 0.23, 0, 0);

                if (segmentDuration > 400 && duration < 4000 && repeatCount < AvailableColumns - 1 - RandomStart)
                    return generateTiledHoldNotes(originalObject.StartTime, segmentDuration, repeatCount);

                return generateHoldAndNormalNotes(originalObject.StartTime, segmentDuration);
            }

            if (segmentDuration <= 110)
            {
                if (PreviousObjects.ColumnsFilled < AvailableColumns)
                    convertType |= LegacyConvertType.ForceNotStack;
                else
                    convertType &= ~LegacyConvertType.ForceNotStack;
                return generateRandomNotes(originalObject.StartTime, segmentDuration, segmentDuration < 80 ? 0 : 1);
            }

            if (conversionDifficulty > 6.5)
            {
                if ((convertType & LegacyConvertType.LowProbability) > 0)
                    return generateNRandomNotes(originalObject.StartTime, endTime, 0.78, 0.3, 0);
                return generateNRandomNotes(originalObject.StartTime, endTime, 0.85, 0.36, 0.03);
            }

            if (conversionDifficulty > 4)
            {
                if ((convertType & LegacyConvertType.LowProbability) > 0)
                    return generateNRandomNotes(originalObject.StartTime, endTime, 0.43, 0.08, 0);
                return generateNRandomNotes(originalObject.StartTime, endTime, 0.56, 0.18, 0);
            }

            if (conversionDifficulty > 2.5)
            {
                if ((convertType & LegacyConvertType.LowProbability) > 0)
                    return generateNRandomNotes(originalObject.StartTime, endTime, 0.3, 0, 0);
                return generateNRandomNotes(originalObject.StartTime, endTime, 0.37, 0.08, 0);
            }

            if ((convertType & LegacyConvertType.LowProbability) > 0)
                return generateNRandomNotes(originalObject.StartTime, endTime, 0.17, 0, 0);
            return generateNRandomNotes(originalObject.StartTime, endTime, 0.27, 0, 0);
        }

        /// <summary>
        /// Generates random hold notes that start at an span the same amount of rows.
        /// </summary>
        /// <param name="startTime">Start time of each hold note.</param>
        /// <param name="endTime">End time of the hold notes.</param>
        /// <param name="noteCount">Number of hold notes.</param>
        /// <returns>The <see cref="ObjectList"/> containing the hit objects.</returns>
        private ObjectList generateRandomHoldNotes(double startTime, double endTime, int noteCount)
        {
            // - - - -
            // ■ - ■ ■
            // □ - □ □
            // ■ - ■ ■

            var newObjects = new ObjectList();

            int usableColumns = AvailableColumns - RandomStart - PreviousObjects.ColumnsFilled;
            int nextColumn = Random.Next(RandomStart, AvailableColumns);
            for (int i = 0; i < Math.Min(usableColumns, noteCount); i++)
            {
                while (newObjects.IsFilled(nextColumn) || PreviousObjects.IsFilled(nextColumn))  //find available column
                    nextColumn = Random.Next(RandomStart, AvailableColumns);
                add(newObjects, nextColumn, startTime, endTime, noteCount);
            }

            // This is can't be combined with the above loop due to RNG
            for (int i = 0; i < noteCount - usableColumns; i++)
            {
                while (newObjects.IsFilled(nextColumn))
                    nextColumn = Random.Next(RandomStart, AvailableColumns);
                add(newObjects, nextColumn, startTime, endTime, noteCount);
            }

            return newObjects;
        }

        /// <summary>
        /// Generates random notes, with one note per row and no stacking.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="separationTime">The separation of notes between rows.</param>
        /// <param name="repeatCount">The number of rows.</param>
        /// <returns>The <see cref="ObjectList"/> containing the hit objects.</returns>
        private ObjectList generateRandomNotes(double startTime, double separationTime, int repeatCount)
        {
            // - - - -
            // x - - -
            // - - x -
            // - - - x
            // x - - -

            var newObjects = new ObjectList();

            int nextColumn = GetColumn((originalObject as IHasXPosition)?.X ?? 0, true);
            if ((convertType & LegacyConvertType.ForceNotStack) > 0 && PreviousObjects.ColumnsFilled < AvailableColumns)
            {
                while (PreviousObjects.IsFilled(nextColumn))
                    nextColumn = Random.Next(RandomStart, AvailableColumns);
            }

            int lastColumn = nextColumn;
            for (int i = 0; i <= repeatCount; i++)
            {
                add(newObjects, nextColumn, startTime, startTime);
                while (nextColumn == lastColumn)
                    nextColumn = Random.Next(RandomStart, AvailableColumns);

                lastColumn = nextColumn;
                startTime += separationTime;
            }

            return newObjects;
        }

        /// <summary>
        /// Generates a stair of notes, with one note per row.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="separationTime">The separation of notes between rows.</param>
        /// <param name="repeatCount">The number of rows/notes.</param>
        /// <returns>The <see cref="ObjectList"/> containing the hit objects.</returns>
        private ObjectList generateStair(double startTime, double separationTime, int repeatCount)
        {
            // - - - -
            // x - - -
            // - x - -
            // - - x -
            // - - - x
            // - - x -
            // - x - -
            // x - - -

            var newObjects = new ObjectList();

            int column = GetColumn((originalObject as IHasXPosition)?.X ?? 0, true);
            bool increasing = Random.NextDouble() > 0.5;

            for (int i = 0; i <= repeatCount; i++)
            {
                add(newObjects, column, startTime, startTime);
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

            return newObjects;
        }

        /// <summary>
        /// Generates random notes with 1-2 notes per row and no stacking.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="separationTime">The separation of notes between rows.</param>
        /// <param name="repeatCount">The number of rows.</param>
        /// <returns>The <see cref="ObjectList"/> containing the hit objects.</returns>
        private ObjectList generateRandomMultipleNotes(double startTime, double separationTime, int repeatCount)
        {
            // - - - -
            // x - - 
            // - x x -
            // - - - x
            // x - x -

            var newObjects = new ObjectList();

            bool legacy = AvailableColumns >= 4 && AvailableColumns <= 8;
            int interval = Random.Next(1, AvailableColumns - (legacy ? 1 : 0));

            int nextColumn = GetColumn((originalObject as IHasXPosition)?.X ?? 0, true);
            for (int i = 0; i <= repeatCount; i++)
            {
                add(newObjects, nextColumn, startTime, startTime, 2);

                nextColumn += interval;
                if (nextColumn >= AvailableColumns - RandomStart)
                    nextColumn = nextColumn - AvailableColumns - RandomStart + (legacy ? 1 : 0);
                nextColumn += RandomStart;

                // If we're in 2K, let's not add many consecutive doubles
                if (AvailableColumns > 2)
                    add(newObjects, nextColumn, startTime, startTime, 2);

                nextColumn = Random.Next(RandomStart, AvailableColumns);
                startTime += separationTime;
            }

            return newObjects;
        }

        /// <summary>
        /// Generates random hold notes. The amount of hold notes generated is determined by probabilities.
        /// </summary>
        /// <param name="startTime">The hold note start time.</param>
        /// <param name="endTime">The hold note end time.</param>
        /// <param name="p2">The probability required for 2 hold notes to be generated.</param>
        /// <param name="p3">The probability required for 3 hold notes to be generated.</param>
        /// <param name="p4">The probability required for 4 hold notes to be generated.</param>
        /// <returns>The <see cref="ObjectList"/> containing the hit objects.</returns>
        private ObjectList generateNRandomNotes(double startTime, double endTime, double p2, double p3, double p4)
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

            bool canGenerateTwoNotes = (convertType & LegacyConvertType.LowProbability) == 0;
            canGenerateTwoNotes &= originalObject.Samples.Any(isDoubleSample) || sampleInfoListAt(originalObject.StartTime).Any(isDoubleSample);

            if (canGenerateTwoNotes)
                p2 = 1;

            return generateRandomHoldNotes(startTime, endTime, GetRandomNoteCount(p2, p3, p4));
        }

        /// <summary>
        /// Generates tiled hold notes. You can think of this as a stair of hold notes.
        /// </summary>
        /// <param name="startTime">The first hold note start time.</param>
        /// <param name="separationTime">The separation time between hold notes.</param>
        /// <param name="noteCount">The amount of hold notes.</param>
        /// <returns>The <see cref="ObjectList"/> containing the hit objects.</returns>
        private ObjectList generateTiledHoldNotes(double startTime, double separationTime, int noteCount)
        {
            // - - - -
            // ■ ■ ■ ■
            // □ □ □ □
            // □ □ □ □
            // □ □ □ ■
            // □ □ ■ -
            // □ ■ - -
            // ■ - - -

            var newObjects = new ObjectList();

            int columnRepeat = Math.Min(noteCount, AvailableColumns);

            int nextColumn = GetColumn((originalObject as IHasXPosition)?.X ?? 0, true);
            if ((convertType & LegacyConvertType.ForceNotStack) > 0 && PreviousObjects.ColumnsFilled < AvailableColumns)
            {
                while (PreviousObjects.IsFilled(nextColumn))
                    nextColumn = Random.Next(RandomStart, AvailableColumns);
            }

            for (int i = 0; i < columnRepeat; i++)
            {
                while (newObjects.IsFilled(nextColumn))
                    nextColumn = Random.Next(RandomStart, AvailableColumns);

                add(newObjects, nextColumn, startTime, endTime, noteCount);
                startTime += separationTime;
            }

            return newObjects;
        }

        /// <summary>
        /// Generates a hold note alongside normal notes.
        /// </summary>
        /// <param name="startTime">The start time of notes.</param>
        /// <param name="separationTime">The separation time between notes.</param>
        /// <returns>The <see cref="ObjectList"/> containing the hit objects.</returns>
        private ObjectList generateHoldAndNormalNotes(double startTime, double separationTime)
        {
            // - - - -
            // ■ x x -
            // ■ - x x
            // ■ x - x
            // ■ - x x

            var newObjects = new ObjectList();

            int holdColumn = GetColumn((originalObject as IHasXPosition)?.X ?? 0, true);
            if ((convertType & LegacyConvertType.ForceNotStack) > 0 && PreviousObjects.ColumnsFilled < AvailableColumns)
            {
                while (PreviousObjects.IsFilled(holdColumn))
                    holdColumn = Random.Next(RandomStart, AvailableColumns);
            }

            // Create the hold note
            add(newObjects, holdColumn, startTime, separationTime * repeatCount);

            int noteCount = 1;
            if (conversionDifficulty > 6.5)
                noteCount = GetRandomNoteCount(0.63, 0);
            else if (conversionDifficulty > 4)
                noteCount = GetRandomNoteCount(AvailableColumns < 6 ? 0.12 : 0.45, 0);
            else if (conversionDifficulty > 2.5)
                noteCount = GetRandomNoteCount(AvailableColumns < 6 ? 0 : 0.24, 0);
            noteCount = Math.Min(AvailableColumns - 1, noteCount);

            bool ignoreHead = !sampleInfoListAt(startTime).Any(s => s.Name == SampleInfo.HIT_WHISTLE || s.Name == SampleInfo.HIT_FINISH || s.Name == SampleInfo.HIT_CLAP);
            int nextColumn = Random.Next(RandomStart, AvailableColumns);

            var tempObjects = new ObjectList();
            for (int i = 0; i <= repeatCount; i++)
            {
                if (!(ignoreHead && startTime == originalObject.StartTime))
                {
                    for (int j = 0; j < noteCount; j++)
                    {
                        while (tempObjects.IsFilled(nextColumn) || nextColumn == holdColumn)
                            nextColumn = Random.Next(RandomStart, AvailableColumns);
                        add(tempObjects, nextColumn, startTime, startTime, noteCount + 1);
                    }
                }

                newObjects.Add(tempObjects);
                tempObjects.Clear();

                startTime += separationTime;
            }

            return newObjects;
        }

        /// <summary>
        /// Constructs and adds a note to an object list.
        /// </summary>
        /// <param name="objectList">The list to add to.</param>
        /// <param name="column">The column to add the note to.</param>
        /// <param name="startTime">The start time of the note.</param>
        /// <param name="endTime">The end time of the note (set to <paramref name="startTime"/> for a non-hold note).</param>
        /// <param name="siblings">The number of children alongside this note (these will not be generated, but are used for volume calculations).</param>
        private void add(ObjectList objectList, int column, double startTime, double endTime, int siblings = 1)
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

            objectList.Add(newObject);
        }

        /// <summary>
        /// Retrieves the sample info list at a point in time.
        /// </summary>
        /// <param name="time">The time to retrieve the sample info list from.</param>
        /// <param name="separationTime"></param>
        /// <returns></returns>
        private SampleInfoList sampleInfoListAt(double time)
        {
            var curveData = originalObject as IHasCurve;

            if (curveData == null)
                return originalObject.Samples;

            double segmentTime = (curveData.EndTime - originalObject.StartTime) / repeatCount;

            int index = (int)(segmentTime == 0 ? 0 : (time - originalObject.StartTime) / segmentTime);
            return curveData.RepeatSamples[index];
        }

        private double? _conversionDifficulty;
        /// <summary>
        /// A difficulty factor used for various conversion methods from osu!stable.
        /// </summary>
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
