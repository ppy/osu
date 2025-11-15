// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;
using System.Collections.Generic;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Corner
{
    /// <summary>
    /// Generates time-based sampling points (<see cref="CornerData"/>) for difficulty calculation.
    /// Computes three distinct corner sets from hit objects:
    /// 1. Base corners (fundamental pattern changes)
    /// 2. Accuracy corners (timing precision windows)
    /// 3. Union corners (final sampling points)
    /// </summary>
    public class CornerPreprocessor
    {
        /// <summary>
        /// Processes hit objects to generate time-based sampling points (<see cref="CornerData"/>) for difficulty calculation.
        /// This method constructs three distinct sets of time corners:
        /// <list type="number">
        /// <item>
        /// <description><see cref="CornerData.BaseTimeCorners"/>: Fundamental pattern change points with micro-offsets to capture nuances</description>
        /// </item>
        /// <item>
        /// <description><see cref="CornerData.AccuracyTimeCorners"/>: Timing precision windows centered around hit objects</description>
        /// </item>
        /// <item>
        /// <description><see cref="CornerData.TimeCorners"/>: Union of base and accuracy corners forming final sampling points</description>
        /// </item>
        /// </list>
        ///
        /// The generated corners serve as temporal anchors where difficulty metrics are evaluated during calculation.
        /// </summary>
        /// <param name="data">The mania difficulty context containing hit objects and timing boundaries</param>
        public static void ProcessAndAssign(ManiaDifficultyContext data)
        {
            var cornerData = new CornerData();
            var hitObjects = data.AllNotes;

            if (hitObjects.Count == 0)
            {
                cornerData.BaseTimeCorners = new[] { 0.0, data.MaxTime };
                cornerData.AccuracyTimeCorners = cornerData.BaseTimeCorners;
                cornerData.TimeCorners = cornerData.BaseTimeCorners;
                data.CornerData = cornerData;
                return;
            }

            int noteCount = hitObjects.Count;
            var arrayPool = ArrayPool<double>.Shared;

            // Collect fundamental time points from hit objects
            double[] baseTimePoints = collectBaseTimePoints(hitObjects, noteCount, arrayPool);

            // Build specialized corner sets
            cornerData.BaseTimeCorners = buildBaseTimeCorners(baseTimePoints, arrayPool, data.MaxTime);
            cornerData.AccuracyTimeCorners = buildAccuracyTimeCorners(baseTimePoints, arrayPool, data.MaxTime);
            cornerData.TimeCorners = createTimeCornerUnion(cornerData.BaseTimeCorners, cornerData.AccuracyTimeCorners);

            data.CornerData = cornerData;
        }

        /// <summary>
        /// Collects fundamental time points from hit objects (start/end times of notes).
        /// </summary>
        /// <returns>Array of unique time points from hit objects</returns>
        private static double[] collectBaseTimePoints(List<ManiaDifficultyHitObject> allNotes, int noteCount, ArrayPool<double> pool)
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
        private static double[] buildBaseTimeCorners(double[] baseTimePoints, ArrayPool<double> pool, int maxTime)
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
        /// Builds accuracy-focused time corners with wider windows around notes.
        /// Emphasizes timing precision requirements by sampling around hit windows.
        /// </summary>
        /// <param name="baseTimePoints">Fundamental time points from hit objects</param>
        /// <param name="pool">Array pool for efficient memory usage</param>
        /// <param name="maxTime">Maximum time boundary</param>
        /// <returns>Sorted, deduplicated accuracy time corners within valid time range</returns>
        private static double[] buildAccuracyTimeCorners(double[] baseTimePoints, ArrayPool<double> pool, int maxTime)
        {
            int basePointCount = baseTimePoints.Length;
            int accuracyCapacity = Math.Max(8, basePointCount * 3 + 2);
            double[] accuracyPoints = pool.Rent(accuracyCapacity);
            int accuracyCount = 0;

            // Add boundary points first
            if (accuracyCount >= accuracyPoints.Length)
                accuracyPoints = expandArray(accuracyPoints, ref accuracyCount, pool);
            accuracyPoints[accuracyCount++] = 0;

            if (accuracyCount >= accuracyPoints.Length)
                accuracyPoints = expandArray(accuracyPoints, ref accuracyCount, pool);
            accuracyPoints[accuracyCount++] = maxTime;

            // Add points with wide timing windows
            for (int i = 0; i < basePointCount; i++)
            {
                double baseTime = baseTimePoints[i];
                double windowBefore = baseTime - 1000; // 1 second before note
                double windowAfter = baseTime + 1000; // 1 second after note

                // Add valid pre-window point
                if (windowBefore >= 0 && windowBefore <= maxTime)
                {
                    if (accuracyCount >= accuracyPoints.Length)
                        accuracyPoints = expandArray(accuracyPoints, ref accuracyCount, pool);
                    accuracyPoints[accuracyCount++] = windowBefore;
                }

                // Add core note time
                if (baseTime >= 0 && baseTime <= maxTime)
                {
                    if (accuracyCount >= accuracyPoints.Length)
                        accuracyPoints = expandArray(accuracyPoints, ref accuracyCount, pool);
                    accuracyPoints[accuracyCount++] = baseTime;
                }

                // Add valid post-window point
                if (windowAfter >= 0 && windowAfter <= maxTime)
                {
                    if (accuracyCount >= accuracyPoints.Length)
                        accuracyPoints = expandArray(accuracyPoints, ref accuracyCount, pool);
                    accuracyPoints[accuracyCount++] = windowAfter;
                }
            }

            // Process, sort and deduplicate points
            double[] result = processAndSortTimePoints(accuracyPoints, accuracyCount, maxTime);
            pool.Return(accuracyPoints, clearArray: true);

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
        /// Creates the union of base and accuracy time corners.
        /// Ensures final sampling points cover both pattern changes and timing windows.
        /// </summary>
        /// <param name="baseTimeCorners">Base time corners</param>
        /// <param name="accuracyTimeCorners">Accuracy time corners</param>
        /// <returns>Sorted, deduplicated union of both corner sets</returns>
        private static double[] createTimeCornerUnion(double[] baseTimeCorners, double[] accuracyTimeCorners)
        {
            int baseLength = baseTimeCorners.Length;
            int accuracyLength = accuracyTimeCorners.Length;
            double[] unionArray = new double[baseLength + accuracyLength];

            int baseIndex = 0, accuracyIndex = 0, unionIndex = 0;

            // Merge sorted arrays while deduplicating
            while (baseIndex < baseLength || accuracyIndex < accuracyLength)
            {
                double baseValue = baseIndex < baseLength ? baseTimeCorners[baseIndex] : double.MaxValue;
                double accuracyValue = accuracyIndex < accuracyLength ? accuracyTimeCorners[accuracyIndex] : double.MaxValue;

                if (baseValue < accuracyValue)
                {
                    if (unionIndex == 0 || unionArray[unionIndex - 1] != baseValue)
                        unionArray[unionIndex++] = baseValue;
                    baseIndex++;
                }
                else if (accuracyValue < baseValue)
                {
                    if (unionIndex == 0 || unionArray[unionIndex - 1] != accuracyValue)
                        unionArray[unionIndex++] = accuracyValue;
                    accuracyIndex++;
                }
                else
                {
                    if (unionIndex == 0 || unionArray[unionIndex - 1] != baseValue)
                        unionArray[unionIndex++] = baseValue;
                    baseIndex++;
                    accuracyIndex++;
                }
            }

            // Trim to actual size
            double[] result = new double[unionIndex];
            Array.Copy(unionArray, 0, result, 0, unionIndex);
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
