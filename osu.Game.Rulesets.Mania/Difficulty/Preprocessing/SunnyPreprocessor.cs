// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Difficulty.Evaluators;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Corner;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Strain;
using osu.Game.Rulesets.Mania.Difficulty.Skills;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing
{
    /// <summary>
    /// Main preprocessor that converts raw difficulty hit objects into structured data for difficulty calculation.
    /// This class orchestrates the entire preprocessing pipeline.
    /// </summary>
    public class SunnyPreprocessor
    {
        private readonly IEnumerable<DifficultyHitObject> difficultyHitObjects;
        private readonly ManiaBeatmap beatmap;
        private readonly FormulaConfig config;

        public SunnyPreprocessor(IEnumerable<DifficultyHitObject> difficultyHitObjects, ManiaBeatmap beatmap, FormulaConfig config)
        {
            this.difficultyHitObjects = difficultyHitObjects;
            this.beatmap = beatmap;
            this.config = config;
        }

        /// <summary>
        /// Main processing method that converts input data into a complete SunnyStrainData structure.
        /// </summary>
        /// <returns>Fully processed strain data ready for difficulty calculation</returns>
        public SunnyStrainData Process()
        {
            var data = new SunnyStrainData
            {
                Config = config,
                KeyCount = beatmap.TotalColumns,
                HitLeniency = calculateHitLeniency(beatmap)
            };

            processDifficultyHitObjects(data);
            organizeNotesByColumn(data);
            calculateMaxTime(data);

            data.CornerData = new CornerData();
            data.CornerData.ComputeTimeCorners(data.AllNotes, data.MaxTime);

            computeAllFeatures(data);

            return data;
        }

        /// <summary>
        /// Calculates hit leniency based on the beatmap's OD.
        /// Tighter timing windows make patterns more difficult to execute accurately.
        /// </summary>
        private double calculateHitLeniency(IBeatmap beatmap)
        {
            double overallDifficulty = beatmap.BeatmapInfo.Difficulty.OverallDifficulty;

            // Calculate raw leniency using OD-based formula
            double odAdjustedValue = config.hitLeniencyOdBase - Math.Ceiling(config.hitLeniencyOdMultiplier * overallDifficulty);
            double rawLeniency = config.hitLeniencyBase * Math.Sqrt(odAdjustedValue / 500.0);

            // Apply cap to prevent extreme values
            rawLeniency = Math.Min(rawLeniency, 0.6 * (rawLeniency - 0.09) + 0.09);

            return Math.Max(1e-9, rawLeniency);
        }

        /// <summary>
        /// Converts DifficultyHitObject instances to internal Note structures.
        /// This step extracts timing and column information while preserving original map timing.
        /// </summary>
        private void processDifficultyHitObjects(SunnyStrainData data)
        {
            int objectCount = difficultyHitObjects.Count();

            if (objectCount == 0)
            {
                data.AllNotes = Array.Empty<Note>();
                return;
            }

            // Convert each hit object to our internal Note format
            // However this will probably change
            var processedNotes = new Note[objectCount];
            int noteIndex = 0;

            foreach (var hitObject in difficultyHitObjects)
            {
                var maniaHitObject = (ManiaDifficultyHitObject)hitObject;

                int column = maniaHitObject.Column;
                int startTime = (int)maniaHitObject.StartTime;
                int endTime = (int)maniaHitObject.EndTime;

                if (endTime <= startTime) endTime = -1;

                processedNotes[noteIndex++] = new Note(column, startTime, endTime);
            }

            if (noteIndex != processedNotes.Length)
                Array.Resize(ref processedNotes, noteIndex);

            Array.Sort(processedNotes, 0, noteIndex);
            data.AllNotes = processedNotes;
        }

        /// <summary>
        /// Organizes notes by column and separates long notes for specialized processing.
        /// This enables efficient column-specific algorithms.
        /// </summary>
        private void organizeNotesByColumn(SunnyStrainData data)
        {
            int keyCount = Math.Max(1, data.KeyCount);

            int[] columnCounts = new int[keyCount];

            foreach (var note in data.AllNotes)
            {
                if (note.Column >= 0 && note.Column < keyCount)
                    columnCounts[note.Column]++;
            }

            data.NotesByColumn = new Note[keyCount][];

            for (int column = 0; column < keyCount; column++)
            {
                data.NotesByColumn[column] = columnCounts[column] == 0 ? Array.Empty<Note>() : new Note[columnCounts[column]];
            }

            int[] columnPositions = new int[keyCount];

            foreach (var note in data.AllNotes)
            {
                if (note.Column >= 0 && note.Column < keyCount)
                {
                    data.NotesByColumn[note.Column][columnPositions[note.Column]++] = note;
                }
            }

            extractLongNotes(data);
        }

        /// <summary>
        /// Extracts long notes and creates specialized arrays for release timing calculations.
        /// </summary>
        private void extractLongNotes(SunnyStrainData data)
        {
            // Count long notes
            int longNoteCount = 0;

            foreach (var note in data.AllNotes)
            {
                if (note.IsLong) longNoteCount++;
            }

            if (longNoteCount == 0)
            {
                data.LongNotes = Array.Empty<Note>();
                data.LongNoteTails = Array.Empty<Note>();
                return;
            }

            data.LongNotes = new Note[longNoteCount];
            int longNoteIndex = 0;

            foreach (var note in data.AllNotes)
            {
                if (note.IsLong) data.LongNotes[longNoteIndex++] = note;
            }

            data.LongNoteTails = (Note[])data.LongNotes.Clone();
            Array.Sort(data.LongNoteTails, (a, b) => a.EndTime.CompareTo(b.EndTime));
        }

        /// <summary>
        /// Calculates the maximum time value in the beatmap for boundary calculations.
        /// </summary>
        private void calculateMaxTime(SunnyStrainData data)
        {
            int maxTime = 0;

            foreach (var note in data.AllNotes)
            {
                maxTime = Math.Max(maxTime, note.StartTime);
                if (note.IsLong) maxTime = Math.Max(maxTime, note.EndTime);
            }

            data.MaxTime = maxTime + 1;
        }

        /// <summary>
        /// Computes all difficulty features using the specialized evaluators.
        /// This is where the main difficulty calculation work happens.
        /// </summary>
        private void computeAllFeatures(SunnyStrainData data)
        {
            data.SameColumnPressure = SameColumnEvaluator.ComputeSameColumnPressure(data);
            data.CrossColumnPressure = CrossColumnEvaluator.ComputeCrossColumnPressure(data);
            data.PressingIntensity = PressingIntensityEvaluator.ComputePressingIntensity(data);
            data.Unevenness = UnevennessEvaluator.ComputeUnevenness(data);
            data.ReleaseFactor = ReleaseFactorEvaluator.ComputeReleaseFactor(data);

            computeSupplementaryMetrics(data);
        }

        /// <summary>
        /// Computes supplementary metrics like note density and active key counts.
        /// These support the main difficulty calculations.
        /// </summary>
        private void computeSupplementaryMetrics(SunnyStrainData data)
        {
            // Get all note hit times for density calculations
            int[] noteHitTimes = data.AllNotes.Select(note => note.StartTime).ToArray();
            Array.Sort(noteHitTimes);

            // Calculate key usage patterns
            bool[][] keyUsagePatterns = CrossColumnEvaluator.ComputeKeyUsage(data);
            int timePointCount = data.CornerData.BaseTimeCorners.Length;

            double[] localNoteCountBase = new double[timePointCount];
            double[] activeKeyCountBase = new double[timePointCount];

            for (int timeIndex = 0; timeIndex < timePointCount; timeIndex++)
            {
                double currentTime = data.CornerData.BaseTimeCorners[timeIndex];

                // Count notes in 1-second window around this time
                localNoteCountBase[timeIndex] = countNotesInWindow(noteHitTimes, currentTime, 500.0);
                activeKeyCountBase[timeIndex] = countActiveKeys(keyUsagePatterns, timeIndex, data.KeyCount);
            }

            // Interpolate to final resolution using step interpolation (maintains discrete values)
            data.LocalNoteCount = interpolateWithStepFunction(data.CornerData.TimeCorners,
                data.CornerData.BaseTimeCorners, localNoteCountBase);
            data.ActiveKeyCount = interpolateWithStepFunction(data.CornerData.TimeCorners,
                data.CornerData.BaseTimeCorners, activeKeyCountBase);
        }

        /// <summary>
        /// Counts notes within a time window around a center point.
        /// </summary>
        private double countNotesInWindow(int[] sortedNoteTimes, double centerTime, double windowRadius)
        {
            double windowStart = centerTime - windowRadius;
            double windowEnd = centerTime + windowRadius;

            int startIndex = Array.BinarySearch(sortedNoteTimes, (int)windowStart);
            if (startIndex < 0) startIndex = ~startIndex;

            int endIndex = Array.BinarySearch(sortedNoteTimes, (int)windowEnd);
            if (endIndex < 0) endIndex = ~endIndex;

            return endIndex - startIndex;
        }

        /// <summary>
        /// Counts the number of active keys at a specific time index.
        /// </summary>
        private double countActiveKeys(bool[][] keyUsage, int timeIndex, int keyCount)
        {
            int activeCount = 0;

            for (int column = 0; column < keyCount; column++)
            {
                if (keyUsage[column][timeIndex]) activeCount++;
            }

            return Math.Max(activeCount, 1);
        }

        /// <summary>
        /// Performs step interpolation (maintains discrete values rather than smooth interpolation).
        /// This is appropriate for count-based metrics that should have discrete values.
        /// </summary>
        private double[] interpolateWithStepFunction(double[] newTimePoints, double[] originalTimePoints, double[] originalValues)
        {
            if (newTimePoints.Length == 0)
                return Array.Empty<double>();

            if (originalTimePoints.Length == 0 || originalValues.Length == 0)
            {
                return new double[newTimePoints.Length];
            }

            int newPointCount = newTimePoints.Length;
            int originalPointCount = originalTimePoints.Length;
            double[] result = new double[newPointCount];

            int sourceIndex = 0;

            for (int targetIndex = 0; targetIndex < newPointCount; targetIndex++)
            {
                double targetTime = newTimePoints[targetIndex];

                // Find the appropriate source value (step function - use value until next breakpoint)
                while (sourceIndex + 1 < originalPointCount && originalTimePoints[sourceIndex + 1] <= targetTime)
                    sourceIndex++;

                result[targetIndex] = originalValues[Math.Min(sourceIndex, originalValues.Length - 1)];
            }

            return result;
        }

        /// <summary>
        /// Will remove later
        /// </summary>
        public readonly struct Note : IComparable<Note>
        {
            public readonly int Column;
            public readonly int StartTime;
            public readonly int EndTime;

            public Note(int column, int startTime, int endTime)
            {
                Column = column;
                StartTime = startTime;
                EndTime = endTime;
            }

            public bool IsLong => EndTime > StartTime;

            public int CompareTo(Note other)
            {
                int timeComparison = StartTime.CompareTo(other.StartTime);
                return timeComparison != 0 ? timeComparison : Column.CompareTo(other.Column);
            }
        }
    }
}
