// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Lists;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.UI.Scrolling.Visualisers
{
    public readonly struct OverlappingSpeedChangeVisualiser : ISpeedChangeVisualiser
    {
        private readonly MultiplierControlPoint searchPoint;

        private readonly SortedList<MultiplierControlPoint> controlPoints;
        private readonly double timeRange;
        private readonly float scrollLength;

        public OverlappingSpeedChangeVisualiser(SortedList<MultiplierControlPoint> controlPoints, double timeRange, float scrollLength)
        {
            this.controlPoints = controlPoints;
            this.timeRange = timeRange;
            this.scrollLength = scrollLength;

            searchPoint = new MultiplierControlPoint();
        }

        public double GetDisplayStartTime(double time)
        {
            // The total amount of time that the hitobject will remain visible within the timeRange, which decreases as the speed multiplier increases
            double visibleDuration = timeRange / controlPointAt(time).Multiplier;
            return time - visibleDuration;
        }

        public float GetLength(double startTime, double endTime)
        {
            // At the hitobject's end time, the hitobject will be positioned such that its end rests at the origin.
            // This results in a negative-position value, and the absolute of it indicates the length of the hitobject.
            return -PositionAt(startTime, endTime);
        }

        public float PositionAt(double time, double currentTime)
            => (float)((time - currentTime) / timeRange * controlPointAt(time).Multiplier * scrollLength);

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
