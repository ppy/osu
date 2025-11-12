// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mania.Difficulty.Utils;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Components
{
    public class SameColumnPreprocessor
    {
        /// <summary>
        /// Computes the same-column pressure values across all time points.
        /// This measures the difficulty of rapid repeated presses in individual columns.
        /// </summary>
        public static double[] ComputeValues(ManiaDifficultyContext data)
        {
            int timePointCount = data.CornerData.BaseTimeCorners.Length;
            if (timePointCount == 0) return Array.Empty<double>();

            double[] baseTimeCorners = data.CornerData.BaseTimeCorners;
            int keyCount = data.KeyCount;
            double hitLeniencyFactorSqrt = Math.Sqrt(Math.Sqrt(data.HitLeniency));

            double[] combinedSameColumn = new double[timePointCount];
            var arrayPool = ArrayPool<double>.Shared;
            int arraySliceSize = Math.Max(1, timePointCount);

            double[] sameColumnIntensityBuffer = arrayPool.Rent(keyCount * arraySliceSize);
            double[] deltaTimeBuffer = arrayPool.Rent(keyCount * arraySliceSize);

            try
            {
                Array.Clear(sameColumnIntensityBuffer, 0, keyCount * arraySliceSize);
                for (int i = 0; i < keyCount * arraySliceSize; i++)
                    deltaTimeBuffer[i] = 1e9; // Large value indicating no pattern

                calculatePerColumnIntensities(data, sameColumnIntensityBuffer, deltaTimeBuffer,
                    baseTimeCorners, hitLeniencyFactorSqrt, keyCount, timePointCount);
                applyColumnSmoothing(sameColumnIntensityBuffer, baseTimeCorners, keyCount, timePointCount, arraySliceSize);
                combineColumnValues(combinedSameColumn, sameColumnIntensityBuffer, deltaTimeBuffer,
                    keyCount, timePointCount);
            }
            finally
            {
                arrayPool.Return(sameColumnIntensityBuffer, clearArray: true);
                arrayPool.Return(deltaTimeBuffer, clearArray: true);
            }

            // Interpolate to match target time resolution
            return StrainArrayUtils.InterpolateArray(
                data.CornerData.TimeCorners,
                data.CornerData.BaseTimeCorners,
                combinedSameColumn
            );
        }

        /// <summary>
        /// Calculates the intensity values for each column independently.
        /// This finds rapid sequences within individual columns.
        /// </summary>
        private static void calculatePerColumnIntensities(ManiaDifficultyContext data, double[] intensityBuffer, double[] deltaBuffer, double[] baseTimeCorners, double hitLeniencyFactor, int keyCount, int timePointCount)
        {
            List<ManiaDifficultyHitObject>[] notesByColumn = data.AllNotes.First().PerColumnObjects.Select(list => list.Cast<ManiaDifficultyHitObject>().ToList()).ToArray();

            //var notesByColumn = data.PerColumnObjects;

            for (int column = 0; column < keyCount; column++)
            {
                var columnNotes = notesByColumn[column];
                if (columnNotes.Count < 2) continue; // Need at least 2 notes for a pattern

                int bufferOffset = column * timePointCount;
                int searchIndex = 0;

                // Process consecutive note pairs in this column
                for (int noteIndex = 0; noteIndex + 1 < columnNotes.Count; noteIndex++)
                {
                    var currentNote = columnNotes[noteIndex];
                    var nextNote = currentNote.NextInColumn();

                    if (nextNote == null) break;

                    // Find time range for this note pair
                    int startTimeIndex = StrainArrayUtils.FindLeftBoundProgressive(baseTimeCorners, ref searchIndex, currentNote.StartTime);
                    int endTimeIndex = StrainArrayUtils.FindLeftBoundProgressive(baseTimeCorners, ref searchIndex, nextNote.StartTime);

                    if (endTimeIndex <= startTimeIndex) continue;

                    double deltaTimeSeconds = 0.001 * (nextNote.StartTime - currentNote.StartTime);

                    // Calculate base intensity - faster = harder
                    double baseIntensity = (1.0 / deltaTimeSeconds) * (1.0 / (deltaTimeSeconds + 0.11 * hitLeniencyFactor));

                    // Apply jack nerf (same-column patterns get harder the closer they are to optimal jack timing)
                    double jackNerf = calculateJackNerf(deltaTimeSeconds);
                    double finalIntensity = baseIntensity * jackNerf;

                    // Apply values to the time range
                    int rangeLength = Math.Min(endTimeIndex, timePointCount) - startTimeIndex;

                    for (int i = 0; i < rangeLength; i++)
                    {
                        int bufferPosition = bufferOffset + startTimeIndex + i;
                        intensityBuffer[bufferPosition] = finalIntensity;
                        deltaBuffer[bufferPosition] = deltaTimeSeconds;
                    }
                }
            }
        }

        /// <summary>
        /// Calculates the jack nerf factor based on timing relative to optimal jack speed.
        /// Jacks (rapid same-column presses) have an optimal timing that's hardest to execute.
        /// </summary>
        private static double calculateJackNerf(double deltaTimeSeconds)
        {
            FormulaConfig config = new FormulaConfig();
            double timingDeviation = Math.Abs(deltaTimeSeconds - 0.08);
            double nerfBase = config.JackNerfBase + timingDeviation;
            double nerfValue = config.JackNerfCoefficient * Math.Pow(nerfBase, config.JackNerfPower);

            return 1.0 - nerfValue; // Return nerf factor (lower = more nerf)
        }

        /// <summary>
        /// Applies smoothing to each column's intensity values independently using the consolidated utility.
        /// This creates more stable difficulty curves.
        /// </summary>
        private static void applyColumnSmoothing(double[] intensityBuffer, double[] baseTimeCorners, int keyCount, int timePointCount, int arraySliceSize)
        {
            FormulaConfig config = new FormulaConfig();

            var arrayPool = ArrayPool<double>.Shared;
            double[] smoothedBuffer = arrayPool.Rent(keyCount * arraySliceSize);
            Array.Clear(smoothedBuffer, 0, keyCount * arraySliceSize);

            try
            {
                for (int column = 0; column < keyCount; column++)
                {
                    int columnOffset = column * timePointCount;

                    StrainArrayUtils.ApplySmoothingToColumn(intensityBuffer, smoothedBuffer, baseTimeCorners,
                        columnOffset, timePointCount, config.SmoothingWindowMs);
                }

                Array.Copy(smoothedBuffer, 0, intensityBuffer, 0, keyCount * timePointCount);
            }
            finally
            {
                arrayPool.Return(smoothedBuffer, clearArray: true);
            }
        }

        /// <summary>
        /// Combines intensity values from all columns into a single same-column pressure value.
        /// Uses weighted power mean to emphasize the most difficult columns.
        /// </summary>
        private static void combineColumnValues(double[] combinedResult, double[] intensityBuffer, double[] deltaBuffer, int keyCount, int timePointCount)
        {
            for (int timeIndex = 0; timeIndex < timePointCount; timeIndex++)
            {
                double weightedNumerator = 0.0;
                double totalWeight = 0.0;

                // Process each column's contribution at this time point
                for (int column = 0; column < keyCount; column++)
                {
                    int bufferPosition = column * timePointCount + timeIndex;
                    double columnIntensity = intensityBuffer[bufferPosition];

                    if (columnIntensity <= 0.0) continue;

                    double deltaTime = deltaBuffer[bufferPosition];

                    // Faster patterns usually get more weight here
                    double weight = deltaTime < 1e8 ? (1.0 / deltaTime) : 0.0;

                    if (weight <= 0.0) continue;

                    // Use 5th power to emphasize difficult patterns
                    double poweredIntensity = Math.Pow(columnIntensity, 5.0);
                    weightedNumerator += poweredIntensity * weight;
                    totalWeight += weight;
                }

                // Calculate weighted power mean (reverse the 5th power)
                combinedResult[timeIndex] = totalWeight > 1e-9 ? Math.Pow(weightedNumerator / totalWeight, 1.0 / 5.0) : 0.0;
            }
        }
    }
}
