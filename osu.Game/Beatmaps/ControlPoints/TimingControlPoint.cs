// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class TimingControlPoint : ControlPoint, IEquatable<TimingControlPoint>
    {
        /// <summary>
        /// The time signature at this control point.
        /// </summary>
        public TimeSignatures TimeSignature = TimeSignatures.SimpleQuadruple;

        /// <summary>
        /// The beat length at this control point.
        /// </summary>
        public virtual double BeatLength
        {
            get => beatLength;
            set => beatLength = MathHelper.Clamp(value, 6, 60000);
        }

        private double beatLength = 1000;

        public bool Equals(TimingControlPoint other)
            => base.Equals(other)
               && TimeSignature == other?.TimeSignature && beatLength.Equals(other.beatLength);
    }
}
