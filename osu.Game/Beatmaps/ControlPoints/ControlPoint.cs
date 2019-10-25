// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Beatmaps.ControlPoints
{
    public abstract class ControlPoint : IComparable<ControlPoint>, IEquatable<ControlPoint>
    {
        /// <summary>
        /// The time at which the control point takes effect.
        /// </summary>
        public double Time => controlPointGroup?.Time ?? 0;

        private ControlPointGroup controlPointGroup;

        public void AttachGroup(ControlPointGroup pointGroup) => this.controlPointGroup = pointGroup;

        public int CompareTo(ControlPoint other) => Time.CompareTo(other.Time);

        /// <summary>
        /// Whether this control point is equivalent to another, ignoring time.
        /// </summary>
        /// <param name="other">Another control point to compare with.</param>
        /// <returns>Whether equivalent.</returns>
        public abstract bool EquivalentTo(ControlPoint other);

        public bool Equals(ControlPoint other) => Time.Equals(other?.Time) && EquivalentTo(other);
    }
}
