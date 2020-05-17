// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.UI.Scrolling.Algorithms
{
    public class SequentialScrollAlgorithm : IScrollAlgorithm
    {
        private readonly Dictionary<double, double> positionCache;

        private readonly IReadOnlyList<MultiplierControlPoint> controlPoints;

        public SequentialScrollAlgorithm(IReadOnlyList<MultiplierControlPoint> controlPoints)
        {
            this.controlPoints = controlPoints;

            positionCache = new Dictionary<double, double>();
        }

        public double GetDisplayStartTime(double originTime, float offset, double timeRange, float scrollLength)
        {
            double adjustedTime = TimeAt(-offset, originTime, timeRange, scrollLength);
            return adjustedTime - timeRange - 1000;
        }

        public float GetLength(double startTime, double endTime, double timeRange, float scrollLength)
        {
            var objectLength = relativePositionAtCached(endTime, timeRange) - relativePositionAtCached(startTime, timeRange);
            return (float)(objectLength * scrollLength);
        }

        public float PositionAt(double time, double currentTime, double timeRange, float scrollLength)
        {
            // Caching is not used here as currentTime is unlikely to have been previously cached
            double timelinePosition = relativePositionAt(currentTime, timeRange);
            return (float)((relativePositionAtCached(time, timeRange) - timelinePosition) * scrollLength);
        }

        public double TimeAt(float position, double currentTime, double timeRange, float scrollLength)
        {
            // Convert the position to a length relative to time = 0
            double length = position / scrollLength + relativePositionAt(currentTime, timeRange);

            // We need to consider all timing points until the specified time and not just the currently-active one,
            // since each timing point individually affects the positions of _all_ hitobjects after its start time
            for (int i = 0; i < controlPoints.Count; i++)
            {
                var current = controlPoints[i];
                var next = i < controlPoints.Count - 1 ? controlPoints[i + 1] : null;

                // Duration of the current control point
                var currentDuration = (next?.StartTime ?? double.PositiveInfinity) - current.StartTime;

                // Figure out the length of control point
                var currentLength = currentDuration / timeRange * current.Multiplier;

                if (currentLength > length)
                {
                    // The point is within this control point
                    return current.StartTime + length * timeRange / current.Multiplier;
                }

                length -= currentLength;
            }

            return 0; // Should never occur
        }

        private double relativePositionAtCached(double time, double timeRange)
        {
            if (!positionCache.TryGetValue(time, out double existing))
                positionCache[time] = existing = relativePositionAt(time, timeRange);
            return existing;
        }

        public void Reset() => positionCache.Clear();

        /// <summary>
        /// Finds the position which corresponds to a point in time.
        /// This is a non-linear operation that depends on all the control points up to and including the one active at the time value.
        /// </summary>
        /// <param name="time">The time to find the position at.</param>
        /// <param name="timeRange">The amount of time visualised by the scrolling area.</param>
        /// <returns>A positive value indicating the position at <paramref name="time"/>.</returns>
        private double relativePositionAt(double time, double timeRange)
        {
            if (controlPoints.Count == 0)
                return time / timeRange;

            double length = 0;

            // We need to consider all timing points until the specified time and not just the currently-active one,
            // since each timing point individually affects the positions of _all_ hitobjects after its start time
            for (int i = 0; i < controlPoints.Count; i++)
            {
                var current = controlPoints[i];
                var next = i < controlPoints.Count - 1 ? controlPoints[i + 1] : null;

                // We don't need to consider any control points beyond the current time, since it will not yet
                // affect any hitobjects
                if (i > 0 && current.StartTime > time)
                    continue;

                // Duration of the current control point
                var currentDuration = (next?.StartTime ?? double.PositiveInfinity) - current.StartTime;

                // We want to consider the minimal amount of time that this control point has affected,
                // which may be either its duration, or the amount of time that has passed within it
                var durationInCurrent = Math.Min(currentDuration, time - current.StartTime);

                // Figure out how much of the time range the duration represents, and adjust it by the speed multiplier
                length += durationInCurrent / timeRange * current.Multiplier;
            }

            return length;
        }
    }
}
