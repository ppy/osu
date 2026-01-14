// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;

namespace osu.Game.Rulesets.Mania.Difficulty.Utils
{
    public static class StrainArrayUtils
    {
        /// <summary>
        /// Applies smoothing to an array using a sliding window approach with cumulative sum optimization.
        /// This creates more stable difficulty curves by averaging values within a time window.
        /// </summary>
        public static double[] ApplySmoothingToArray(double[] timePoints, double[] values, double windowSizeMs, double scale, string mode)
        {
            int pointCount = timePoints.Length;
            if (pointCount == 0) return Array.Empty<double>();

            var arrayPool = ArrayPool<double>.Shared;
            double[] cumulativeSum = arrayPool.Rent(Math.Max(1, pointCount));

            // Build cumulative sum for efficient window queries
            BuildCumulativeSum(cumulativeSum, timePoints, values, 0, pointCount);

            double[] smoothedResults = new double[pointCount];
            int lastStartIndex = 0, lastEndIndex = 0;

            // Apply smoothing to each point using sliding window
            for (int pointIndex = 0; pointIndex < pointCount; pointIndex++)
            {
                double centerTime = timePoints[pointIndex];
                double windowStart = Math.Max(centerTime - windowSizeMs, timePoints[0]);
                double windowEnd = Math.Min(centerTime + windowSizeMs, timePoints[pointCount - 1]);

                // Calculate sum within the window using progressive queries
                double windowSum = QueryCumulativeSumProgressive(windowEnd, timePoints, cumulativeSum, values, 0, ref lastEndIndex) - QueryCumulativeSumProgressive(windowStart, timePoints, cumulativeSum, values, 0, ref lastStartIndex);

                // Apply the appropriate smoothing mode
                smoothedResults[pointIndex] = applySmoothingMode(windowSum, windowStart, windowEnd, scale, mode);
            }

            arrayPool.Return(cumulativeSum, clearArray: true);

            return smoothedResults;
        }

        /// <summary>
        /// Builds a cumulative sum array for efficient range query operations.
        /// This is the consolidated version that can handle both regular arrays and offset-based arrays.
        /// </summary>
        /// <param name="cumulativeSum">Output array for cumulative sums</param>
        /// <param name="timePoints">Time coordinates</param>
        /// <param name="values">Values to accumulate</param>
        /// <param name="valuesOffset">Offset into values array (for column-based data)</param>
        /// <param name="pointCount">Number of points to process</param>
        public static void BuildCumulativeSum(double[] cumulativeSum, double[] timePoints, double[] values, int valuesOffset, int pointCount)
        {
            cumulativeSum[0] = 0.0;

            for (int i = 1; i < pointCount; i++)
            {
                double timeSpan = timePoints[i] - timePoints[i - 1];
                double valueContribution = values[valuesOffset + i - 1] * timeSpan;
                cumulativeSum[i] = cumulativeSum[i - 1] + valueContribution;
            }
        }

        /// <summary>
        /// Applies smoothing to a single column's intensity values using the consolidated cumulative sum logic.
        /// This replaces the duplicate logic that was in SameColumnEvaluator.
        /// </summary>
        public static void ApplySmoothingToColumn(double[] intensityBuffer, double[] smoothedBuffer, double[] baseTimeCorners, int columnOffset, int timePointCount, double smoothingWindow)
        {
            var arrayPool = ArrayPool<double>.Shared;
            double[] cumulativeSum = arrayPool.Rent(timePointCount);

            try
            {
                // Build cumulative sum using the consolidated function
                BuildCumulativeSum(cumulativeSum, baseTimeCorners, intensityBuffer, columnOffset, timePointCount);

                int lastStartIndex = 0, lastEndIndex = 0;

                // Apply smoothing using sliding window
                for (int i = 0; i < timePointCount; i++)
                {
                    double centerTime = baseTimeCorners[i];
                    double windowStart = Math.Max(centerTime - smoothingWindow, baseTimeCorners[0]);
                    double windowEnd = Math.Min(centerTime + smoothingWindow, baseTimeCorners[timePointCount - 1]);

                    // Use progressive search for better performance
                    double windowSum = QueryCumulativeSumProgressive(windowEnd, baseTimeCorners, cumulativeSum, intensityBuffer, columnOffset, ref lastEndIndex) - QueryCumulativeSumProgressive(windowStart, baseTimeCorners, cumulativeSum, intensityBuffer, columnOffset, ref lastStartIndex);

                    smoothedBuffer[columnOffset + i] = 0.001 * windowSum;
                }
            }
            finally
            {
                arrayPool.Return(cumulativeSum, clearArray: true);
            }
        }

