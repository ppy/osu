// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Humanizer;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;

namespace osu.Game.Utils
{
    public static class FormatUtils
    {
        /// <summary>
        /// Turns the provided accuracy into a percentage with 2 decimal places.
        /// </summary>
        /// <param name="accuracy">The accuracy to be formatted.</param>
        /// <returns>formatted accuracy in percentage</returns>
        public static LocalisableString FormatAccuracy(this double accuracy)
        {
            // for the sake of display purposes, we don't want to show a user a "rounded up" percentage to the next whole number.
            // ie. a score which gets 89.99999% shouldn't ever show as 90%.
            // the reasoning for this is that cutoffs for grade increases are at whole numbers and displaying the required
            // percentile with a non-matching grade is confusing.
            accuracy = Math.Floor(accuracy * 10000) / 10000;

            return accuracy.ToLocalisableString("0.00%");
        }

        /// <summary>
        /// Formats the supplied rank/leaderboard position in a consistent, simplified way.
        /// </summary>
        /// <param name="rank">The rank/position to be formatted.</param>
        public static string FormatRank(this int rank) => rank.ToMetric(decimals: rank < 100_000 ? 1 : 0);

        /// <summary>
        /// Finds the number of digits after the decimal.
        /// </summary>
        /// <param name="d">The value to find the number of decimal digits for.</param>
        /// <returns>The number decimal digits.</returns>
        public static int FindPrecision(decimal d)
        {
            int precision = 0;

            while (d != Math.Round(d))
            {
                d *= 10;
                precision++;
            }

            return precision;
        }

        /// <summary>
        /// Applies rounding to the given BPM value.
        /// </summary>
        /// <remarks>
        /// Double-rounding is applied intentionally (see https://github.com/ppy/osu/pull/18345#issue-1243311382 for rationale).
        /// </remarks>
        /// <param name="baseBpm">The base BPM to round.</param>
        /// <param name="rate">Rate adjustment, if applicable.</param>
        public static int RoundBPM(double baseBpm, double rate = 1) => (int)Math.Round(Math.Round(baseBpm) * rate);

        /// <summary>
        /// Resampling strain values to certain bin size.
        /// </summary>
        /// <remarks>
        /// The main feature of this resampling is that peak strains will be always preserved.
        /// This means that the highest strain can't be decreased by averaging or interpolation.
        /// </remarks>
        public static double[] ResampleStrains(double[] values, int targetSize)
        {
            // Set to at least one value, what will be 0 in this case
            if (values.Length == 0)
                values = new double[1];

            if (targetSize > values.Length)
                return resamplingUpscale(values, targetSize);
            else if (targetSize < values.Length)
                return resamplingDownscale(values, targetSize);

            return (double[])values.Clone();
        }

        private static double[] resamplingUpscale(double[] values, int targetSize)
        {
            // Create array filled with -inf
            double[] result = Enumerable.Repeat(double.NegativeInfinity, targetSize).ToArray();

            // First and last peaks are constant
            result[0] = values[0];
            result[^1] = values[^1];

            // On the first pass we place peaks

            int sourceIndex = 1;
            int targetIndex = 1;

            // Adjust sizes accounting for the fact that first and last elements already set-up
            int sourceSize = Math.Max(1, values.Length - 1);
            targetSize -= 1;

            for (; targetIndex < targetSize - 1; targetIndex++)
            {
                double sourceProgress = (double)sourceIndex / sourceSize;

                double targetProgressNext = (targetIndex + 1.0) / targetSize;

                // If we reached the point where source is between current and next - then peak is either current or next
                if (sourceProgress <= targetProgressNext)
                {
                    double targetProgressCurrent = (double)targetIndex / targetSize;

                    double distanceToCurrent = sourceProgress - targetProgressCurrent;
                    double distanceToNext = targetProgressNext - sourceProgress;

                    // If it's next what is closer - abbadon current and move to next immediatly
                    if (distanceToNext < distanceToCurrent)
                    {
                        result[targetIndex] = double.NegativeInfinity;
                        targetIndex++;
                    }

                    result[targetIndex] = values[sourceIndex];
                    sourceIndex++;
                }
            }

            // On second pass we interpolate between peaks

            sourceIndex = 0;
            targetIndex = 1;

            for (; targetIndex < targetSize; targetIndex++)
            {
                // If we're on peak - skip iteration
                if (result[targetIndex] != double.NegativeInfinity)
                {
                    sourceIndex++;
                    continue;
                }

                double targetProgress = (double)targetIndex / targetSize;

                double previousPeakProgress = (double)sourceIndex / sourceSize;
                double nextPeakProgress = (sourceIndex + 1.0) / sourceSize;

                double distanceToPreviousPeak = targetProgress - previousPeakProgress;
                double distanceToNextPeak = nextPeakProgress - targetProgress;

                double lerpCoef = distanceToPreviousPeak / (distanceToPreviousPeak + distanceToNextPeak);
                double nextValue = sourceIndex + 1 < values.Length ? values[sourceIndex + 1] : values[sourceIndex];
                result[targetIndex] = double.Lerp(values[sourceIndex], nextValue, lerpCoef);
            }

            return result;
        }

        private static double[] resamplingDownscale(double[] values, int targetSize)
        {
            double[] result = new double[targetSize];

            int sourceIndex = 0;
            int targetIndex = 0;

            double currentSampleMax = double.NegativeInfinity;

            for (; sourceIndex < values.Length; sourceIndex++)
            {
                double currentValue = values[sourceIndex];

                double sourceProgress = (sourceIndex + 0.5) / values.Length;
                double targetProgressBorder = (targetIndex + 1.0) / targetSize;

                double distanceToBorder = targetProgressBorder - sourceProgress;

                // Handle transition to next sample
                if (distanceToBorder < 0)
                {
                    double targetProgressCurrent = (targetIndex + 0.5) / targetSize;
                    double targetProgressNext = (targetIndex + 1.5) / targetSize;

                    // Try fit weighted current into still current sample
                    // It would always be closer to Next than to Current
                    double weight = (targetProgressNext - sourceProgress) / (sourceProgress - targetProgressCurrent);
                    double weightedValue = currentValue * weight;

                    if (currentSampleMax < weightedValue) currentSampleMax = weightedValue;

                    // Flush current max
                    result[targetIndex] = currentSampleMax;
                    targetIndex++;
                    currentSampleMax = double.NegativeInfinity;

                    // Try to fit weighted previous into future sample
                    if (sourceIndex > 0)
                    {
                        double prevValue = values[sourceIndex - 1];
                        double sourceProgressPrev = (sourceIndex - 0.5) / values.Length;

                        // It would always be closer to Current than to Current
                        weight = (sourceProgressPrev - targetProgressCurrent) / (targetProgressNext - sourceProgressPrev);
                        weightedValue = prevValue * weight;

                        currentSampleMax = weightedValue;
                    }
                }

                // Replace with maximum of the sample
                if (currentSampleMax < currentValue) currentSampleMax = currentValue;
            }

            // Flush last value
            result[targetIndex] = currentSampleMax;

            return result;
        }
    }
}
