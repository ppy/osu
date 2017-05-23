// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class ControlPoint : IComparable<ControlPoint>
    {
        /// <summary>
        /// The time at which the control point takes effect.
        /// </summary>
        public double Time;

        public int CompareTo(ControlPoint other) => Time.CompareTo(other.Time);
    }
}