        /// <summary>
        /// Applies the specified smoothing mode to the calculated window sum.
        /// </summary>
        private static double applySmoothingMode(double windowSum, double windowStart, double windowEnd, double scale, string mode)
        {
            if (mode == "avg")
            {
                double windowLength = windowEnd - windowStart;
                return windowLength > 0.0 ? windowSum / windowLength : 0.0;
            }

            return scale * windowSum;
        }

        /// <summary>
        /// Efficiently queries a cumulative sum at a specific point using progressive search.
        /// This maintains search state to avoid redundant binary searches.
        /// </summary>
        /// <param name="queryPoint">The time point to query</param>
        /// <param name="timeArray">Sorted array of time coordinates</param>
        /// <param name="cumulativeSum">Precomputed cumulative sum array</param>
        /// <param name="values">Original values array</param>
        /// <param name="valuesOffset">Starting index offset into the values array (used for column-based data stored in flat arrays)</param>
        /// <param name="lastIndex">Search state - index of last query (modified by reference)</param>
        public static double QueryCumulativeSumProgressive(double queryPoint, double[] timeArray, double[] cumulativeSum, double[] values, int valuesOffset, ref int lastIndex)
        {
            if (queryPoint <= timeArray[0]) return 0.0;

            int lastArrayIndex = timeArray.Length - 1;
            if (queryPoint >= timeArray[lastArrayIndex]) return cumulativeSum[lastArrayIndex];

            // Progressive search from last position
            int currentIndex = Math.Max(0, lastIndex);
            while (currentIndex < lastArrayIndex && timeArray[currentIndex] <= queryPoint)
                currentIndex++;
            lastIndex = currentIndex;

            if (timeArray[currentIndex] == queryPoint) return cumulativeSum[currentIndex];

            // Interpolate using offset-based value lookup
            int leftIndex = currentIndex - 1;
            if (leftIndex < 0) return 0.0;

            double timeDifference = queryPoint - timeArray[leftIndex];
            double valueContribution = values[valuesOffset + leftIndex] * timeDifference;

            return cumulativeSum[leftIndex] + valueContribution;
        }

        /// <summary>
        /// Finds the leftmost position where a value can be inserted to maintain sorted order.
        /// Uses binary search for O(log n) performance.
        /// </summary>
        public static int FindLeftBound(double[] sortedArray, double targetValue)
        {
            int left = 0, right = sortedArray.Length;

            while (left < right)
            {
                int mid = (left + right) >> 1; // Bit shift for faster division by 2
                if (sortedArray[mid] < targetValue)
                    left = mid + 1;
                else
                    right = mid;
            }

            return left;
        }

        /// <summary>
        /// Progressive version of FindLeftBound that maintains search state for better performance
        /// when making sequential queries. This is particularly efficient for time-series data
        /// where queries tend to be in chronological order.
        /// </summary>
        public static int FindLeftBoundProgressive(double[] sortedArray, ref int lastIndex, double targetValue)
        {
            int arrayLength = sortedArray.Length;
            if (arrayLength == 0) return 0;

            // Start from the last known position
            int currentIndex = Math.Min(lastIndex, arrayLength - 1);

            // Move forward until we find the insertion point
            while (currentIndex < arrayLength && sortedArray[currentIndex] < targetValue)
                currentIndex++;

            lastIndex = currentIndex;
            return currentIndex;
        }
    }
}
