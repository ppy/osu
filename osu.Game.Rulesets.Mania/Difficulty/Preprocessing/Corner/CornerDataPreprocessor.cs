// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;
using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Corner.Data;

namespace osu.Game.Rulesets.Mania.Difficulty.Preprocessing.Corner
{
    /// <summary>
    /// Preprocessor that computes time corners for difficulty sampling.
    /// Separates computation logic from data storage.
    /// </summary>
    public class CornerDataPreprocessor
    {
        public static CornerData Process(List<DifficultyHitObject> hitObjects, int maxTime)
        {
            var cornerData = new CornerData();

            if (hitObjects.Count == 0)
            {
                cornerData.BaseTimeCorners = new[] { 0.0, maxTime };
                cornerData.AccuracyTimeCorners = cornerData.BaseTimeCorners;
                cornerData.TimeCorners = cornerData.BaseTimeCorners;
                return cornerData;
            }

            int noteCount = hitObjects.Count;
            var arrayPool = ArrayPool<double>.Shared;

            double[] baseTimePoints = collectBaseTimePoints(hitObjects, noteCount, arrayPool);

            cornerData.BaseTimeCorners = buildBaseTimeCorners(baseTimePoints, arrayPool, maxTime);
            cornerData.AccuracyTimeCorners = buildAccuracyTimeCorners(baseTimePoints, arrayPool, maxTime);
            cornerData.TimeCorners = createTimeCornerUnion(cornerData.BaseTimeCorners, cornerData.AccuracyTimeCorners);

            return cornerData;
        }

        private static double[] collectBaseTimePoints(List<DifficultyHitObject> allNotes, int noteCount, ArrayPool<double> pool)
        {
            int estimatedCapacity = Math.Max(32, noteCount * 3);
            double[] timePoints = pool.Rent(estimatedCapacity);
            int timePointCount = 0;

            for (int i = 0; i < noteCount; i++)
            {
                var note = (ManiaDifficultyHitObject)allNotes[i];

                if (timePointCount >= timePoints.Length)
                    timePoints = expandArray(timePoints, ref timePointCount, pool);

                timePoints[timePointCount++] = note.StartTime;

                if (note.IsLong)
                {
                    if (timePointCount >= timePoints.Length)
                        timePoints = expandArray(timePoints, ref timePointCount, pool);
                    timePoints[timePointCount++] = note.EndTime;
                }
            }

            double[] result = new double[timePointCount];
            Array.Copy(timePoints, 0, result, 0, timePointCount);
            pool.Return(timePoints, clearArray: true);

            return result;
        }

        private static double[] buildBaseTimeCorners(double[] baseTimePoints, ArrayPool<double> pool, int maxTime)
        {
            int basePointCount = baseTimePoints.Length;
            int expandedCapacity = Math.Max(8, basePointCount * 4 + 4);
            double[] expandedPoints = pool.Rent(expandedCapacity);
            int expandedCount = 0;

            for (int i = 0; i < basePointCount; i++)
            {
                double baseTime = baseTimePoints[i];
                if (baseTime < 0 || baseTime > maxTime) continue;

                if (expandedCount + 4 >= expandedPoints.Length)
                    expandedPoints = expandArray(expandedPoints, ref expandedCount, pool);

                expandedPoints[expandedCount++] = baseTime;
                expandedPoints[expandedCount++] = baseTime + 501;
                expandedPoints[expandedCount++] = baseTime - 499;
                expandedPoints[expandedCount++] = baseTime + 1;
            }

            if (expandedCount + 2 >= expandedPoints.Length)
                expandedPoints = expandArray(expandedPoints, ref expandedCount, pool);
            expandedPoints[expandedCount++] = 0;
            expandedPoints[expandedCount++] = maxTime;

            double[] result = processAndSortTimePoints(expandedPoints, expandedCount, maxTime);
            pool.Return(expandedPoints, clearArray: true);

            return result;
        }

        private static double[] buildAccuracyTimeCorners(double[] baseTimePoints, ArrayPool<double> pool, int maxTime)
        {
            int basePointCount = baseTimePoints.Length;
            int accuracyCapacity = Math.Max(8, basePointCount * 3 + 2);
            double[] accuracyPoints = pool.Rent(accuracyCapacity);
            int accuracyCount = 0;

            if (accuracyCount >= accuracyPoints.Length)
                accuracyPoints = expandArray(accuracyPoints, ref accuracyCount, pool);
            accuracyPoints[accuracyCount++] = 0;

            if (accuracyCount >= accuracyPoints.Length)
                accuracyPoints = expandArray(accuracyPoints, ref accuracyCount, pool);
            accuracyPoints[accuracyCount++] = maxTime;

            for (int i = 0; i < basePointCount; i++)
            {
                double baseTime = baseTimePoints[i];
                double windowBefore = baseTime - 1000;
                double windowAfter = baseTime + 1000;

                if (windowBefore >= 0 && windowBefore <= maxTime)
                {
                    if (accuracyCount >= accuracyPoints.Length)
                        accuracyPoints = expandArray(accuracyPoints, ref accuracyCount, pool);
                    accuracyPoints[accuracyCount++] = windowBefore;
                }

                if (baseTime >= 0 && baseTime <= maxTime)
                {
                    if (accuracyCount >= accuracyPoints.Length)
                        accuracyPoints = expandArray(accuracyPoints, ref accuracyCount, pool);
                    accuracyPoints[accuracyCount++] = baseTime;
                }

                if (windowAfter >= 0 && windowAfter <= maxTime)
                {
                    if (accuracyCount >= accuracyPoints.Length)
                        accuracyPoints = expandArray(accuracyPoints, ref accuracyCount, pool);
                    accuracyPoints[accuracyCount++] = windowAfter;
                }
            }

            double[] result = processAndSortTimePoints(accuracyPoints, accuracyCount, maxTime);
            pool.Return(accuracyPoints, clearArray: true);

            return result;
        }

        private static double[] processAndSortTimePoints(double[] timePoints, int count, int maxTime)
        {
            Array.Sort(timePoints, 0, count);

            int writeIndex = 0;

            for (int readIndex = 0; readIndex < count; readIndex++)
            {
                double currentValue = timePoints[readIndex];
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

        private static double[] createTimeCornerUnion(double[] baseTimeCorners, double[] accuracyTimeCorners)
        {
            int baseLength = baseTimeCorners.Length;
            int accuracyLength = accuracyTimeCorners.Length;
            double[] unionArray = new double[baseLength + accuracyLength];

            int baseIndex = 0, accuracyIndex = 0, unionIndex = 0;

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

            double[] result = new double[unionIndex];
            Array.Copy(unionArray, 0, result, 0, unionIndex);
            return result;
        }

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
