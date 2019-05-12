// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osuTK;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class TimingControlPoint : ControlPoint
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

        public override bool EquivalentTo(ControlPoint other)
            => base.EquivalentTo(other)
               && other is TimingControlPoint timing
               && TimeSignature.Equals(timing.TimeSignature)
               && BeatLength.Equals(timing.BeatLength);
    }
}
