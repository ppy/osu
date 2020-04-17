// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Beatmaps.ControlPoints
{
    public abstract class ControlPoint : IComparable<ControlPoint>
    {
        /// <summary>
        /// The time at which the control point takes effect.
        /// </summary>
        public double Time => controlPointGroup?.Time ?? 0;

        private ControlPointGroup controlPointGroup;

        public void AttachGroup(ControlPointGroup pointGroup) => controlPointGroup = pointGroup;

        public int CompareTo(ControlPoint other) => Time.CompareTo(other.Time);

        /// <summary>
        /// Whether this control point results in a meaningful change when placed after another.
        /// </summary>
        /// <param name="existing">An existing control point to compare with.</param>
        /// <param name="time">The time this control point will be placed at if it is added.</param>
        /// <returns>Whether redundant.</returns>
        public abstract bool IsRedundant(ControlPoint existing, double time);
    }
}
