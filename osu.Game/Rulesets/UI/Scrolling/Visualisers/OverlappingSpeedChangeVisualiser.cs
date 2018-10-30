// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Lists;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.UI.Scrolling.Visualisers
{
    public class OverlappingSpeedChangeVisualiser : ISpeedChangeVisualiser
    {
        public double TimeRange { get; set; }

        public float ScrollLength { get; set; }

        private readonly SortedList<MultiplierControlPoint> controlPoints;

        public OverlappingSpeedChangeVisualiser(SortedList<MultiplierControlPoint> controlPoints)
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

        public double GetDisplayStartTime(double startTime)
        {
            // The total amount of time that the hitobject will remain visible within the timeRange, which decreases as the speed multiplier increases
            double visibleDuration = TimeRange / controlPointAt(startTime).Multiplier;
            return startTime - visibleDuration;
        }

        public float GetLength(double startTime, double endTime)
        {
            // At the hitobject's end time, the hitobject will be positioned such that its end rests at the origin.
            // This results in a negative-position value, and the absolute of it indicates the length of the hitobject.
            return -PositionAt(endTime, startTime);
        }

        public float PositionAt(double currentTime, double startTime)
            => (float)((startTime - currentTime) / TimeRange * controlPointAt(startTime).Multiplier * ScrollLength);

        private readonly MultiplierControlPoint searchPoint = new MultiplierControlPoint();

        /// <summary>
        /// Finds the <see cref="MultiplierControlPoint"/> which affects the speed of hitobjects at a specific time.
        /// </summary>
        /// <param name="time">The time which the <see cref="MultiplierControlPoint"/> should affect.</param>
        /// <returns>The <see cref="MultiplierControlPoint"/>.</returns>
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
