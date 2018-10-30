// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.UI.Scrolling.Visualisers
{
    public class SequentialSpeedChangeVisualiser : ISpeedChangeVisualiser
    {
        public double TimeRange { get; set; }

        public float ScrollLength { get; set; }

        private readonly Dictionary<double, double> positionCache = new Dictionary<double, double>();

        private readonly IReadOnlyList<MultiplierControlPoint> controlPoints;

        public SequentialSpeedChangeVisualiser(IReadOnlyList<MultiplierControlPoint> controlPoints)
        {
            this.controlPoints = controlPoints;
        }

        public void ComputeInitialStates(IEnumerable<DrawableHitObject> hitObjects, ScrollingDirection direction)
        {
            foreach (var obj in hitObjects)
            {
                obj.LifetimeStart = GetDisplayStartTime(obj.HitObject.StartTime);

                if (obj.HitObject is IHasEndTime endTime)
                {
                    switch (direction)
                    {
                        case ScrollingDirection.Up:
                        case ScrollingDirection.Down:
                            obj.Height = GetLength(obj.HitObject.StartTime, endTime.EndTime);
                            break;
                        case ScrollingDirection.Left:
                        case ScrollingDirection.Right:
                            obj.Width = GetLength(obj.HitObject.StartTime, endTime.EndTime);
                            break;
                    }
                }

                ComputeInitialStates(obj.NestedHitObjects, direction);

                // Nested hitobjects don't need to scroll, but they do need accurate positions
                UpdatePositions(obj.NestedHitObjects, direction, obj.HitObject.StartTime);
            }
        }

        public void UpdatePositions(IEnumerable<DrawableHitObject> hitObjects, ScrollingDirection direction, double currentTime)
        {
            foreach (var obj in hitObjects)
            {
                switch (direction)
                {
                    case ScrollingDirection.Up:
                        obj.Y = PositionAt(currentTime, obj.HitObject.StartTime);
                        break;
                    case ScrollingDirection.Down:
                        obj.Y = -PositionAt(currentTime, obj.HitObject.StartTime);
                        break;
                    case ScrollingDirection.Left:
                        obj.X = PositionAt(currentTime, obj.HitObject.StartTime);
                        break;
                    case ScrollingDirection.Right:
                        obj.X = -PositionAt(currentTime, obj.HitObject.StartTime);
                        break;
                }
            }
        }

        public double GetDisplayStartTime(double startTime) => startTime - TimeRange - 1000;

        public float GetLength(double startTime, double endTime)
        {
            var objectLength = relativePositionAtCached(endTime) - relativePositionAtCached(startTime);
            return (float)(objectLength * ScrollLength);
        }

        public float PositionAt(double currentTime, double startTime)
        {
            // Caching is not used here as currentTime is unlikely to have been previously cached
            double timelinePosition = relativePositionAt(currentTime);
            return (float)((relativePositionAtCached(startTime) - timelinePosition) * ScrollLength);
        }

        private double relativePositionAtCached(double time)
        {
            if (!positionCache.TryGetValue(time, out double existing))
                positionCache[time] = existing = relativePositionAt(time);
            return existing;
        }

        /// <summary>
        /// Finds the position which corresponds to a point in time.
        /// This is a non-linear operation that depends on all the control points up to and including the one active at the time value.
        /// </summary>
        /// <param name="time">The time to find the position at.</param>
        /// <param name="timeRange">The amount of time visualised by the scrolling area.</param>
        /// <returns>A positive value indicating the position at <paramref name="time"/>.</returns>
        private double relativePositionAt(double time)
        {
            if (controlPoints.Count == 0)
                return time / TimeRange;

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
                length += durationInCurrent / TimeRange * current.Multiplier;
            }

            return length;
        }
    }
}
