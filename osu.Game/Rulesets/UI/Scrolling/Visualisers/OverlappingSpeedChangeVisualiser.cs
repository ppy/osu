// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Framework.Lists;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Objects.Types;
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
                // The total amount of time that the hitobject will remain visible within the timeRange, which decreases as the speed multiplier increases
                double visibleDuration = timeRange / controlPointAt(obj.HitObject.StartTime).Multiplier;

                obj.LifetimeStart = obj.HitObject.StartTime - visibleDuration;

                if (obj.HitObject is IHasEndTime endTime)
                {
                    // At the hitobject's end time, the hitobject will be positioned such that its end rests at the origin.
                    // This results in a negative-position value, and the absolute of it indicates the length of the hitobject.
                    var hitObjectLength = -hitObjectPositionAt(obj, endTime.EndTime, timeRange);

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
            foreach (var obj in hitObjects)
            {
                var position = hitObjectPositionAt(obj, currentTime, timeRange);

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

        /// <summary>
        /// Computes the position of a <see cref="DrawableHitObject"/> at a point in time.
        /// <para>
        /// At t &lt; startTime, position &gt; 0. <br />
        /// At t = startTime, position = 0. <br />
        /// At t &gt; startTime, position &lt; 0.
        /// </para>
        /// </summary>
        /// <param name="obj">The <see cref="DrawableHitObject"/>.</param>
        /// <param name="time">The time to find the position of <paramref name="obj"/> at.</param>
        /// <param name="timeRange">The amount of time visualised by the scrolling area.</param>
        /// <returns>The position of <paramref name="obj"/> in the scrolling area at time = <paramref name="time"/>.</returns>
        private double hitObjectPositionAt(DrawableHitObject obj, double time, double timeRange)
            => (obj.HitObject.StartTime - time) / timeRange * controlPointAt(obj.HitObject.StartTime).Multiplier;

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
