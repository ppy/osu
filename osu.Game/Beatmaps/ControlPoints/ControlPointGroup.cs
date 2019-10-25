// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class ControlPointGroup : IComparable<ControlPointGroup>
    {
        public event Action<ControlPoint> ItemAdded;
        public event Action<ControlPoint> ItemRemoved;

        /// <summary>
        /// The time at which the control point takes effect.
        /// </summary>
        public double Time { get; }

        public IReadOnlyList<ControlPoint> ControlPoints => controlPoints;

        private readonly List<ControlPoint> controlPoints = new List<ControlPoint>();

        public ControlPointGroup(double time)
        {
            Time = time;
        }

        public int CompareTo(ControlPointGroup other) => Time.CompareTo(other.Time);

        public void Add(ControlPoint point)
        {
            point.AttachGroup(this);

            foreach (var existing in controlPoints.Where(p => p.GetType() == point.GetType()).ToArray())
                Remove(existing);

            controlPoints.Add(point);
            ItemAdded?.Invoke(point);
        }

        public void Remove(ControlPoint point)
        {
            controlPoints.Remove(point);
            ItemRemoved?.Invoke(point);
        }
    }
}
