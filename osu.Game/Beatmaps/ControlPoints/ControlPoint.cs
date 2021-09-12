// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using Newtonsoft.Json;
using osu.Game.Graphics;
using osu.Game.Utils;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.ControlPoints
{
    public abstract class ControlPoint : IComparable<ControlPoint>, IDeepCloneable<ControlPoint>
    {
        /// <summary>
        /// The time at which the control point takes effect.
        /// </summary>
        [JsonIgnore]
        public double Time => controlPointGroup?.Time ?? 0;

        private ControlPointGroup controlPointGroup;

        public void AttachGroup(ControlPointGroup pointGroup) => controlPointGroup = pointGroup;

        public int CompareTo(ControlPoint other) => Time.CompareTo(other.Time);

        public virtual Color4 GetRepresentingColour(OsuColour colours) => colours.Yellow;

        /// <summary>
        /// Determines whether this <see cref="ControlPoint"/> results in a meaningful change when placed alongside another.
        /// </summary>
        /// <param name="existing">An existing control point to compare with.</param>
        /// <returns>Whether this <see cref="ControlPoint"/> is redundant when placed alongside <paramref name="existing"/>.</returns>
        public abstract bool IsRedundant(ControlPoint existing);

        /// <summary>
        /// Create an unbound copy of this control point.
        /// </summary>
        public ControlPoint DeepClone()
        {
            var copy = (ControlPoint)Activator.CreateInstance(GetType());

            copy.CopyFrom(this);

            return copy;
        }

        public virtual void CopyFrom(ControlPoint other)
        {
        }
    }
}
