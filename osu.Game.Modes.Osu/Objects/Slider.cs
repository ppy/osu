// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Samples;
using osu.Game.Beatmaps.Timing;
using System;
using System.Collections.Generic;

namespace osu.Game.Modes.Osu.Objects
{
    public class Slider : OsuHitObject
    {
        public override double EndTime => StartTime + RepeatCount * Curve.Length / Velocity;

        public override Vector2 EndPosition => RepeatCount % 2 == 0 ? Position : Curve.PositionAt(1);

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

        public List<Vector2> ControlPoints
        {
            get { return Curve.ControlPoints; }
            set { Curve.ControlPoints = value; }
        }

        public double Length
        {
            get { return Curve.Length; }
            set { Curve.Length = value; }
        }

        public CurveTypes CurveType
        {
            get { return Curve.CurveType; }
            set { Curve.CurveType = value; }
        }

        public double Velocity;
        public double TickDistance;

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            var baseDifficulty = beatmap.BeatmapInfo.BaseDifficulty;

            ControlPoint overridePoint;
            ControlPoint timingPoint = beatmap.TimingPointAt(StartTime, out overridePoint);
            var velocityAdjustment = overridePoint?.VelocityAdjustment ?? 1;
            var baseVelocity = 100 * baseDifficulty.SliderMultiplier;

            Velocity = baseVelocity / (timingPoint.BeatLength * velocityAdjustment);
            TickDistance = baseVelocity / (baseDifficulty.SliderTickRate * velocityAdjustment);
        }

        public int RepeatCount = 1;

        internal readonly SliderCurve Curve = new SliderCurve();

        public IEnumerable<SliderTick> Ticks
        {
            get
            {
                if (TickDistance == 0) yield break;

                var length = Curve.Length;
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
                            Colour = Colour,
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
    }

    public enum CurveTypes
    {
        Catmull,
        Bezier,
        Linear,
        PerfectCurve
    }
}
