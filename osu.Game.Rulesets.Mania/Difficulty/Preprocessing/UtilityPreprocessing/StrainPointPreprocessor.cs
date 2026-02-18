// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;
using System.Collections.Generic;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Data;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.UtilityPreprocessing
{
    /// <summary>
    /// Generates time-based sampling points for difficulty calculation.
    /// </summary>
    public class StrainPointPreprocessor
    {
        /// <summary>
        /// Processes hit objects to generate time-based sampling points for difficulty calculation.
        /// The generated corners serve as temporal anchors where difficulty metrics are evaluated during calculation.
        /// </summary>
        /// <param name="data">The mania difficulty context containing hit objects</param>
        public static double[] CreateStrainPoints(ManiaDifficultyData data)
        {
            var hitObjects = data.AllNotes;

            if (hitObjects.Count == 0)
            {
                return new[] { 0.0, data.MaxTime };
            }

            int noteCount = hitObjects.Count;
            var arrayPool = ArrayPool<double>.Shared;

            // Collect fundamental time points from hit objects
            double[] baseTimePoints = getStrainPointBaseTimes(hitObjects, noteCount, arrayPool);

            // Build specialized sets of points at locations relevant for difficulty
            return buildStrainPoints(baseTimePoints, arrayPool, data.MaxTime);
        }

        /// <summary>
        /// Collects fundamental time points from hit objects (start/end times of notes).
        /// </summary>
        /// <returns>Array of unique time points from hit objects</returns>
        private static double[] getStrainPointBaseTimes(List<ManiaDifficultyHitObject> allNotes, int noteCount, ArrayPool<double> pool)
        {
            int estimatedCapacity = Math.Max(32, noteCount * 3);
            double[] timePoints = pool.Rent(estimatedCapacity);
            int timePointCount = 0;

            for (int i = 0; i < noteCount; i++)
            {
                var note = allNotes[i];

                // Expand array if needed
                if (timePointCount >= timePoints.Length)
                    timePoints = expandArray(timePoints, ref timePointCount, pool);

                // Add note start time
                timePoints[timePointCount++] = note.StartTime;

                // Add note end time for long notes
                if (note.IsLong)
                {
                    if (timePointCount >= timePoints.Length)
                        timePoints = expandArray(timePoints, ref timePointCount, pool);
                    timePoints[timePointCount++] = note.EndTime;
                }
            }

            // Trim and return final array
            double[] result = new double[timePointCount];
            Array.Copy(timePoints, 0, result, 0, timePointCount);
            pool.Return(timePoints, clearArray: true);

            return result;
        }

        /// <summary>
        /// Builds fundamental time corners from base time points with small offsets.
        /// Creates sampling points that capture core pattern changes.
        /// </summary>
        /// <param name="baseTimePoints">Fundamental time points from hit objects</param>
        /// <param name="pool">Array pool for efficient memory usage</param>
        /// <param name="maxTime">Maximum time boundary</param>
        /// <returns>Sorted, deduplicated base time corners within valid time range</returns>
        private static double[] buildStrainPoints(double[] baseTimePoints, ArrayPool<double> pool, int maxTime)
        {
            int basePointCount = baseTimePoints.Length;
            int expandedCapacity = Math.Max(8, basePointCount * 4 + 4);
            double[] expandedPoints = pool.Rent(expandedCapacity);
            int expandedCount = 0;

            // Add base points with small offsets to capture pattern nuances
            for (int i = 0; i < basePointCount; i++)
            {
                double baseTime = baseTimePoints[i];
                if (baseTime < 0 || baseTime > maxTime) continue;

                // Ensure capacity before adding points
                if (expandedCount + 4 >= expandedPoints.Length)
                    expandedPoints = expandArray(expandedPoints, ref expandedCount, pool);

                // Add core point and strategic offsets
                expandedPoints[expandedCount++] = baseTime; // Exact note time
                expandedPoints[expandedCount++] = baseTime + 501; // Post-note offset
                expandedPoints[expandedCount++] = baseTime - 499; // Pre-note offset
                expandedPoints[expandedCount++] = baseTime + 1; // Minimal offset
            }

            // Add boundary points
            if (expandedCount + 2 >= expandedPoints.Length)
                expandedPoints = expandArray(expandedPoints, ref expandedCount, pool);
            expandedPoints[expandedCount++] = 0;
            expandedPoints[expandedCount++] = maxTime;

            // Process, sort and deduplicate points
            double[] result = processAndSortTimePoints(expandedPoints, expandedCount, maxTime);
            pool.Return(expandedPoints, clearArray: true);

            return result;
        }

        /// <summary>
        /// Processes raw time points by sorting, deduplicating, and clamping to valid range.
        /// </summary>
        /// <param name="timePoints">Raw time points to process</param>
        /// <param name="count">Number of valid points in the array</param>
        /// <param name="maxTime">Maximum valid time value</param>
        /// <returns>Sorted, deduplicated array of valid time points</returns>
        private static double[] processAndSortTimePoints(double[] timePoints, int count, int maxTime)
        {
            // Sort relevant portion of array
            Array.Sort(timePoints, 0, count);

            int writeIndex = 0;

            // Deduplicate and filter invalid points
            for (int readIndex = 0; readIndex < count; readIndex++)
            {
                double currentValue = timePoints[readIndex];
                if (currentValue < 0 || currentValue > maxTime) continue;

                // Skip duplicates
                if (writeIndex == 0 || timePoints[readIndex] != timePoints[writeIndex - 1])
                {
                    timePoints[writeIndex++] = currentValue;
                }
            }

            // Create final trimmed array
            double[] result = new double[writeIndex];
            for (int i = 0; i < writeIndex; i++)
                result[i] = timePoints[i];

            return result;
        }

        /// <summary>
        /// Expands an array using pooled memory when capacity is exceeded.
        /// </summary>
        /// <param name="array">Current array to expand</param>
        /// <param name="currentSize">Current number of valid elements</param>
        /// <param name="pool">Array pool to rent new arrays from</param>
        /// <returns>New expanded array containing existing elements</returns>
        private static double[] expandArray(double[] array, ref int currentSize, ArrayPool<double> pool)
        {
            int newSize = Math.Max(array.Length * 2, currentSize + 4);
            double[] newArray = pool.Rent(newSize);
            Array.Copy(array, 0, newArray, 0, currentSize);
            pool.Return(array, clearArray: true);
            return newArray;
        }
    }
}
