// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Bindables;
using osu.Framework.Utils;
using osu.Game.Beatmaps.Timing;
using osu.Game.Graphics;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osuTK.Graphics;

namespace osu.Game.Beatmaps.ControlPoints
{
    public class TimingControlPoint : ControlPoint, IEquatable<TimingControlPoint>
    {
        /// <summary>
        /// The time signature at this control point.
        /// </summary>
        public readonly Bindable<TimeSignature> TimeSignatureBindable = new Bindable<TimeSignature>(TimeSignature.SimpleQuadruple);

        /// <summary>
        /// Whether the first bar line of this control point is ignored.
        /// </summary>
        public readonly BindableBool OmitFirstBarLineBindable = new BindableBool();

        /// <summary>
        /// Default length of a beat in milliseconds. Used whenever there is no beatmap or track playing.
        /// </summary>
        private const double default_beat_length = 60000.0 / 60.0;

        public override Color4 GetRepresentingColour(OsuColour colours) => colours.Orange1;

        public static readonly TimingControlPoint DEFAULT = new TimingControlPoint
        {
            BeatLengthBindable =
            {
                Value = default_beat_length,
                Disabled = true
            },
            OmitFirstBarLineBindable = { Disabled = true },
            TimeSignatureBindable = { Disabled = true }
        };

        /// <summary>
        /// The time signature at this control point.
        /// </summary>
        public TimeSignature TimeSignature
        {
            get => TimeSignatureBindable.Value;
            set => TimeSignatureBindable.Value = value;
        }

        /// <summary>
        /// Whether the first bar line of this control point is ignored.
        /// </summary>
        public bool OmitFirstBarLine
        {
            get => OmitFirstBarLineBindable.Value;
            set => OmitFirstBarLineBindable.Value = value;
        }

        public const double DEFAULT_BEAT_LENGTH = 1000;

        /// <summary>
        /// The beat length at this control point.
        /// </summary>
        public readonly BindableDouble BeatLengthBindable = new BindableDouble(DEFAULT_BEAT_LENGTH)
        {
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
        public override bool IsRedundant(ControlPoint? existing) => false;

        public override void CopyFrom(ControlPoint other)
        {
            TimeSignature = ((TimingControlPoint)other).TimeSignature;
            OmitFirstBarLine = ((TimingControlPoint)other).OmitFirstBarLine;
            BeatLength = ((TimingControlPoint)other).BeatLength;

            base.CopyFrom(other);
        }

        public override bool Equals(ControlPoint? other)
            => other is TimingControlPoint otherTimingControlPoint
               && Equals(otherTimingControlPoint);

        public bool Equals(TimingControlPoint? other)
            => base.Equals(other)
               && TimeSignature.Equals(other.TimeSignature)
               && OmitFirstBarLine == other.OmitFirstBarLine
               && BeatLength.Equals(other.BeatLength);

        public override int GetHashCode() => HashCode.Combine(base.GetHashCode(), TimeSignature, BeatLength, OmitFirstBarLine);

        public List<HitObject> HitObjectsInTimingRange(IBeatmap beatmap)
        {
            // If the first group, we grab all hitobjects prior to the next, if the last group, we grab all remaining hitobjects
            double startTime = beatmap.ControlPointInfo.TimingPoints.Any(x => x.Time < Time) ? Time : double.MinValue;
            double endTime = beatmap.ControlPointInfo.TimingPoints.FirstOrDefault(x => x.Time > Time)?.Time ?? double.MaxValue;

            return beatmap.HitObjects.Where(x => Precision.AlmostBigger(x.StartTime, startTime) && Precision.DefinitelyBigger(endTime, x.StartTime)).ToList();
        }

        public void AdjustHitObjectOffset(IBeatmap beatmap, double adjust)
        {
            foreach (HitObject hitObject in HitObjectsInTimingRange(beatmap))
            {
                hitObject.StartTime += adjust;
            }
        }

        public void SetHitObjectBPM(IBeatmap beatmap, double oldBeatLength)
        {
            foreach (HitObject hitObject in HitObjectsInTimingRange(beatmap))
            {
                double beat = (hitObject.StartTime - Time) / oldBeatLength;

                hitObject.StartTime = (beat * BeatLength) + Time;

                if (hitObject is not IHasRepeats && hitObject is IHasDuration hitObjectWithDuration)
                    hitObjectWithDuration.Duration *= BeatLength / oldBeatLength;
            }
        }
    }
}
