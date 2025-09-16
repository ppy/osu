// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Corner
{
    public class CornerData
    {
        /// <summary>Time corners used for final difficulty sampling (union of Base and Accuracy corners)</summary>
        public double[] TimeCorners { get; set; } = Array.Empty<double>();

        /// <summary>Base time corners derived from note positions with small offsets</summary>
        public double[] BaseTimeCorners { get; set; } = Array.Empty<double>();

        /// <summary>Accuracy-focused time corners with wider windows around notes</summary>
        public double[] AccuracyTimeCorners { get; set; } = Array.Empty<double>();

        /// <summary>
        /// Computes all time corner arrays based on note positions.
        /// This creates strategic time points for efficient difficulty calculation.
        /// </summary>
        public void ComputeTimeCorners(SunnyPreprocessor.Note[] allNotes, int maxTime)
        {
            int noteCount = allNotes?.Length ?? 0;

            if (noteCount == 0)
            {
                // No notes - create minimal time corners
                BaseTimeCorners = new[] { 0.0, maxTime };
                AccuracyTimeCorners = BaseTimeCorners;
                TimeCorners = BaseTimeCorners;
                return;
            }

            var arrayPool = ArrayPool<int>.Shared;

            // Collect all significant time points from notes
            int[] baseTimePoints = collectBaseTimePoints(allNotes, noteCount, arrayPool);

            BaseTimeCorners = buildBaseTimeCorners(baseTimePoints, arrayPool, maxTime);
            AccuracyTimeCorners = buildAccuracyTimeCorners(baseTimePoints, arrayPool, maxTime);
            TimeCorners = createTimeCornerUnion();
        }

        /// <summary>
        /// Collects all fundamental time points from notes (start/end times).
        /// </summary>
        private int[] collectBaseTimePoints(SunnyPreprocessor.Note[] allNotes, int noteCount, ArrayPool<int> pool)
        {
            int estimatedCapacity = Math.Max(32, noteCount * 3);
            int[] timePoints = pool.Rent(estimatedCapacity);
            int timePointCount = 0;

            for (int i = 0; i < noteCount; i++)
            {
                var note = allNotes[i];

                // Ensure we don't exceed array capacity
                if (timePointCount >= timePoints.Length)
                    timePoints = expandIntArray(timePoints, ref timePointCount, pool);

                timePoints[timePointCount++] = note.StartTime;

                if (note.IsLong)
                {
                    if (timePointCount >= timePoints.Length)
                        timePoints = expandIntArray(timePoints, ref timePointCount, pool);
                    timePoints[timePointCount++] = note.EndTime;
                }
            }

            // Return only the used portion
            int[] result = new int[timePointCount];
            Array.Copy(timePoints, 0, result, 0, timePointCount);
            pool.Return(timePoints, clearArray: true);

            return result;
        }

        /// <summary>
        /// Builds base time corners with small offsets around each note time.
        /// These corners are used for most difficulty calculations.
        /// </summary>
        private double[] buildBaseTimeCorners(int[] baseTimePoints, ArrayPool<int> pool, int maxTime)
        {
            int basePointCount = baseTimePoints.Length;
            int expandedCapacity = Math.Max(8, basePointCount * 4 + 4);
            int[] expandedPoints = pool.Rent(expandedCapacity);
            int expandedCount = 0;

            // Create small offsets around each base time point
            for (int i = 0; i < basePointCount; i++)
            {
                int baseTime = baseTimePoints[i];
                if (baseTime < 0 || baseTime > maxTime) continue;

                // Ensure capacity for 4 new points
                if (expandedCount + 4 >= expandedPoints.Length)
                    expandedPoints = expandIntArray(expandedPoints, ref expandedCount, pool);

                // Add the base time and small offsets
                expandedPoints[expandedCount++] = baseTime;
                expandedPoints[expandedCount++] = baseTime + 501; // Just after hit window
                expandedPoints[expandedCount++] = baseTime - 499; // Just before hit window
                expandedPoints[expandedCount++] = baseTime + 1; // Tiny offset for precision
            }

            // Add boundary points
            if (expandedCount + 2 >= expandedPoints.Length)
                expandedPoints = expandIntArray(expandedPoints, ref expandedCount, pool);
            expandedPoints[expandedCount++] = 0;
            expandedPoints[expandedCount++] = maxTime;

            double[] result = processAndSortTimePoints(expandedPoints, expandedCount, maxTime);
            pool.Return(expandedPoints, clearArray: true);

            return result;
        }

        /// <summary>
        /// Builds accuracy time corners with wider windows around notes.
        /// These are used specifically for accuracy-related calculations.
        /// </summary>
        private double[] buildAccuracyTimeCorners(int[] baseTimePoints, ArrayPool<int> pool, int maxTime)
        {
            int basePointCount = baseTimePoints.Length;
            int accuracyCapacity = Math.Max(8, basePointCount * 3 + 2);
            int[] accuracyPoints = pool.Rent(accuracyCapacity);
            int accuracyCount = 0;

            // Add boundary points
            if (accuracyCount >= accuracyPoints.Length)
                accuracyPoints = expandIntArray(accuracyPoints, ref accuracyCount, pool);
            accuracyPoints[accuracyCount++] = 0;

            if (accuracyCount >= accuracyPoints.Length)
                accuracyPoints = expandIntArray(accuracyPoints, ref accuracyCount, pool);
            accuracyPoints[accuracyCount++] = maxTime;

            // Add wider windows around each note
            for (int i = 0; i < basePointCount; i++)
            {
                int baseTime = baseTimePoints[i];

                // Create 1-second windows around each note
                int windowBefore = baseTime - 1000;
                int windowAfter = baseTime + 1000;

                // Add window start if it's within valid range
                if (windowBefore >= 0 && windowBefore <= maxTime)
                {
                    if (accuracyCount >= accuracyPoints.Length)
                        accuracyPoints = expandIntArray(accuracyPoints, ref accuracyCount, pool);
                    accuracyPoints[accuracyCount++] = windowBefore;
                }

                // Add the base time
                if (baseTime >= 0 && baseTime <= maxTime)
                {
                    if (accuracyCount >= accuracyPoints.Length)
                        accuracyPoints = expandIntArray(accuracyPoints, ref accuracyCount, pool);
                    accuracyPoints[accuracyCount++] = baseTime;
                }

                // Add window end if it's within valid range
                if (windowAfter >= 0 && windowAfter <= maxTime)
                {
                    if (accuracyCount >= accuracyPoints.Length)
                        accuracyPoints = expandIntArray(accuracyPoints, ref accuracyCount, pool);
                    accuracyPoints[accuracyCount++] = windowAfter;
                }
            }

            double[] result = processAndSortTimePoints(accuracyPoints, accuracyCount, maxTime);
            pool.Return(accuracyPoints, clearArray: true);

            return result;
        }

        /// <summary>
        /// Processes and sorts time points, removing duplicates.
        /// </summary>
        private double[] processAndSortTimePoints(int[] timePoints, int count, int maxTime)
        {
            Array.Sort(timePoints, 0, count);

            int writeIndex = 0;

            for (int readIndex = 0; readIndex < count; readIndex++)
            {
                int currentValue = timePoints[readIndex];
                if (currentValue < 0 || currentValue > maxTime) continue;

                if (writeIndex == 0 || timePoints[readIndex] != timePoints[writeIndex - 1])
                {
                    timePoints[writeIndex++] = currentValue;
                }
            }

            double[] result = new double[writeIndex];
            for (int i = 0; i < writeIndex; i++)
                result[i] = timePoints[i];

            return result;
        }

        /// <summary>
        /// Creates a union of BaseTimeCorners and AccuracyTimeCorners.
        /// This provides comprehensive time coverage for all difficulty calculations.
        /// </summary>
        private double[] createTimeCornerUnion()
        {
            int baseLength = BaseTimeCorners.Length;
            int accuracyLength = AccuracyTimeCorners.Length;
            double[] unionArray = new double[baseLength + accuracyLength];

            int baseIndex = 0, accuracyIndex = 0, unionIndex = 0;

            // Merge the two sorted arrays while removing duplicates
            while (baseIndex < baseLength || accuracyIndex < accuracyLength)
            {
                double baseValue = baseIndex < baseLength ? BaseTimeCorners[baseIndex] : double.MaxValue;
                double accuracyValue = accuracyIndex < accuracyLength ? AccuracyTimeCorners[accuracyIndex] : double.MaxValue;

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
                    // Both values are equal - add once and advance both indices
                    if (unionIndex == 0 || unionArray[unionIndex - 1] != baseValue)
                        unionArray[unionIndex++] = baseValue;
                    baseIndex++;
                    accuracyIndex++;
                }
            }

            double[] result = new double[unionIndex];
            Array.Copy(unionArray, 0, result, 0, unionIndex);
            return result;
        }

        /// <summary>
        /// Expands an integer array when more capacity is needed.
        /// </summary>
        private static int[] expandIntArray(int[] array, ref int currentSize, ArrayPool<int> pool)
        {
            int newSize = Math.Max(array.Length * 2, currentSize + 4);
            int[] newArray = pool.Rent(newSize);
            Array.Copy(array, 0, newArray, 0, currentSize);
            pool.Return(array, clearArray: true);
            return newArray;
        }
    }
}
