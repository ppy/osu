// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Timing;
using OpenTK;

namespace osu.Game.Rulesets.UI.Scrolling.Visualisers
{
    public class SequentialSpeedChangeVisualiser : ISpeedChangeVisualiser
    {
        private readonly Dictionary<DrawableHitObject, double> hitObjectPositions = new Dictionary<DrawableHitObject, double>();

        private readonly IReadOnlyList<MultiplierControlPoint> controlPoints;

        public SequentialSpeedChangeVisualiser(IReadOnlyList<MultiplierControlPoint> controlPoints)
        {
            this.controlPoints = controlPoints;
        }

        public void ComputeInitialStates(IEnumerable<DrawableHitObject> hitObjects, ScrollingDirection direction, double timeRange, Vector2 length)
        {
            foreach (var obj in hitObjects)
            {
                // To reduce iterations when updating hitobject positions later on, their initial positions are cached
                var startPosition = hitObjectPositions[obj] = positionAt(obj.HitObject.StartTime, timeRange);

                // Todo: This is approximate and will be incorrect in the case of extreme speed changes
                obj.LifetimeStart = obj.HitObject.StartTime - timeRange - 1000;

                if (obj.HitObject is IHasEndTime endTime)
                {
                    var hitObjectLength = positionAt(endTime.EndTime, timeRange) - startPosition;

                    switch (direction)
                    {
                        case ScrollingDirection.Up:
                        case ScrollingDirection.Down:
                            obj.Height = (float)(hitObjectLength * length.Y);
                            break;
                        case ScrollingDirection.Left:
                        case ScrollingDirection.Right:
                            obj.Width = (float)(hitObjectLength * length.X);
                            break;
                    }
                }

                ComputeInitialStates(obj.NestedHitObjects, direction, timeRange, length);

                // Nested hitobjects don't need to scroll, but they do need accurate positions
                UpdatePositions(obj.NestedHitObjects, direction, obj.HitObject.StartTime, timeRange, length);
            }
        }

        public void UpdatePositions(IEnumerable<DrawableHitObject> hitObjects, ScrollingDirection direction, double currentTime, double timeRange, Vector2 length)
        {
            var timelinePosition = positionAt(currentTime, timeRange);

            foreach (var obj in hitObjects)
            {
                var finalPosition = hitObjectPositions[obj] - timelinePosition;

                switch (direction)
                {
                    case ScrollingDirection.Up:
                        obj.Y = (float)(finalPosition * length.Y);
                        break;
                    case ScrollingDirection.Down:
                        obj.Y = (float)(-finalPosition * length.Y);
                        break;
                    case ScrollingDirection.Left:
                        obj.X = (float)(finalPosition * length.X);
                        break;
                    case ScrollingDirection.Right:
                        obj.X = (float)(-finalPosition * length.X);
                        break;
                }
            }
        }

        /// <summary>
        /// Finds the position which corresponds to a point in time.
        /// This is a non-linear operation that depends on all the control points up to and including the one active at the time value.
        /// </summary>
        /// <param name="time">The time to find the position at.</param>
        /// <param name="timeRange">The amount of time visualised by the scrolling area.</param>
        /// <returns>A positive value indicating the position at <paramref name="time"/>.</returns>
        private double positionAt(double time, double timeRange)
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
