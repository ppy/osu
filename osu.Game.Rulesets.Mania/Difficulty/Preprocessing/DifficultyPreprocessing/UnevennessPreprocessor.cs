// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Data;
using osu.Game.Rulesets.Mania.Difficulty.Utils;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.DifficultyPreprocessing
{
    public static class UnevennessPreprocessor
    {
        private const int accuracy_smoothing_window_ms = 250;

        /// <summary>
        /// Computes the unevenness values across all-time points.
        /// This measures how irregular the timing patterns are between adjacent columns.
        /// </summary>
        public static double[] ComputeValues(ManiaDifficultyData data)
        {
            bool[][] keyUsagePatterns = data.SharedKeyUsage ?? CrossColumnPreprocessor.ComputeKeyUsage(data);
            double[][] timingDeltasByColumn = computeTimingDeltasByColumn(data);

            double[] baseUnevenness = calculateBaseUnevenness(data, keyUsagePatterns, timingDeltasByColumn);

            // Interpolate to match target time resolution
            return baseUnevenness;
        }

        /// <summary>
        /// Computes timing deltas (time between consecutive notes) for each column.
        /// This is used to analyze timing irregularities.
        /// </summary>
        private static double[][] computeTimingDeltasByColumn(ManiaDifficultyData data)
        {
            int timePointCount = data.StrainTimePoints.Length;
            double[][] timingDeltas = new double[data.KeyCount][];

            for (int column = 0; column < data.KeyCount; column++)
            {
                double[] columnDeltas = new double[timePointCount];
                List<ManiaDifficultyHitObject> columnNotes = data.AllNotes.First().PerColumnObjects.Select(list => list.Cast<ManiaDifficultyHitObject>().ToList()).ToArray()[column];

                // Initialize with default value (no pattern detected)
                const double no_pattern_value = 1e9;
                for (int i = 0; i < timePointCount; i++)
                    columnDeltas[i] = no_pattern_value;

                if (columnNotes.Count < 2)
                {
                    timingDeltas[column] = columnDeltas;
                    continue;
                }

                int searchIndex = 0;

                // Process consecutive note pairs
                for (int noteIndex = 0; noteIndex < columnNotes.Count; noteIndex++)
                {
                    var currentNote = columnNotes[noteIndex];
                    var nextNote = currentNote.NextInColumn();

                    if (nextNote == null) break;

                    int startIndex = StrainArrayUtils.FindLeftBoundProgressive(
                        data.StrainTimePoints, ref searchIndex, currentNote.StartTime);
                    int endIndex = StrainArrayUtils.FindLeftBoundProgressive(
                        data.StrainTimePoints, ref searchIndex, nextNote.StartTime);

                    if (endIndex <= startIndex) continue;

                    double deltaTimeSeconds = 0.001 * (nextNote.StartTime - currentNote.StartTime);

                    int rangeEnd = Math.Min(endIndex, timePointCount);
                    for (int timeIndex = startIndex; timeIndex < rangeEnd; timeIndex++)
                        columnDeltas[timeIndex] = deltaTimeSeconds;
                }

                timingDeltas[column] = columnDeltas;
            }

            return timingDeltas;
        }

        /// <summary>
        /// Calculates the base unevenness without smoothing.
        /// This analyzes timing differences between adjacent active columns.
        /// </summary>
        private static double[] calculateBaseUnevenness(ManiaDifficultyData data, bool[][] keyUsagePatterns, double[][] timingDeltas)
        {
            int keyCount = data.KeyCount;
            int accuracyTimePoints = data.StrainTimePoints.Length;

            if (accuracyTimePoints == 0) return Array.Empty<double>();

            double[] unevennessBase = new double[accuracyTimePoints];
            for (int i = 0; i < accuracyTimePoints; i++)
                unevennessBase[i] = 1.0;

            if (keyCount < 2) return unevennessBase;

            int baseTimePoints = data.StrainTimePoints.Length;

            // Process each accuracy time point directly without pre-computing all differences
            for (int accuracyIndex = 0; accuracyIndex < accuracyTimePoints; accuracyIndex++)
            {
                double accuracyTime = data.StrainTimePoints[accuracyIndex];
                int baseTimeIndex = StrainArrayUtils.FindLeftBound(data.StrainTimePoints, accuracyTime);

                if (baseTimeIndex >= baseTimePoints) baseTimeIndex = baseTimePoints - 1;
                if (baseTimeIndex < 0) continue;

                int previousActiveColumn = -1;

                for (int column = 0; column < keyCount; column++)
                {
                    if (!keyUsagePatterns[column][baseTimeIndex]) continue;

                    if (previousActiveColumn >= 0)
                    {
                        double leftDelta = timingDeltas[previousActiveColumn][baseTimeIndex];
                        double rightDelta = timingDeltas[column][baseTimeIndex];

                        double absoluteDifference = Math.Abs(leftDelta - rightDelta);
                        double slowPatternPenalty = 0.4 * Math.Max(0.0, Math.Max(leftDelta, rightDelta) - 0.11);
                        double timingDifference = absoluteDifference + slowPatternPenalty;

                        double penalty = calculateUnevennessPenalty(timingDifference, leftDelta, rightDelta);
                        unevennessBase[accuracyIndex] *= penalty;
                    }

                    previousActiveColumn = column;
                }
            }

            return StrainArrayUtils.ApplySmoothingToArray(
                data.StrainTimePoints,
                unevennessBase,
                accuracy_smoothing_window_ms,
                1.0,
                "avg"
            );
        }

        /// <summary>
        /// Calculates the unevenness penalty for a pair of adjacent active columns.
        /// Higher penalties are applied when timing patterns are more irregular.
        /// </summary>
        private static double calculateUnevennessPenalty(double timingDifference, double leftDelta, double rightDelta)
        {
            if (timingDifference < 0.02)
            {
                // Very small differences => mild penalty with timing component
                return Math.Min(0.75 + 0.5 * Math.Max(leftDelta, rightDelta), 1.0);
            }

            if (timingDifference < 0.07)
            {
                // Moderate differences => variable penalty
                return Math.Min(0.65 + 5.0 * timingDifference + 0.5 * Math.Max(leftDelta, rightDelta), 1.0);
            }

            return 1.0; // No penalty for large differences
        }
    }
}
