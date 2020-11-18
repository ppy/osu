// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class TimingControlPoint : ControlPoint
    {
        /// <summary>
        /// The time signature at this control point.
        /// </summary>
        public readonly Bindable<TimeSignatures> TimeSignatureBindable = new Bindable<TimeSignatures>(TimeSignatures.SimpleQuadruple) { Default = TimeSignatures.SimpleQuadruple };

        /// <summary>
        /// Default length of a beat in milliseconds. Used whenever there is no beatmap or track playing.
        /// </summary>
        private const double default_beat_length = 60000.0 / 60.0;

        public override Color4 GetRepresentingColour(OsuColour colours) => colours.YellowDark;

        public static readonly TimingControlPoint DEFAULT = new TimingControlPoint
        {
            BeatLengthBindable =
            {
                Value = default_beat_length,
                Disabled = true
            },
            TimeSignatureBindable = { Disabled = true }
        };

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

        /// <summary>
        /// The BPM at this control point.
        /// </summary>
        public double BPM => 60000 / BeatLength;

        // Timing points are never redundant as they can change the time signature.
        public override bool IsRedundant(ControlPoint existing) => false;
    }
}
