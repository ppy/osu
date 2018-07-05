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
        /// Whether this <see cref="ControlPoint"/> provides the same parametric changes as another <see cref="ControlPoint"/>.
        /// Basically an equality check without considering the <see cref="Time"/>.
        /// </summary>
        /// <param name="other">The <see cref="ControlPoint"/> to compare to.</param>
        /// <returns>Whether this <see cref="ControlPoint"/> is equivalent to <paramref name="other"/>.</returns>
        public virtual bool EquivalentTo(ControlPoint other) => true;

        public bool Equals(ControlPoint other)
            => EquivalentTo(other) && Time.Equals(other?.Time);
    }
}
