// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Samples;
using osu.Game.Beatmaps.Timing;
using osu.Game.Modes.Objects.Types;
using System;
using System.Collections.Generic;
using osu.Game.Modes.Objects;

namespace osu.Game.Modes.Osu.Objects
{
    public class Slider : OsuHitObject, IHasCurve
    {
        public IHasCurve CurveObject { get; set; }

        public SliderCurve Curve => CurveObject.Curve;

        public double EndTime => StartTime + RepeatCount * Curve.Distance / Velocity;
        public double Duration => EndTime - StartTime;

        public override Vector2 EndPosition => PositionAt(1);

        public Vector2 PositionAt(double progress) => CurveObject.PositionAt(progress);
        public double ProgressAt(double progress) => CurveObject.ProgressAt(progress);
        public int RepeatAt(double progress) => CurveObject.RepeatAt(progress);

        public List<Vector2> ControlPoints => CurveObject.ControlPoints;
        public CurveType CurveType => CurveObject.CurveType;
        public double Distance => CurveObject.Distance;

        public int RepeatCount => CurveObject.RepeatCount;

        private int stackHeight;
        public override int StackHeight
        {
            get { return stackHeight; }
            set
            {
                stackHeight = value;
                Curve.Offset = StackOffset;
            }
        }

        public double Velocity;
        public double TickDistance;

        public override void SetDefaultsFromBeatmap(Beatmap<OsuHitObject> beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            var baseDifficulty = beatmap.BeatmapInfo.Difficulty;

            ControlPoint overridePoint;
            ControlPoint timingPoint = beatmap.TimingInfo.TimingPointAt(StartTime, out overridePoint);
            var velocityAdjustment = overridePoint?.VelocityAdjustment ?? 1;
            var baseVelocity = 100 * baseDifficulty.SliderMultiplier / velocityAdjustment;

            Velocity = baseVelocity / timingPoint.BeatLength;
            TickDistance = baseVelocity / baseDifficulty.SliderTickRate;
        }

        public IEnumerable<SliderTick> Ticks
        {
            get
            {
                if (TickDistance == 0) yield break;

                var length = Curve.Distance;
                var tickDistance = Math.Min(TickDistance, length);
                var repeatDuration = length / Velocity;

                var minDistanceFromEnd = Velocity * 0.01;

                for (var repeat = 0; repeat < RepeatCount; repeat++)
                {
                    var repeatStartTime = StartTime + repeat * repeatDuration;
                    var reversed = repeat % 2 == 1;

                    for (var d = tickDistance; d <= length; d += tickDistance)
                    {
                        if (d > length - minDistanceFromEnd)
                            break;

                        var distanceProgress = d / length;
                        var timeProgress = reversed ? 1 - distanceProgress : distanceProgress;

                        yield return new SliderTick
                        {
                            RepeatIndex = repeat,
                            StartTime = repeatStartTime + timeProgress * repeatDuration,
                            Position = Curve.PositionAt(distanceProgress),
                            StackHeight = StackHeight,
                            Scale = Scale,
                            ComboColour = ComboColour,
                            Sample = new HitSampleInfo
                            {
                                Type = SampleType.None,
                                Set = SampleSet.Soft,
                            },
                        };
                    }
                }
            }
        }

        public override HitObjectType Type => HitObjectType.Slider;
    }
}
