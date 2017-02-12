// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Beatmaps;
using OpenTK;
using System.Collections.Generic;
using System;
using osu.Game.Beatmaps.Samples;

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
                if (Curve != null)
                    Curve.Offset = StackOffset;
            }
        }

        public double Velocity;
        public double TickDistance;

        public override void SetDefaultsFromBeatmap(Beatmap beatmap)
        {
            base.SetDefaultsFromBeatmap(beatmap);

            var baseDifficulty = beatmap.BeatmapInfo.BaseDifficulty;

            var startBeatLength = beatmap.BeatLengthAt(StartTime);
            var multipliedStartBeatLength = beatmap.BeatLengthAt(StartTime, true);

            Velocity = 100 / multipliedStartBeatLength * baseDifficulty.SliderMultiplier;
            TickDistance = (100 * baseDifficulty.SliderMultiplier) / baseDifficulty.SliderTickRate / (multipliedStartBeatLength / startBeatLength);
        }

        public int RepeatCount;

        public SliderCurve Curve;

        public IEnumerable<SliderTick> Ticks
        {
            get
            {
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
                            Position = Curve.PositionAt(distanceProgress) - StackedPosition,
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
