// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;
using osu.Game.Graphics;
using osu.Game.Utils;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.ControlPoints
{
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicParameterlessConstructor)]
    public abstract class ControlPoint : IComparable<ControlPoint>, IDeepCloneable<ControlPoint>, IEquatable<ControlPoint>, IControlPoint
    {
        /// <summary>
        /// Invoked when any of this <see cref="ControlPoint"/>'s properties have changed.
        /// </summary>
        public event Action<ControlPoint>? Changed;

        protected void RaiseChanged() => Changed?.Invoke(this);

        private double time;

        [JsonIgnore]
        public double Time
        {
            get => time;
            set
            {
                if (time == value)
                    return;

                time = value;
                RaiseChanged();
            }
        }

        public void AttachGroup(ControlPointGroup pointGroup) => Time = pointGroup.Time;

        public int CompareTo(ControlPoint? other) => Time.CompareTo(other?.Time);

        public virtual Color4 GetRepresentingColour(OsuColour colours) => colours.Yellow;

        /// <summary>
        /// Determines whether this <see cref="ControlPoint"/> results in a meaningful change when placed alongside another.
        /// </summary>
        /// <param name="existing">An existing control point to compare with.</param>
        /// <returns>Whether this <see cref="ControlPoint"/> is redundant when placed alongside <paramref name="existing"/>.</returns>
        public abstract bool IsRedundant(ControlPoint? existing);

        /// <summary>
        /// Create an unbound copy of this control point.
        /// </summary>
        public ControlPoint DeepClone()
        {
            var copy = (ControlPoint)Activator.CreateInstance(GetType())!;

            copy.CopyFrom(this);

            return copy;
        }

        public virtual void CopyFrom(ControlPoint other)
        {
            Time = other.Time;
        }

        public sealed override bool Equals(object? obj)
            => obj is ControlPoint otherControlPoint
               && Equals(otherControlPoint);

        public virtual bool Equals(ControlPoint? other)
        {
            if (ReferenceEquals(other, null)) return false;
            if (ReferenceEquals(other, this)) return true;

            return Time == other.Time;
        }

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => Time.GetHashCode();
    }
}
