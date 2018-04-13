// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Lists;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Timing;
using OpenTK;

namespace osu.Game.Rulesets.UI.Scrolling.Visualisers
{
    public class OverlappingSpeedChangeVisualiser : ISpeedChangeVisualiser
    {
        private readonly SortedList<MultiplierControlPoint> controlPoints;

        public OverlappingSpeedChangeVisualiser(SortedList<MultiplierControlPoint> controlPoints)
        {
            this.controlPoints = controlPoints;
        }

        public void ComputeInitialStates(IEnumerable<DrawableHitObject> hitObjects, ScrollingDirection direction, double timeRange, Vector2 length)
        {
            foreach (var obj in hitObjects)
            {
                var controlPoint = controlPointAt(obj.HitObject.StartTime);
                obj.LifetimeStart = obj.HitObject.StartTime - timeRange / controlPoint.Multiplier;

                if (obj.HasNestedHitObjects)
                {
                    ComputeInitialStates(obj.NestedHitObjects, direction, timeRange, length);
                    ComputePositions(obj.NestedHitObjects, direction, obj.HitObject.StartTime, timeRange, length);
                }
            }
        }

        public void ComputePositions(IEnumerable<DrawableHitObject> hitObjects, ScrollingDirection direction, double currentTime, double timeRange, Vector2 length)
        {
            foreach (var obj in hitObjects)
            {
                var controlPoint = controlPointAt(obj.HitObject.StartTime);

                var position = (obj.HitObject.StartTime - currentTime) * controlPoint.Multiplier / timeRange;

                switch (direction)
                {
                    case ScrollingDirection.Up:
                        obj.Y = (float)(position * length.Y);
                        break;
                    case ScrollingDirection.Down:
                        obj.Y = (float)(-position * length.Y);
                        break;
                    case ScrollingDirection.Left:
                        obj.X = (float)(position * length.X);
                        break;
                    case ScrollingDirection.Right:
                        obj.X = (float)(-position * length.X);
                        break;
                }
            }
        }

        private readonly MultiplierControlPoint searchPoint = new MultiplierControlPoint();
        private MultiplierControlPoint controlPointAt(double time)
        {
            if (controlPoints.Count == 0)
                return new MultiplierControlPoint(double.NegativeInfinity);

            if (time < controlPoints[0].StartTime)
                return controlPoints[0];

            searchPoint.StartTime = time;
            int index = controlPoints.BinarySearch(searchPoint);

            if (index < 0)
                index = ~index - 1;

            return controlPoints[index];
        }
    }
}
