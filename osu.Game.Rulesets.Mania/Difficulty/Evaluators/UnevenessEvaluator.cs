// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Data;
using osu.Game.Rulesets.Mania.Difficulty.Utils;

namespace osu.Game.Rulesets.Mania.Difficulty.Evaluators
{
    public static class UnevennessEvaluator
    {
        /// <summary>
        /// Evaluates the unevenness difficulty at a specific time point.
        /// </summary>
        public static double EvaluateDifficultyAt(double time, SunnyStrainData data)
        {
            return data.SampleFeatureAtTime(time, data.Unevenness);
        }

        /// <summary>
        /// Computes the unevenness values across all time points.
        /// This measures how irregular the timing patterns are between adjacent columns.
        /// </summary>
        public static double[] ComputeUnevenness(SunnyStrainData data)
        {
            bool[][] keyUsagePatterns = CrossColumnEvaluator.ComputeKeyUsage(data);
            double[][] timingDeltasByColumn = computeTimingDeltasByColumn(data);

            double[] baseUnevenness = calculateBaseUnevenness(data, keyUsagePatterns, timingDeltasByColumn);

            // Interpolate to match target time resolution
            return StrainArrayUtils.InterpolateArray(
                data.CornerData.TimeCorners,
                data.CornerData.AccuracyTimeCorners,
                baseUnevenness
            );
        }

        /// <summary>
        /// Computes timing deltas (time between consecutive notes) for each column.
        /// This is used to analyze timing irregularities.
        /// </summary>
        private static double[][] computeTimingDeltasByColumn(SunnyStrainData data)
        {
            int timePointCount = data.CornerData.BaseTimeCorners.Length;
            double[][] timingDeltas = new double[data.KeyCount][];

            // Process each column independently for better cache performance
            for (int column = 0; column < data.KeyCount; column++)
            {
                double[] columnDeltas = new double[timePointCount];
                var columnNotes = data.NotesByColumn[column];

                // Initialize with default value (no pattern detected)
                const double no_pattern_value = 1e9;
                for (int i = 0; i < timePointCount; i++)
                    columnDeltas[i] = no_pattern_value;

                if (columnNotes.Length < 2)
                {
                    timingDeltas[column] = columnDeltas;
                    continue;
                }

                int searchIndex = 0;

                // Process consecutive note pairs
                for (int noteIndex = 0; noteIndex + 1 < columnNotes.Length; noteIndex++)
                {
                    var currentNote = columnNotes[noteIndex];
                    var nextNote = columnNotes[noteIndex + 1];

                    int startIndex = StrainArrayUtils.FindLeftBoundProgressive(
                        data.CornerData.BaseTimeCorners, ref searchIndex, currentNote.StartTime);
                    int endIndex = StrainArrayUtils.FindLeftBoundProgressive(
                        data.CornerData.BaseTimeCorners, ref searchIndex, nextNote.StartTime);

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
        private static double[] calculateBaseUnevenness(SunnyStrainData data, bool[][] keyUsagePatterns, double[][] timingDeltas)
        {
            int keyCount = data.KeyCount;
            int accuracyTimePoints = data.CornerData.AccuracyTimeCorners.Length;
            if (accuracyTimePoints == 0) return Array.Empty<double>();

            double[] unevennessBase = new double[accuracyTimePoints];

            // Initialize to neutral
            for (int i = 0; i < accuracyTimePoints; i++)
                unevennessBase[i] = 1.0;

            if (keyCount < 2) return unevennessBase; // Need at least 2 columns for unevenness

            int baseTimePoints = data.CornerData.BaseTimeCorners.Length;

            // Pre-calculate adjacent column differences once
            double[][] adjacentDifferences = new double[keyCount - 1][];

            for (int adjacentPair = 0; adjacentPair < keyCount - 1; adjacentPair++)
            {
                double[] pairDifferences = new double[baseTimePoints];
                double[] leftDeltas = timingDeltas[adjacentPair];
                double[] rightDeltas = timingDeltas[adjacentPair + 1];

                for (int timeIndex = 0; timeIndex < baseTimePoints; timeIndex++)
                {
                    double leftDelta = leftDeltas[timeIndex];
                    double rightDelta = rightDeltas[timeIndex];
                    double absoluteDifference = Math.Abs(leftDelta - rightDelta);
                    double slowPatternPenalty = 0.4 * Math.Max(0.0, Math.Max(leftDelta, rightDelta) - 0.11);
                    pairDifferences[timeIndex] = absoluteDifference + slowPatternPenalty;
                }

                adjacentDifferences[adjacentPair] = pairDifferences;
            }

            // Process each accuracy time point
            for (int accuracyIndex = 0; accuracyIndex < accuracyTimePoints; accuracyIndex++)
            {
                double accuracyTime = data.CornerData.AccuracyTimeCorners[accuracyIndex];
                int baseTimeIndex = StrainArrayUtils.FindLeftBound(data.CornerData.BaseTimeCorners, accuracyTime);

                // Clamp to a valid range
                if (baseTimeIndex >= baseTimePoints) baseTimeIndex = baseTimePoints - 1;
                if (baseTimeIndex < 0) continue;

                int previousActiveColumn = -1;

                // Find active adjacent column pairs and apply penalties
                for (int column = 0; column < keyCount; column++)
                {
                    if (!keyUsagePatterns[column][baseTimeIndex]) continue;

                    if (previousActiveColumn >= 0 && previousActiveColumn < keyCount - 1)
                    {
                        double timingDifference = adjacentDifferences[previousActiveColumn][baseTimeIndex];
                        double penalty = calculateUnevennessPenalty(timingDifference,
                            timingDeltas[previousActiveColumn][baseTimeIndex],
                            timingDeltas[column][baseTimeIndex]);

                        unevennessBase[accuracyIndex] *= penalty;
                    }

                    previousActiveColumn = column;
                }
            }

            // Apply smoothing in a single pass
            return StrainArrayUtils.ApplySmoothingToArray(
                data.CornerData.AccuracyTimeCorners,
                unevennessBase,
                data.Config.accuracySmoothingWindowMs,
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
