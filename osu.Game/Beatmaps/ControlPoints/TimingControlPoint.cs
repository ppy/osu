// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps.Timing;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class TimingControlPoint : ControlPoint
    {
        /// <summary>
        /// The time signature at this control point.
        /// </summary>
        public readonly Bindable<TimeSignatures> TimeSignatureBindable = new Bindable<TimeSignatures>(TimeSignatures.SimpleQuadruple) { Default = TimeSignatures.SimpleQuadruple };

        /// <summary>
        /// The time signature at this control point.
        /// </summary>
        public TimeSignatures TimeSignature
        {
            get => TimeSignatureBindable.Value;
            set => TimeSignatureBindable.Value = value;
        }

        public const double DEFAULT_BEAT_LENGTH = 1000;

        /// <summary>
        /// The beat length at this control point.
        /// </summary>
        public readonly BindableDouble BeatLengthBindable = new BindableDouble(DEFAULT_BEAT_LENGTH)
        {
            Default = DEFAULT_BEAT_LENGTH,
            MinValue = 6,
            MaxValue = 60000
        };

        /// <summary>
        /// The beat length at this control point.
        /// </summary>
        public double BeatLength
        {
            get => BeatLengthBindable.Value;
            set => BeatLengthBindable.Value = value;
        }

        public override bool EquivalentTo(ControlPoint other) =>
            other is TimingControlPoint otherTyped
            && TimeSignature == otherTyped.TimeSignature && BeatLength.Equals(otherTyped.BeatLength);
    }
}
