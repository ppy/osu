// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Screens.Play.HUD
{
    public partial class DefaultSongProgressGraph : SquareGraph
    {
        private const int granularity = 200;

        public void SetFromObjects(IEnumerable<HitObject> objects)
        {
            Values = new float[granularity];

            if (!objects.Any())
                return;

            (double firstHit, double lastHit) = BeatmapExtensions.CalculatePlayableBounds(objects);

            if (lastHit == 0)
                lastHit = objects.Last().StartTime;

            double interval = (lastHit - firstHit + 1) / granularity;

            foreach (var h in objects)
            {
                double endTime = h.GetEndTime();

                Debug.Assert(endTime >= h.StartTime);

                int startRange = (int)((h.StartTime - firstHit) / interval);
                int endRange = (int)((endTime - firstHit) / interval);
                for (int i = startRange; i <= endRange; i++)
                    Values[i]++;
            }
        }

        public void SetFromStrains(double[] strains)
        {
            // For some reason it has 1 column delay, account for this by skipping first value
            Values = resampling(strains, granularity).Select(value => (float)value).ToArray();
        }

        private static double[] resampling(double[] values, int targetSize)
        {
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
            int sourceSize = values.Length - 1;
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
                result[targetIndex] = double.Lerp(values[sourceIndex], values[sourceIndex + 1], lerpCoef);
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
