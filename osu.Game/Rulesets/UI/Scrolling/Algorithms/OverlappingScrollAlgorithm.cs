// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Diagnostics;
using System.Linq;
using osu.Framework.Lists;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.UI.Scrolling.Algorithms
{
    public class OverlappingScrollAlgorithm : IScrollAlgorithm
    {
        private readonly SortedList<MultiplierControlPoint> controlPoints;

        public OverlappingScrollAlgorithm(SortedList<MultiplierControlPoint> controlPoints)
        {
            this.controlPoints = controlPoints;
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

        public float PositionAt(double time, double currentTime, double timeRange, float scrollLength, double? originTime = null)
            => (float)((time - currentTime) / timeRange * controlPointAt(originTime ?? time).Multiplier * scrollLength);

        public double TimeAt(float position, double currentTime, double timeRange, float scrollLength)
        {
            Debug.Assert(controlPoints.Count > 0);

            // Iterate over control points and find the most relevant for the provided position.
            // Note: Due to velocity adjustments, overlapping control points will provide multiple valid time values for a single position
            // As such, this operation provides unexpected results by using the latter of the control points.
            var relevantControlPoint = controlPoints.LastOrDefault(cp => PositionAt(cp.Time, currentTime, timeRange, scrollLength) <= position) ?? controlPoints.First();

            float positionAtControlPoint = PositionAt(relevantControlPoint.Time, currentTime, timeRange, scrollLength);

            return relevantControlPoint.Time + (position - positionAtControlPoint) * timeRange / relevantControlPoint.Multiplier / scrollLength;
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
            return ControlPointInfo.BinarySearch(controlPoints, time)
                   // The standard binary search will fail if there's no control points, or if the time is before the first.
                   // For this method, we want to use the first control point in the latter case.
                   ?? controlPoints.FirstOrDefault()
                   ?? new MultiplierControlPoint(double.NegativeInfinity);
        }
    }
}
