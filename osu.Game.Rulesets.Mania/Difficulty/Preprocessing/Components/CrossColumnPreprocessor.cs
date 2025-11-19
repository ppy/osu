// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mania.Difficulty.Utils;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Components
{
    public class CrossColumnPreprocessor
    {
        private const int column_activity_window_ms = 150;
        private const int smoothing_window_ms = 500;

        /// <summary>
        /// Computes the cross-column pressure values across all time points.
        /// This involves calculating base pressure values and then applying smoothing.
        /// </summary>
        public static double[] ComputeValues(ManiaDifficultyContext data)
        {
            double[] baseCrossColumnPressure = calculateBaseCrossColumnPressure(data);

            // Apply smoothing to reduce noise and make the difficulty curve more stable
            double[] smoothedPressure = StrainArrayUtils.ApplySmoothingToArray(
                data.CornerData.BaseTimeCorners,
                baseCrossColumnPressure,
                smoothing_window_ms,
                0.001,
                "sum"
            );

            // Interpolate to match the target time resolution
            return StrainArrayUtils.InterpolateArray(
                data.CornerData.TimeCorners,
                data.CornerData.BaseTimeCorners,
                smoothedPressure
            );
        }

        /// <summary>
        /// Calculates the base cross-column pressure without smoothing.
        /// This is the core algorithm that determines how difficult cross-column patterns are.
        /// </summary>
        private static double[] calculateBaseCrossColumnPressure(ManiaDifficultyContext data)
        {
            int timePointCount = data.CornerData.BaseTimeCorners.Length;
            if (timePointCount == 0) return Array.Empty<double>();

            double[] crossColumnPressure = new double[timePointCount];
            double[] crossHandCoefficients = getCrossHandCoefficients(data.KeyCount);
            bool[][] keyUsageByTime = data.SharedKeyUsage ?? ComputeKeyUsage(data);
            double[] baseTimeCorners = data.CornerData.BaseTimeCorners;

            // Create a lookup table for which keys are active at each time point
            byte[] activeKeyLookup = ArrayPool<byte>.Shared.Rent(data.KeyCount * Math.Max(1, timePointCount));

            try
            {
                buildActiveKeyLookup(activeKeyLookup, keyUsageByTime, data.KeyCount, timePointCount);

                var poolD = ArrayPool<double>.Shared;
                int groupCount = data.KeyCount + 1;
                double[] crossIntensityBuffer = poolD.Rent(groupCount * Math.Max(1, timePointCount));
                double[] fastPressureBuffer = poolD.Rent(groupCount * Math.Max(1, timePointCount));

                Array.Clear(crossIntensityBuffer, 0, groupCount * timePointCount);
                Array.Clear(fastPressureBuffer, 0, groupCount * timePointCount);

                processColumnBoundaries(data, crossIntensityBuffer, fastPressureBuffer,
                    activeKeyLookup, crossHandCoefficients, baseTimeCorners, timePointCount);

                combineCrossColumnValues(crossColumnPressure, crossIntensityBuffer, fastPressureBuffer,
                    crossHandCoefficients, data.KeyCount, timePointCount);

                poolD.Return(crossIntensityBuffer, clearArray: true);
                poolD.Return(fastPressureBuffer, clearArray: true);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(activeKeyLookup);
            }

            return crossColumnPressure;
        }

        /// <summary>
        /// Builds a lookup table showing which keys are active at each time point.
        /// </summary>
        /// <param name="activeByTime">Output array to populate with activity data</param>
        /// <param name="keyUsage">2D array of key usage patterns by time [column][time index]</param>
        /// <param name="keyCount">Total number of keys/columns</param>
        /// <param name="timePointCount">Number of time points to process</param>
        private static void buildActiveKeyLookup(byte[] activeByTime, bool[][] keyUsage, int keyCount, int timePointCount)
        {
            for (int keyIndex = 0; keyIndex < keyCount; keyIndex++)
            {
                bool[] keyUsageArray = keyUsage[keyIndex];
                int offset = keyIndex * timePointCount;

                for (int timeIndex = 0; timeIndex < timePointCount; timeIndex++)
                {
                    activeByTime[offset + timeIndex] = keyUsageArray[timeIndex] ? (byte)1 : (byte)0;
                }
            }
        }

        /// <summary>
        /// Processes each column boundary to calculate cross-column difficulty.
        /// A boundary exists between each pair of adjacent columns.
        /// </summary>
        private static void processColumnBoundaries(ManiaDifficultyContext data, double[] crossIntensityBuffer, double[] fastPressureBuffer, byte[] activeByTime, double[] crossHandCoefficients, double[] baseTimeCorners, int timePointCount)
        {
            List<ManiaDifficultyHitObject>[] columns = data.AllNotes.First().PerColumnObjects.Select(list => list.Cast<ManiaDifficultyHitObject>().ToList()).ToArray();

            //var columns = data.AllNotes.First().PerColumnObjects;

            for (int boundaryIndex = 0; boundaryIndex <= data.KeyCount; boundaryIndex++)
            {
                var (leftColumnNotes, rightColumnNotes) = getBoundaryColumns(columns, boundaryIndex, data.KeyCount);

                processBoundaryNotes(leftColumnNotes, rightColumnNotes, boundaryIndex, data,
                    crossIntensityBuffer, fastPressureBuffer, activeByTime,
                    crossHandCoefficients, baseTimeCorners, timePointCount);
            }
        }

        /// <summary>
        /// Gets the <see cref="ManiaDifficultyHitObject"/> arrays for the left and right columns of a boundary.
        /// </summary>
        private static (List<ManiaDifficultyHitObject> left, List<ManiaDifficultyHitObject> right)
            getBoundaryColumns(List<ManiaDifficultyHitObject>[] columns, int boundaryIndex, int keyCount)
        {
            var left = new List<ManiaDifficultyHitObject>();
            var right = new List<ManiaDifficultyHitObject>();

            if (boundaryIndex == 0)
            {
                // Left edge - only consider leftmost column
                if (columns.Length > 0)
                    left = columns[0];
            }
            else if (boundaryIndex == keyCount)
            {
                // Right edge - only consider rightmost column
                if (columns.Length >= keyCount)
                    left = columns[keyCount - 1];
                else if (columns.Length > 0)
                    left = columns[^1];
            }
            else
            {
                // Middle boundary - consider both adjacent columns, guard indexes
                int leftIndex = Math.Max(0, boundaryIndex - 1);
                int rightIndex = Math.Min(columns.Length - 1, boundaryIndex);

                if (leftIndex < columns.Length)
                    left = columns[leftIndex];

                if (rightIndex >= 0 && rightIndex < columns.Length)
                    right = columns[rightIndex];
            }

            return (left, right);
        }

        /// <summary>
        /// Processes notes from two columns to calculate cross-column pressure at a boundary.
        /// </summary>
        private static void processBoundaryNotes(List<ManiaDifficultyHitObject> leftNotes, List<ManiaDifficultyHitObject> rightNotes,
                                                 int boundaryIndex, ManiaDifficultyContext data, double[] crossIntensityBuffer, double[] fastPressureBuffer,
                                                 byte[] activeByTime, double[] crossHandCoefficients, double[] baseTimeCorners, int timePointCount)
        {
            int leftIndex = 0, rightIndex = 0;
            int baseOffset = boundaryIndex * timePointCount;
            int searchIndex = 0;

            int leftLength = leftNotes.Count;
            int rightLength = rightNotes.Count;

            if (leftLength == 0 && rightLength == 0)
                return;

            // Initialize previousNote to the chronologically first note.
            var previousNote = getNextChronologicalNote(leftNotes, rightNotes, ref leftIndex, ref rightIndex,
                leftLength, rightLength);

            while (leftIndex < leftLength || rightIndex < rightLength)
            {
                var currentNote = getNextChronologicalNote(leftNotes, rightNotes, ref leftIndex, ref rightIndex,
                    leftLength, rightLength);

                // Calculate difficulty for the interval between previous and current note
                calculateIntervalDifficulty(previousNote, currentNote, boundaryIndex, data, crossIntensityBuffer,
                    fastPressureBuffer, activeByTime, crossHandCoefficients,
                    baseTimeCorners, timePointCount, baseOffset, ref searchIndex);

                previousNote = currentNote;
            }
        }

        /// <summary>
        /// Gets the next note in chronological order from either the left or right column.
        /// </summary>
        private static ManiaDifficultyHitObject getNextChronologicalNote(List<ManiaDifficultyHitObject> leftNotes, List<ManiaDifficultyHitObject> rightNotes, ref int leftIndex, ref int rightIndex, int leftLength, int rightLength)
        {
            if (rightLength == 0)
                return leftNotes[leftIndex++];

            if (leftLength == 0)
                return rightNotes[rightIndex++];

            // Choose the earlier note, or left if they're simultaneous
            if (leftIndex < leftLength && (rightIndex >= rightLength || leftNotes[leftIndex].StartTime <= rightNotes[rightIndex].StartTime))
                return leftNotes[leftIndex++];

            return rightNotes[rightIndex++];
        }

        /// <summary>
        /// Calculates the difficulty contribution for the time interval between two notes.
        /// </summary>
        private static void calculateIntervalDifficulty(ManiaDifficultyHitObject previousNote, ManiaDifficultyHitObject currentNote, int boundaryIndex, ManiaDifficultyContext data, double[] crossIntensityBuffer, double[] fastPressureBuffer, byte[] activeByTime, double[] crossHandCoefficients, double[] baseTimeCorners, int timePointCount, int baseOffset, ref int searchIndex)
        {
            int startIndex = StrainArrayUtils.FindLeftBoundProgressive(baseTimeCorners, ref searchIndex, previousNote.StartTime);
            int endIndex = StrainArrayUtils.FindLeftBoundProgressive(baseTimeCorners, ref searchIndex, currentNote.StartTime);

            if (endIndex <= startIndex) return;

            double deltaTimeSeconds = 0.001 * (currentNote.StartTime - previousNote.StartTime);
            double hitLeniency = Math.Max(data.HitLeniency, deltaTimeSeconds);

            double crossIntensity = 0.16 * (1.0 / (hitLeniency * hitLeniency));

            applyActivityModifiers(ref crossIntensity, boundaryIndex, data.KeyCount, startIndex, endIndex,
                timePointCount, activeByTime, crossHandCoefficients);

            // Calculate fast pressure component for rapid patterns
            double fastPressure = calculateFastPressure(deltaTimeSeconds, data.HitLeniency);

            applyValuesToTimeRange(crossIntensityBuffer, fastPressureBuffer, baseOffset, startIndex, endIndex,
                timePointCount, crossIntensity, fastPressure);
        }

        /// <summary>
        /// Applies modifiers based on whether the adjacent columns are active.
        /// Less active columns receive reduced cross-column pressure.
        /// </summary>
        private static void applyActivityModifiers(ref double crossIntensity, int boundaryIndex, int keyCount, int startIndex, int endIndex, int timePointCount, byte[] activeByTime, double[] crossHandCoefficients)
        {
            bool leftColumnActive = false, rightColumnActive = false;
            int timePointAdjusted = Math.Min(endIndex, timePointCount - 1);

            // Check if left column is active
            if (boundaryIndex - 1 >= 0 && boundaryIndex - 1 < keyCount)
            {
                int leftOffset = (boundaryIndex - 1) * timePointCount;
                if (activeByTime[leftOffset + startIndex] != 0 || activeByTime[leftOffset + timePointAdjusted] != 0)
                    leftColumnActive = true;
            }

            // Check if right column is active
            if (boundaryIndex >= 0 && boundaryIndex < keyCount)
            {
                int rightOffset = boundaryIndex * timePointCount;
                if (activeByTime[rightOffset + startIndex] != 0 || activeByTime[rightOffset + timePointAdjusted] != 0)
                    rightColumnActive = true;
            }

            // Reduce intensity if either column is inactive
            if (!leftColumnActive || !rightColumnActive)
                crossIntensity *= (1.0 - crossHandCoefficients[boundaryIndex]);
        }

        /// <summary>
        /// Calculates the fast pressure component for rapid note sequences.
        /// </summary>
        private static double calculateFastPressure(double deltaTimeSeconds, double hitLeniency)
        {
            double minDelta = Math.Max(deltaTimeSeconds, 0.06);
            double hitLeniencyAdjusted = 0.75 * hitLeniency;

            if (hitLeniencyAdjusted > minDelta)
                minDelta = hitLeniencyAdjusted;

            double fastPressureRaw = 0.4 * (1.0 / (minDelta * minDelta)) - 80.0;
            return fastPressureRaw > 0.0 ? fastPressureRaw : 0.0;
        }

        /// <summary>
        /// Applies the calculated intensity values to the specified time range.
        /// </summary>
        private static void applyValuesToTimeRange(double[] crossIntensityBuffer, double[] fastPressureBuffer, int baseOffset, int startIndex, int endIndex, int timePointCount, double crossIntensity, double fastPressure)
        {
            for (int timeIndex = startIndex; timeIndex < endIndex && timeIndex < timePointCount; timeIndex++)
            {
                crossIntensityBuffer[baseOffset + timeIndex] = crossIntensity;
                fastPressureBuffer[baseOffset + timeIndex] = fastPressure;
            }
        }

        /// <summary>
        /// Combines all calculated cross-column values into the final pressure array.
        /// </summary>
        private static void combineCrossColumnValues(double[] crossColumnPressure, double[] crossIntensityBuffer, double[] fastPressureBuffer, double[] crossHandCoefficients, int keyCount, int timePointCount)
        {
            for (int timeIndex = 0; timeIndex < timePointCount; timeIndex++)
            {
                // Sum weighted cross-column intensities
                double crossSum = 0.0;

                for (int boundaryIndex = 0; boundaryIndex <= keyCount; boundaryIndex++)
                {
                    crossSum += crossIntensityBuffer[boundaryIndex * timePointCount + timeIndex] * crossHandCoefficients[boundaryIndex];
                }

                // Sum fast pressure interactions between adjacent columns
                double fastSum = 0.0;

                for (int keyIndex = 0; keyIndex < keyCount; keyIndex++)
                {
                    double leftFast = fastPressureBuffer[keyIndex * timePointCount + timeIndex];
                    double rightFast = fastPressureBuffer[(keyIndex + 1) * timePointCount + timeIndex];

                    fastSum += Math.Sqrt(Math.Max(0.0, leftFast) * crossHandCoefficients[keyIndex] * Math.Max(0.0, rightFast) * crossHandCoefficients[keyIndex + 1]);
                }

                crossColumnPressure[timeIndex] = crossSum + fastSum;
            }
        }

        /// <summary>
        /// Gets the cross-hand coefficients that determine how much cross-column difficulty
        /// each column boundary contributes based on the total key count.
        /// These values are tuned based on typical finger layouts and hand coordination.
        /// </summary>
        private static double[] getCrossHandCoefficients(int keyCount)
        {
            // Pre-calculated coefficient matrices for different key counts (1K to 10K)
            double[][] crossMatrix =
            {
                new[] { 0.075, 0.075 }, // 1K
                new[] { 0.125, 0.05, 0.125 }, // 2K
                new[] { 0.125, 0.125, 0.125, 0.125 }, // 3K
                new[] { 0.175, 0.25, 0.05, 0.25, 0.175 }, // 4K
                new[] { 0.175, 0.25, 0.175, 0.175, 0.25, 0.175 }, // 5K
                new[] { 0.225, 0.35, 0.25, 0.05, 0.25, 0.35, 0.225 }, // 6K
                new[] { 0.225, 0.35, 0.25, 0.225, 0.225, 0.25, 0.35, 0.225 }, // 7K
                new[] { 0.275, 0.45, 0.35, 0.25, 0.05, 0.25, 0.35, 0.45, 0.275 }, // 8K
                new[] { 0.275, 0.45, 0.35, 0.25, 0.275, 0.275, 0.25, 0.35, 0.45, 0.275 }, // 9K
                new[] { 0.325, 0.55, 0.45, 0.35, 0.25, 0.05, 0.25, 0.35, 0.45, 0.55, 0.325 } // 10K
            };

            if (keyCount >= 1 && keyCount <= 10)
            {
                double[] sourceRow = crossMatrix[keyCount - 1];
                double[] result = new double[keyCount + 1];
                Array.Clear(result, 0, result.Length);
                Array.Copy(sourceRow, result, Math.Min(sourceRow.Length, result.Length));
                return result;
            }

            // Fallback for unsupported key counts - use uniform distribution
            double[] fallback = new double[keyCount + 1];
            for (int i = 0; i < fallback.Length; i++)
                fallback[i] = 1.0 / fallback.Length;
            return fallback;
        }

        /// <summary>
        /// Computes which keys are considered "active" (recently pressed or held) at each time point.
        /// This is used to determine when cross-column patterns are more difficult due to finger coordination.
        /// </summary>
        public static bool[][] ComputeKeyUsage(ManiaDifficultyContext data)
        {
            int timePoints = data.CornerData.BaseTimeCorners.Length;
            bool[][] keyUsage = new bool[data.KeyCount][];

            for (int column = 0; column < data.KeyCount; column++)
                keyUsage[column] = new bool[timePoints];

            for (int noteIndex = 0; noteIndex < data.AllNotes.Count; noteIndex++)
            {
                var note = data.AllNotes[noteIndex];

                // Calculate the active window around this note
                double windowStartTime = Math.Max(0, note.StartTime - column_activity_window_ms);
                double windowEndTime;

                if (note.IsLong)
                    windowEndTime = Math.Min(data.MaxTime - 1, note.EndTime + column_activity_window_ms);
                else
                    windowEndTime = Math.Min(data.MaxTime - 1, note.StartTime + column_activity_window_ms);

                int startIndex = Math.Max(0, StrainArrayUtils.FindLeftBound(data.CornerData.BaseTimeCorners, windowStartTime));
                int endIndex = Math.Max(0, StrainArrayUtils.FindLeftBound(data.CornerData.BaseTimeCorners, windowEndTime));

                if (startIndex >= endIndex) continue;

                // Mark the column as active during this time window
                int column = note.Column;

                if (column >= 0 && column < data.KeyCount)
                {
                    bool[] columnUsageArray = keyUsage[column];
                    for (int timeIndex = startIndex; timeIndex < endIndex && timeIndex < timePoints; timeIndex++)
                        columnUsageArray[timeIndex] = true;
                }
            }

            return keyUsage;
        }
    }
}
