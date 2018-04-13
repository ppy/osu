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
                var startPosition = hitObjectPositions[obj] = positionAt(obj.HitObject.StartTime, timeRange);

                obj.LifetimeStart = obj.HitObject.StartTime - timeRange - 1000;

                if (obj.HitObject is IHasEndTime endTime)
                {
                    var diff = positionAt(endTime.EndTime, timeRange) - startPosition;

                    switch (direction)
                    {
                        case ScrollingDirection.Up:
                        case ScrollingDirection.Down:
                            obj.Height = (float)(diff * length.Y);
                            break;
                        case ScrollingDirection.Left:
                        case ScrollingDirection.Right:
                            obj.Width = (float)(diff * length.X);
                            break;
                    }
                }

                if (obj.HasNestedHitObjects)
                {
                    ComputeInitialStates(obj.NestedHitObjects, direction, timeRange, length);
                    ComputePositions(obj.NestedHitObjects, direction, obj.HitObject.StartTime, timeRange, length);
                }
            }
        }

        public void ComputePositions(IEnumerable<DrawableHitObject> hitObjects, ScrollingDirection direction, double currentTime, double timeRange, Vector2 length)
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

        private double positionAt(double time, double timeRange)
        {
            double length = 0;
            for (int i = 0; i < controlPoints.Count; i++)
            {
                var current = controlPoints[i];
                var next = i < controlPoints.Count - 1 ? controlPoints[i + 1] : null;

                if (i > 0 && current.StartTime > time)
                    continue;

                // Duration of the current control point
                var currentDuration = (next?.StartTime ?? double.PositiveInfinity) - current.StartTime;

                length += Math.Min(currentDuration, time - current.StartTime) * current.Multiplier / timeRange;
            }

            return length;
        }
    }
}
