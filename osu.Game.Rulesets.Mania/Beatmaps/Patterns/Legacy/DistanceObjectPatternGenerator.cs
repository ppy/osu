// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Extensions.EnumExtensions;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mania.MathUtils;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Beatmaps.Formats;

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

        public readonly int StartTime;
        public readonly int EndTime;
        public readonly int SegmentDuration;
        public readonly int SpanCount;

        private PatternType convertType;

        public DistanceObjectPatternGenerator(FastRandom random, HitObject hitObject, ManiaBeatmap beatmap, Pattern previousPattern, IBeatmap originalBeatmap)
            : base(random, hitObject, beatmap, previousPattern, originalBeatmap)
        {
            convertType = PatternType.None;
            if (!Beatmap.ControlPointInfo.EffectPointAt(hitObject.StartTime).KiaiMode)
                convertType = PatternType.LowProbability;

            var distanceData = hitObject as IHasDistance;
            var repeatsData = hitObject as IHasRepeats;

            Debug.Assert(distanceData != null);

            TimingControlPoint timingPoint = beatmap.ControlPointInfo.TimingPointAt(hitObject.StartTime);
            DifficultyControlPoint difficultyPoint = hitObject.DifficultyControlPoint;

            double beatLength;
#pragma warning disable 618
            if (difficultyPoint is LegacyBeatmapDecoder.LegacyDifficultyControlPoint legacyDifficultyPoint)
#pragma warning restore 618
                beatLength = timingPoint.BeatLength * legacyDifficultyPoint.BpmMultiplier;
            else
                beatLength = timingPoint.BeatLength / difficultyPoint.SliderVelocity;

            SpanCount = repeatsData?.SpanCount() ?? 1;
            StartTime = (int)Math.Round(hitObject.StartTime);

            // This matches stable's calculation.
            EndTime = (int)Math.Floor(StartTime + distanceData.Distance * beatLength * SpanCount * 0.01 / beatmap.Difficulty.SliderMultiplier);

            SegmentDuration = (EndTime - StartTime) / SpanCount;
        }

        public override IEnumerable<Pattern> Generate()
        {
            var originalPattern = generate();

            if (originalPattern.HitObjects.Count() == 1)
            {
                yield return originalPattern;

                yield break;
            }

            // We need to split the intermediate pattern into two new patterns:
            // 1. A pattern containing all objects that do not end at our EndTime.
            // 2. A pattern containing all objects that end at our EndTime. This will be used for further pattern generation.
            var intermediatePattern = new Pattern();
            var endTimePattern = new Pattern();

            foreach (var obj in originalPattern.HitObjects)
            {
                if (EndTime != (int)Math.Round(obj.GetEndTime()))
                    intermediatePattern.Add(obj);
                else
                    endTimePattern.Add(obj);
            }

            yield return intermediatePattern;
            yield return endTimePattern;
        }

        private Pattern generate()
        {
            if (TotalColumns == 1)
            {
                var pattern = new Pattern();
                addToPattern(pattern, 0, StartTime, EndTime);
                return pattern;
            }

            if (SpanCount > 1)
            {
                if (SegmentDuration <= 90)
                    return generateRandomHoldNotes(StartTime, 1);

                if (SegmentDuration <= 120)
                {
                    convertType |= PatternType.ForceNotStack;
                    return generateRandomNotes(StartTime, SpanCount + 1);
                }

                if (SegmentDuration <= 160)
                    return generateStair(StartTime);

                if (SegmentDuration <= 200 && ConversionDifficulty > 3)
                    return generateRandomMultipleNotes(StartTime);

                double duration = EndTime - StartTime;
                if (duration >= 4000)
                    return generateNRandomNotes(StartTime, 0.23, 0, 0);

                if (SegmentDuration > 400 && SpanCount < TotalColumns - 1 - RandomStart)
                    return generateTiledHoldNotes(StartTime);

                return generateHoldAndNormalNotes(StartTime);
            }

            if (SegmentDuration <= 110)
            {
                if (PreviousPattern.ColumnWithObjects < TotalColumns)
                    convertType |= PatternType.ForceNotStack;
                else
                    convertType &= ~PatternType.ForceNotStack;
                return generateRandomNotes(StartTime, SegmentDuration < 80 ? 1 : 2);
            }

            if (ConversionDifficulty > 6.5)
            {
                if (convertType.HasFlagFast(PatternType.LowProbability))
                    return generateNRandomNotes(StartTime, 0.78, 0.3, 0);

                return generateNRandomNotes(StartTime, 0.85, 0.36, 0.03);
            }

            if (ConversionDifficulty > 4)
            {
                if (convertType.HasFlagFast(PatternType.LowProbability))
                    return generateNRandomNotes(StartTime, 0.43, 0.08, 0);

                return generateNRandomNotes(StartTime, 0.56, 0.18, 0);
            }

            if (ConversionDifficulty > 2.5)
            {
                if (convertType.HasFlagFast(PatternType.LowProbability))
                    return generateNRandomNotes(StartTime, 0.3, 0, 0);

                return generateNRandomNotes(StartTime, 0.37, 0.08, 0);
            }

            if (convertType.HasFlagFast(PatternType.LowProbability))
                return generateNRandomNotes(StartTime, 0.17, 0, 0);

            return generateNRandomNotes(StartTime, 0.27, 0, 0);
        }

        /// <summary>
        /// Generates random hold notes that start at an span the same amount of rows.
        /// </summary>
        /// <param name="startTime">Start time of each hold note.</param>
        /// <param name="noteCount">Number of hold notes.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateRandomHoldNotes(int startTime, int noteCount)
        {
            // - - - -
            // ■ - ■ ■
            // □ - □ □
            // ■ - ■ ■

            var pattern = new Pattern();

            int usableColumns = TotalColumns - RandomStart - PreviousPattern.ColumnWithObjects;
            int nextColumn = GetRandomColumn();

            for (int i = 0; i < Math.Min(usableColumns, noteCount); i++)
            {
                // Find available column
                nextColumn = FindAvailableColumn(nextColumn, pattern, PreviousPattern);
                addToPattern(pattern, nextColumn, startTime, EndTime);
            }

            // This is can't be combined with the above loop due to RNG
            for (int i = 0; i < noteCount - usableColumns; i++)
            {
                nextColumn = FindAvailableColumn(nextColumn, pattern);
                addToPattern(pattern, nextColumn, startTime, EndTime);
            }

            return pattern;
        }

        /// <summary>
        /// Generates random notes, with one note per row and no stacking.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <param name="noteCount">The number of notes.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateRandomNotes(int startTime, int noteCount)
        {
            // - - - -
            // x - - -
            // - - x -
            // - - - x
            // x - - -

            var pattern = new Pattern();

            int nextColumn = GetColumn((HitObject as IHasXPosition)?.X ?? 0, true);
            if (convertType.HasFlagFast(PatternType.ForceNotStack) && PreviousPattern.ColumnWithObjects < TotalColumns)
                nextColumn = FindAvailableColumn(nextColumn, PreviousPattern);

            int lastColumn = nextColumn;

            for (int i = 0; i < noteCount; i++)
            {
                addToPattern(pattern, nextColumn, startTime, startTime);
                nextColumn = FindAvailableColumn(nextColumn, validation: c => c != lastColumn);
                lastColumn = nextColumn;
                startTime += SegmentDuration;
            }

            return pattern;
        }

        /// <summary>
        /// Generates a stair of notes, with one note per row.
        /// </summary>
        /// <param name="startTime">The start time.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateStair(int startTime)
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

            for (int i = 0; i <= SpanCount; i++)
            {
                addToPattern(pattern, column, startTime, startTime);
                startTime += SegmentDuration;

                // Check if we're at the borders of the stage, and invert the pattern if so
                if (increasing)
                {
                    if (column >= TotalColumns - 1)
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
        private Pattern generateRandomMultipleNotes(int startTime)
        {
            // - - - -
            // x - - -
            // - x x -
            // - - - x
            // x - x -

            var pattern = new Pattern();

            bool legacy = TotalColumns >= 4 && TotalColumns <= 8;
            int interval = Random.Next(1, TotalColumns - (legacy ? 1 : 0));

            int nextColumn = GetColumn((HitObject as IHasXPosition)?.X ?? 0, true);

            for (int i = 0; i <= SpanCount; i++)
            {
                addToPattern(pattern, nextColumn, startTime, startTime);

                nextColumn += interval;
                if (nextColumn >= TotalColumns - RandomStart)
                    nextColumn = nextColumn - TotalColumns - RandomStart + (legacy ? 1 : 0);
                nextColumn += RandomStart;

                // If we're in 2K, let's not add many consecutive doubles
                if (TotalColumns > 2)
                    addToPattern(pattern, nextColumn, startTime, startTime);

                nextColumn = GetRandomColumn();
                startTime += SegmentDuration;
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
        private Pattern generateNRandomNotes(int startTime, double p2, double p3, double p4)
        {
            // - - - -
            // ■ - ■ ■
            // □ - □ □
            // ■ - ■ ■

            switch (TotalColumns)
            {
                case 2:
                    p2 = 0;
                    p3 = 0;
                    p4 = 0;
                    break;

                case 3:
                    p2 = Math.Min(p2, 0.1);
                    p3 = 0;
                    p4 = 0;
                    break;

                case 4:
                    p2 = Math.Min(p2, 0.3);
                    p3 = Math.Min(p3, 0.04);
                    p4 = 0;
                    break;

                case 5:
                    p2 = Math.Min(p2, 0.34);
                    p3 = Math.Min(p3, 0.1);
                    p4 = Math.Min(p4, 0.03);
                    break;
            }

            static bool isDoubleSample(HitSampleInfo sample) => sample.Name == HitSampleInfo.HIT_CLAP || sample.Name == HitSampleInfo.HIT_FINISH;

            bool canGenerateTwoNotes = !convertType.HasFlagFast(PatternType.LowProbability);
            canGenerateTwoNotes &= HitObject.Samples.Any(isDoubleSample) || sampleInfoListAt(StartTime).Any(isDoubleSample);

            if (canGenerateTwoNotes)
                p2 = 1;

            return generateRandomHoldNotes(startTime, GetRandomNoteCount(p2, p3, p4));
        }

        /// <summary>
        /// Generates tiled hold notes. You can think of this as a stair of hold notes.
        /// </summary>
        /// <param name="startTime">The first hold note start time.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateTiledHoldNotes(int startTime)
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

            int columnRepeat = Math.Min(SpanCount, TotalColumns);

            // Due to integer rounding, this is not guaranteed to be the same as EndTime (the class-level variable).
            int endTime = startTime + SegmentDuration * SpanCount;

            int nextColumn = GetColumn((HitObject as IHasXPosition)?.X ?? 0, true);
            if (convertType.HasFlagFast(PatternType.ForceNotStack) && PreviousPattern.ColumnWithObjects < TotalColumns)
                nextColumn = FindAvailableColumn(nextColumn, PreviousPattern);

            for (int i = 0; i < columnRepeat; i++)
            {
                nextColumn = FindAvailableColumn(nextColumn, pattern);
                addToPattern(pattern, nextColumn, startTime, endTime);
                startTime += SegmentDuration;
            }

            return pattern;
        }

        /// <summary>
        /// Generates a hold note alongside normal notes.
        /// </summary>
        /// <param name="startTime">The start time of notes.</param>
        /// <returns>The <see cref="Pattern"/> containing the hit objects.</returns>
        private Pattern generateHoldAndNormalNotes(int startTime)
        {
            // - - - -
            // ■ x x -
            // ■ - x x
            // ■ x - x
            // ■ - x x

            var pattern = new Pattern();

            int holdColumn = GetColumn((HitObject as IHasXPosition)?.X ?? 0, true);
            if (convertType.HasFlagFast(PatternType.ForceNotStack) && PreviousPattern.ColumnWithObjects < TotalColumns)
                holdColumn = FindAvailableColumn(holdColumn, PreviousPattern);

            // Create the hold note
            addToPattern(pattern, holdColumn, startTime, EndTime);

            int nextColumn = GetRandomColumn();
            int noteCount;
            if (ConversionDifficulty > 6.5)
                noteCount = GetRandomNoteCount(0.63, 0);
            else if (ConversionDifficulty > 4)
                noteCount = GetRandomNoteCount(TotalColumns < 6 ? 0.12 : 0.45, 0);
            else if (ConversionDifficulty > 2.5)
                noteCount = GetRandomNoteCount(TotalColumns < 6 ? 0 : 0.24, 0);
            else
                noteCount = 0;
            noteCount = Math.Min(TotalColumns - 1, noteCount);

            bool ignoreHead = !sampleInfoListAt(startTime).Any(s => s.Name == HitSampleInfo.HIT_WHISTLE || s.Name == HitSampleInfo.HIT_FINISH || s.Name == HitSampleInfo.HIT_CLAP);

            var rowPattern = new Pattern();

            for (int i = 0; i <= SpanCount; i++)
            {
                if (!(ignoreHead && startTime == StartTime))
                {
                    for (int j = 0; j < noteCount; j++)
                    {
                        nextColumn = FindAvailableColumn(nextColumn, validation: c => c != holdColumn, patterns: rowPattern);
                        addToPattern(rowPattern, nextColumn, startTime, startTime);
                    }
                }

                pattern.Add(rowPattern);
                rowPattern.Clear();

                startTime += SegmentDuration;
            }

            return pattern;
        }

        /// <summary>
        /// Retrieves the sample info list at a point in time.
        /// </summary>
        /// <param name="time">The time to retrieve the sample info list from.</param>
        private IList<HitSampleInfo> sampleInfoListAt(int time) => nodeSamplesAt(time)?.First() ?? HitObject.Samples;

        /// <summary>
        /// Retrieves the list of node samples that occur at time greater than or equal to <paramref name="time"/>.
        /// </summary>
        /// <param name="time">The time to retrieve node samples at.</param>
        private IList<IList<HitSampleInfo>> nodeSamplesAt(int time)
        {
            if (!(HitObject is IHasPathWithRepeats curveData))
                return null;

            int index = SegmentDuration == 0 ? 0 : (time - StartTime) / SegmentDuration;

            // avoid slicing the list & creating copies, if at all possible.
            return index == 0 ? curveData.NodeSamples : curveData.NodeSamples.Skip(index).ToList();
        }

        /// <summary>
        /// Constructs and adds a note to a pattern.
        /// </summary>
        /// <param name="pattern">The pattern to add to.</param>
        /// <param name="column">The column to add the note to.</param>
        /// <param name="startTime">The start time of the note.</param>
        /// <param name="endTime">The end time of the note (set to <paramref name="startTime"/> for a non-hold note).</param>
        private void addToPattern(Pattern pattern, int column, int startTime, int endTime)
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
                newObject = new HoldNote
                {
                    StartTime = startTime,
                    Duration = endTime - startTime,
                    Column = column,
                    Samples = HitObject.Samples,
                    NodeSamples = nodeSamplesAt(startTime)
                };
            }

            pattern.Add(newObject);
        }
    }
}
