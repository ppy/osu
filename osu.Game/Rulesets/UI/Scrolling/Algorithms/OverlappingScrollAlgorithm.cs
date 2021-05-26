// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Lists;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.UI.Scrolling.Algorithms
{
    public class OverlappingScrollAlgorithm : IScrollAlgorithm
    {
        private readonly MultiplierControlPoint searchPoint;

        private readonly SortedList<MultiplierControlPoint> controlPoints;

        public OverlappingScrollAlgorithm(SortedList<MultiplierControlPoint> controlPoints)
        {
            this.controlPoints = controlPoints;

            searchPoint = new MultiplierControlPoint();
        }

        public double GetDisplayStartTime(double originTime, float offset, double timeRange, float scrollLength)
        {
            var controlPoint = controlPointAt(originTime);
            // The total amount of time that the hitobject will remain visible within the timeRange, which decreases as the speed multiplier increases
            double visibleDuration = (scrollLength + offset) * timeRange / controlPoint.Multiplier / scrollLength;
            return originTime - visibleDuration;
        }

        public float GetLength(double startTime, double endTime, double timeRange, float scrollLength)
        {
            // At the hitobject's end time, the hitobject will be positioned such that its end rests at the origin.
            // This results in a negative-position value, and the absolute of it indicates the length of the hitobject.
            return -PositionAt(startTime, endTime, timeRange, scrollLength);
        }

        public float PositionAt(double time, double currentTime, double timeRange, float scrollLength)
            => (float)((time - currentTime) / timeRange * controlPointAt(time).Multiplier * scrollLength);

        public double TimeAt(float position, double currentTime, double timeRange, float scrollLength)
        {
            // Find the control point relating to the position.
            // Note: Due to velocity adjustments, overlapping control points will provide multiple valid time values for a single position
            // As such, this operation provides unexpected results by using the latter of the control points.

            int i = 0;
            float pos = 0;

            for (; i < controlPoints.Count; i++)
            {
                float lastPos = pos;
                pos = PositionAt(controlPoints[i].StartTime, currentTime, timeRange, scrollLength);

                if (pos > position)
                {
                    i--;
                    pos = lastPos;
                    break;
                }
            }

            i = Math.Clamp(i, 0, controlPoints.Count - 1);

            return controlPoints[i].StartTime + (position - pos) * timeRange / controlPoints[i].Multiplier / scrollLength;
        }

        public void Reset()
        {
        }

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
