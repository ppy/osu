// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Framework.Bindables;

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

        public IBindableList<ControlPoint> ControlPoints => controlPoints;

        private readonly BindableList<ControlPoint> controlPoints = new BindableList<ControlPoint>();

        public ControlPointGroup(double time)
        {
            Time = time;
        }

        public int CompareTo(ControlPointGroup other) => Time.CompareTo(other.Time);

        public void Add(ControlPoint point)
        {
            var existing = controlPoints.FirstOrDefault(p => p.GetType() == point.GetType());

            if (existing != null)
                Remove(existing);

            point.AttachGroup(this);

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
