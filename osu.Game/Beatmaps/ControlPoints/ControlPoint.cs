// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class ControlPoint : IComparable<ControlPoint>, IEquatable<ControlPoint>
    {
        /// <summary>
        /// The time at which the control point takes effect.
        /// </summary>
        public double Time;

        public int CompareTo(ControlPoint other) => Time.CompareTo(other.Time);

        /// <summary>
        /// Whether this <see cref="ControlPoint"/> provides the same changes to gameplay as another <see cref="ControlPoint"/>.
        /// </summary>
        /// <param name="other">The <see cref="ControlPoint"/> to compare to.</param>
        /// <returns>Whether this <see cref="ControlPoint"/> provides the same changes to gameplay as <paramref name="other"/>.</returns>
        public virtual bool ChangeEquals(ControlPoint other) => !ReferenceEquals(null, other);

        public bool Equals(ControlPoint other)
            => ChangeEquals(other)
               && !ReferenceEquals(null, other)
               && Time.Equals(other.Time);
    }
}
