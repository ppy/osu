// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using JetBrains.Annotations;
using osu.Game.Rulesets.Timing;

namespace osu.Game.Rulesets.UI.Scrolling.Algorithms
{
    public class SequentialScrollAlgorithm : IScrollAlgorithm
    {
        private static readonly IComparer<PositionMapping> by_position_comparer = Comparer<PositionMapping>.Create((c1, c2) => c1.Position.CompareTo(c2.Position));

        private readonly IReadOnlyList<MultiplierControlPoint> controlPoints;

        /// <summary>
        /// Stores a mapping of time -> position for each control point.
        /// </summary>
        private readonly List<PositionMapping> positionMappings = new List<PositionMapping>();

        public SequentialScrollAlgorithm(IReadOnlyList<MultiplierControlPoint> controlPoints)
        {
            this.controlPoints = controlPoints;
        }

        public double GetDisplayStartTime(double originTime, float offset, double timeRange, float scrollLength)
        {
            return TimeAt(-(scrollLength + offset), originTime, timeRange, scrollLength);
        }

        public float GetLength(double startTime, double endTime, double timeRange, float scrollLength)
        {
            double objectLength = relativePositionAt(endTime, timeRange) - relativePositionAt(startTime, timeRange);
            return (float)(objectLength * scrollLength);
        }

        public float PositionAt(double time, double currentTime, double timeRange, float scrollLength)
        {
            double timelineLength = relativePositionAt(time, timeRange) - relativePositionAt(currentTime, timeRange);
            return (float)(timelineLength * scrollLength);
        }

        public double TimeAt(float position, double currentTime, double timeRange, float scrollLength)
        {
            if (controlPoints.Count == 0)
                return position * timeRange;

            // Find the position at the current time, and the given length.
            double relativePosition = relativePositionAt(currentTime, timeRange) + position / scrollLength;

            var positionMapping = findControlPointMapping(timeRange, new PositionMapping(0, null, relativePosition), by_position_comparer);

            // Begin at the control point's time and add the remaining time to reach the given position.
            return positionMapping.Time + (relativePosition - positionMapping.Position) * timeRange / positionMapping.ControlPoint.Multiplier;
        }

        public void Reset() => positionMappings.Clear();

        /// <summary>
        /// Finds the position which corresponds to a point in time.
        /// This is a non-linear operation that depends on all the control points up to and including the one active at the time value.
        /// </summary>
        /// <param name="time">The time to find the position at.</param>
        /// <param name="timeRange">The amount of time visualised by the scrolling area.</param>
        /// <returns>A positive value indicating the position at <paramref name="time"/>.</returns>
        private double relativePositionAt(in double time, in double timeRange)
        {
            if (controlPoints.Count == 0)
                return time / timeRange;

            var mapping = findControlPointMapping(timeRange, new PositionMapping(time));

            // Begin at the control point's position and add the remaining distance to reach the given time.
            return mapping.Position + (time - mapping.Time) / timeRange * mapping.ControlPoint.Multiplier;
        }

        /// <summary>
        /// Finds a <see cref="MultiplierControlPoint"/>'s <see cref="PositionMapping"/> that is relevant to a given <see cref="PositionMapping"/>.
        /// </summary>
        /// <remarks>
        /// This is used to find the last <see cref="MultiplierControlPoint"/> occuring prior to a time value, or prior to a position value (if <see cref="by_position_comparer"/> is used).
        /// </remarks>
        /// <param name="timeRange">The time range.</param>
        /// <param name="search">The <see cref="PositionMapping"/> to find the closest <see cref="PositionMapping"/> to.</param>
        /// <param name="comparer">The comparison. If null, the default comparer is used (by time).</param>
        /// <returns>The <see cref="MultiplierControlPoint"/>'s <see cref="PositionMapping"/> that is relevant for <paramref name="search"/>.</returns>
        private PositionMapping findControlPointMapping(in double timeRange, in PositionMapping search, IComparer<PositionMapping> comparer = null)
        {
            generatePositionMappings(timeRange);

            int mappingIndex = positionMappings.BinarySearch(search, comparer ?? Comparer<PositionMapping>.Default);

            if (mappingIndex < 0)
            {
                // If the search value isn't found, the _next_ control point is returned, but we actually want the _previous_ control point.
                // In doing so, we must make sure to not underflow the position mapping list (i.e. always use the 0th control point for time < first_control_point_time).
                mappingIndex = Math.Max(0, ~mappingIndex - 1);

                Debug.Assert(mappingIndex < positionMappings.Count);
            }

            var mapping = positionMappings[mappingIndex];
            Debug.Assert(mapping.ControlPoint != null);

            return mapping;
        }

        /// <summary>
        /// Generates the mapping of <see cref="MultiplierControlPoint"/> (and their respective start times) to their relative position from 0.
        /// </summary>
        /// <param name="timeRange">The time range.</param>
        private void generatePositionMappings(in double timeRange)
        {
            if (positionMappings.Count > 0)
                return;

            if (controlPoints.Count == 0)
                return;

            positionMappings.Add(new PositionMapping(controlPoints[0].StartTime, controlPoints[0]));

            for (int i = 0; i < controlPoints.Count - 1; i++)
            {
                var current = controlPoints[i];
                var next = controlPoints[i + 1];

                // Figure out how much of the time range the duration represents, and adjust it by the speed multiplier
                float length = (float)((next.StartTime - current.StartTime) / timeRange * current.Multiplier);

                positionMappings.Add(new PositionMapping(next.StartTime, next, positionMappings[^1].Position + length));
            }
        }

        private readonly struct PositionMapping : IComparable<PositionMapping>
        {
            /// <summary>
            /// The time corresponding to this position.
            /// </summary>
            public readonly double Time;

            /// <summary>
            /// The <see cref="MultiplierControlPoint"/> at <see cref="Time"/>.
            /// </summary>
            [CanBeNull]
            public readonly MultiplierControlPoint ControlPoint;

            /// <summary>
            /// The relative position from 0 of <see cref="ControlPoint"/>.
            /// </summary>
            public readonly double Position;

            public PositionMapping(double time, MultiplierControlPoint controlPoint = null, double position = default)
            {
                Time = time;
                ControlPoint = controlPoint;
                Position = position;
            }

            public int CompareTo(PositionMapping other) => Time.CompareTo(other.Time);
        }
    }
}
