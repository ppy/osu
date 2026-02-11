// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Beatmaps;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Data;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.DifficultyPreprocessing;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.UtilityPreprocessing;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing
{
    /// <summary>
    /// Orchestrates the entire preprocessing pipeline for Mania difficulty calculation.
    /// Converts raw hit objects into structured difficulty context data through multiple processing stages.
    /// </summary>
    public class ManiaDifficultyPreprocessor
    {
        private const double hit_leniency_base = 0.3;
        private const double hit_leniency_multiplier = 3.0;
        private const double hit_leniency_od_base = 64.5;

        /// <summary>
        /// Entry point for the preprocessing pipeline.
        /// Processes hit objects, computes all difficulty metrics, and assigns the final context to each hit object.
        /// </summary>
        public static void ProcessAndAssign(List<DifficultyHitObject> hitObjects, IBeatmap beatmap)
        {
            var data = new ManiaDifficultyData
            {
                KeyCount = ((ManiaBeatmap)beatmap).TotalColumns,
                HitLeniency = calculateHitLeniency(beatmap)
            };

            createDifficultyHitObjectLists(data, hitObjects);
            data.MaxTime = (int)(data.AllNotes.LastOrDefault()?.EndTime + 1 ?? 0);

            data.StrainTimePoints = StrainPointPreprocessor.CreateStrainPoints(data);
            setStrainPointIndices(data.StrainTimePoints, hitObjects);
            LongNoteDensityPreprocessor.ProcessAndAssign(data);

            data.SharedKeyUsage = CrossColumnPreprocessor.ComputeKeyUsage(data);

            data.SameColumnPressure = SameColumnPreprocessor.ComputeValues(data);
            data.CrossColumnPressure = CrossColumnPreprocessor.ComputeValues(data);
            data.PressingIntensity = PressingIntensityPreprocessor.ComputeValues(data);
            data.Unevenness = UnevennessPreprocessor.ComputeValues(data);
            data.ReleaseFactor = ReleasePreprocessor.ComputeValues(data);

            computeSupplementaryMetrics(data);

            hitObjects.ForEach(hitObject => ((ManiaDifficultyHitObject)hitObject).DifficultyData = data);
        }

        /// <summary>
        /// Categorizes hit objects into specialized <see cref="ManiaDifficultyHitObject"/> lists and performs initial setup.
        /// </summary>
        private static void createDifficultyHitObjectLists(ManiaDifficultyData data, List<DifficultyHitObject> hitObjects)
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

            // Special handling for long note tails (sorted by end time)
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

        private static void setStrainPointIndices(double[] strainTimePoints, List<DifficultyHitObject> hitObjects)
        {
            int strainPointIndex = 0;

            foreach (var obj in hitObjects)
            {
                while (strainTimePoints[strainPointIndex] < obj.StartTime)
                {
                    strainPointIndex++;
                }

                ((ManiaDifficultyHitObject)obj).StrainTimePointIndex = strainPointIndex;
            }
        }

        /// <summary>
        /// Calculates hit window leniency based on the beatmap's Overall Difficulty.
        /// </summary>
        /// <returns>Hit leniency value in milliseconds (clamped to reasonable bounds)</returns>
        private static double calculateHitLeniency(IBeatmap beatmap)
        {
            double overallDifficulty = beatmap.BeatmapInfo.Difficulty.OverallDifficulty;
            double odAdjustedValue = hit_leniency_od_base - Math.Ceiling(hit_leniency_multiplier * overallDifficulty);
            double rawLeniency = hit_leniency_base * Math.Sqrt(odAdjustedValue / 500.0);

            rawLeniency = Math.Min(rawLeniency, 0.6 * (rawLeniency - 0.09) + 0.09);

            return Math.Max(1e-9, rawLeniency);
        }

        /// <summary>
        /// Computes supplementary metrics that support the main difficulty calculations:
        /// - Local note density (notes per second)
        /// - Active key count (simultaneous key presses)
        /// These are computed at base time corners and then interpolated to final time corners.
        /// </summary>
        private static void computeSupplementaryMetrics(ManiaDifficultyData data)
        {
            // Prepare sorted note times for density calculations
            double[] noteHitTimes = data.AllNotes.Select(note => note.StartTime).ToArray();
            Array.Sort(noteHitTimes);

            // Get key usage patterns from cross-column preprocessor
            bool[][] keyUsagePatterns = data.SharedKeyUsage ?? CrossColumnPreprocessor.ComputeKeyUsage(data);
            int timePointCount = data.StrainTimePoints.Length;

            double[] localNoteCountBase = new double[timePointCount];
            double[] activeKeyCountBase = new double[timePointCount];

            for (int timeIndex = 0; timeIndex < timePointCount; timeIndex++)
            {
                double currentTime = data.StrainTimePoints[timeIndex];

                // Count notes in 1-second window (500ms before/after)
                localNoteCountBase[timeIndex] = countNotesInWindow(noteHitTimes, currentTime, 500.0);
                // Count simultaneously active keys
                activeKeyCountBase[timeIndex] = countActiveKeys(keyUsagePatterns, timeIndex, data.KeyCount);
            }

            // Interpolate to final time corners using step function (preserves discrete values)
            data.LocalNoteCount = interpolateWithStepFunction(data.StrainTimePoints,
                data.StrainTimePoints, localNoteCountBase);
            data.ActiveKeyCount = interpolateWithStepFunction(data.StrainTimePoints,
                data.StrainTimePoints, activeKeyCountBase);
        }

        /// <summary>
        /// Counts notes within a symmetric time window around a center point.
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
        /// Counts the number of keys currently pressed at a specific time index.
        /// Uses precomputed key usage patterns from <see cref="CrossColumnPreprocessor"/>.
        /// </summary>
        /// <param name="keyUsage">2D array of key usage [column][time index]</param>
        /// <param name="timeIndex">Index in the time corner array</param>
        /// <param name="keyCount">Total number of keys/columns</param>
        /// <returns>Number of simultaneously active keys</returns>
        private static double countActiveKeys(bool[][] keyUsage, int timeIndex, int keyCount)
        {
            int activeCount = 0;

            // Count active keys across all columns
            for (int column = 0; column < keyCount; column++)
            {
                if (keyUsage[column][timeIndex]) activeCount++;
            }

            return Math.Max(activeCount, 1);
        }

        /// <summary>
        /// Performs step interpolation (zero-order hold) between time points.
        /// Preserves discrete values rather than creating smooth transitions.
        /// Appropriate for count-based metrics like note density and active keys.
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

            // Step interpolation: maintain value until next breakpoint
            for (int targetIndex = 0; targetIndex < newPointCount; targetIndex++)
            {
                double targetTime = newTimePoints[targetIndex];

                // Advance source index while next breakpoint is before target time
                while (sourceIndex + 1 < originalPointCount && originalTimePoints[sourceIndex + 1] <= targetTime)
                    sourceIndex++;

                // Use current source value (clamped to array bounds)
                result[targetIndex] = originalValues[Math.Min(sourceIndex, originalValues.Length - 1)];
            }

            return result;
        }
    }
}
