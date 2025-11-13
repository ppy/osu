// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Components;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Corner;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing
{
    /// <summary>
    /// Main preprocessor that converts raw difficulty hit objects into structured data for difficulty calculation.
    /// This class orchestrates the entire preprocessing pipeline.
    /// </summary>
    public class ManiaDifficultyPreprocessor
    {
        /// <summary>
        /// Main processing method that converts input data into a complete <see cref="ManiaDifficultyContext"/> structure.
        /// </summary>
        /// <returns>Fully processed strain data ready for difficulty calculation</returns>
        public static void ProcessAndAssign(List<DifficultyHitObject> hitObjects, IBeatmap beatmap)
        {
            var data = new ManiaDifficultyContext
            {
                KeyCount = ((ManiaBeatmap)beatmap).TotalColumns,
                HitLeniency = calculateHitLeniency(beatmap)
            };

            processDifficultyHitObjects(data, hitObjects, beatmap);
            calculateMaxTime(data);

            var cornerData = CornerDataPreprocessor.Process(hitObjects, data.MaxTime);
            data.CornerData = cornerData;

            computeAllComponents(data);

            hitObjects.ForEach(hitObject =>
                ((ManiaDifficultyHitObject)hitObject).PreprocessedDifficultyData = data);
        }

        private static void processDifficultyHitObjects(ManiaDifficultyContext data, List<DifficultyHitObject> hitObjects, IBeatmap beatmap)
        {
            if (hitObjects.Count == 0)
            {
                data.AllNotes = new List<ManiaDifficultyHitObject>();
                data.LongNotes = new List<ManiaDifficultyHitObject>();
                data.LongNoteTails = new List<ManiaDifficultyHitObject>();
                return;
            }

            var maniaList = new List<ManiaDifficultyHitObject>(hitObjects.Count);
            var longNotes = new List<ManiaDifficultyHitObject>();

            for (int i = 0; i < hitObjects.Count; i++)
            {
                var mdho = (ManiaDifficultyHitObject)hitObjects[i];
                maniaList.Add(mdho);

                if (mdho.IsLong)
                    longNotes.Add(mdho);
            }

            data.AllNotes = maniaList;
            data.LongNotes = longNotes;

            if (longNotes.Count > 0)
            {
                var longNoteTails = new List<ManiaDifficultyHitObject>(longNotes);
                longNoteTails.Sort((a, b) => a.EndTime.CompareTo(b.EndTime));
                data.LongNoteTails = longNoteTails;
            }
            else
            {
                data.LongNoteTails = new List<ManiaDifficultyHitObject>();
            }
        }

        private static void calculateMaxTime(ManiaDifficultyContext data)
        {
            var notes = data.AllNotes;

            if (notes.Count == 0)
            {
                data.MaxTime = 0;
                return;
            }

            var last = notes[^1];

            double maxTime = Math.Max(last.StartTime, last.IsLong ? last.EndTime : last.StartTime);
            data.MaxTime = (int)Math.Ceiling(maxTime) + 1;
        }

        /// <summary>
        /// Calculates hit leniency based on the beatmap's OD.
        /// Tighter timing windows make patterns more difficult to execute accurately.
        /// </summary>
        private static double calculateHitLeniency(IBeatmap beatmap)
        {
            FormulaConfig config = new FormulaConfig();
            double overallDifficulty = beatmap.BeatmapInfo.Difficulty.OverallDifficulty;

            // Calculate raw leniency using OD-based formula
            double odAdjustedValue = config.HitLeniencyOdBase - Math.Ceiling(config.HitLeniencyOdMultiplier * overallDifficulty);
            double rawLeniency = config.HitLeniencyBase * Math.Sqrt(odAdjustedValue / 500.0);

            // Apply cap to prevent extreme values
            rawLeniency = Math.Min(rawLeniency, 0.6 * (rawLeniency - 0.09) + 0.09);

            return Math.Max(1e-9, rawLeniency);
        }

        private static void computeAllComponents(ManiaDifficultyContext data)
        {
            data.SameColumnPressure = SameColumnPreprocessor.ComputeValues(data);
            data.CrossColumnPressure = CrossColumnPreprocessor.ComputeValues(data);
            data.PressingIntensity = PressingIntensityPreprocessor.ComputeValues(data);
            data.Unevenness = UnevennessPreprocessor.ComputeValues(data);
            data.ReleaseFactor = ReleasePreprocessor.ComputeValues(data);

            computeSupplementaryMetrics(data);
        }

        /// <summary>
        /// Computes supplementary metrics like note density and active key counts.
        /// These support the main difficulty calculations.
        /// </summary>
        private static void computeSupplementaryMetrics(ManiaDifficultyContext data)
        {
            // Get all note hit times for density calculations
            double[] noteHitTimes = data.AllNotes.Select(note => note.StartTime).ToArray();
            Array.Sort(noteHitTimes);

            // Calculate key usage patterns
            bool[][] keyUsagePatterns = CrossColumnPreprocessor.ComputeKeyUsage(data);
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
        private static double countNotesInWindow(double[] sortedNoteTimes, double centerTime, double windowRadius)
        {
            double windowStart = centerTime - windowRadius;
            double windowEnd = centerTime + windowRadius;

            int startIndex = Array.BinarySearch(sortedNoteTimes, windowStart);
            if (startIndex < 0) startIndex = ~startIndex;

            int endIndex = Array.BinarySearch(sortedNoteTimes, windowEnd);
            if (endIndex < 0) endIndex = ~endIndex;

            return endIndex - startIndex;
        }

        /// <summary>
        /// Counts the number of active keys at a specific time index.
        /// </summary>
        private static double countActiveKeys(bool[][] keyUsage, int timeIndex, int keyCount)
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
        private static double[] interpolateWithStepFunction(double[] newTimePoints, double[] originalTimePoints, double[] originalValues)
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
    }
}
