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

        public void AttachGroup(ControlPointGroup pointGroup) => controlPointGroup = pointGroup;

        public int CompareTo(ControlPoint other) => Time.CompareTo(other.Time);

        /// <summary>
        /// Whether this control point is equivalent to another, ignoring time.
        /// </summary>
        /// <param name="other">Another control point to compare with.</param>
        /// <returns>Whether equivalent.</returns>
        public abstract bool EquivalentTo(ControlPoint other);

        /// <summary>
        /// Whether this control point results in a meaningful change when placed after another.
        /// </summary>
        /// <param name="other">Another control point to compare with.</param>
        /// <param name="time">The time this timing point will be placed at.</param>
        /// <returns>Whether redundant.</returns>
        public abstract bool IsRedundant(ControlPoint other, double time);

        public bool Equals(ControlPoint other) => Time == other?.Time && EquivalentTo(other);
    }
}
