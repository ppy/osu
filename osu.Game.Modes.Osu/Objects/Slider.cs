﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Samples;
using System;
using System.Collections.Generic;

namespace osu.Game.Modes.Osu.Objects
{
    public class Slider : OsuHitObject
    {
        public override double EndTime => StartTime + RepeatCount * Curve.Length / Velocity;

        public override Vector2 EndPosition => PositionAt(1);

        /// <summary>
        /// Computes the position on the slider at a given progress that ranges from 0 (beginning of the slider)
        /// to 1 (end of the slider). This includes repeat logic.
        /// </summary>
        /// <param name="progress">Ranges from 0 (beginning of the slider) to 1 (end of the slider).</param>
        /// <returns></returns>
        public Vector2 PositionAt(double progress) => Curve.PositionAt(CurveProgressAt(progress));

        /// <summary>
        /// Find the current progress along the curve, accounting for repeat logic.
        /// </summary>
        public double CurveProgressAt(double progress)
        {
            var p = progress * RepeatCount % 1;
            if (RepeatAt(progress) % 2 == 1)
                p = 1 - p;
            return p;
        }

        /// <summary>
        /// Determine which repeat of the slider we are on at a given progress.
        /// Range is 0..RepeatCount where 0 is the first run.
        /// </summary>
        public int RepeatAt(double progress) => (int)(progress * RepeatCount);

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
            var scoringDistance = 100 * baseDifficulty.SliderMultiplier;

            Velocity = scoringDistance / beatmap.Timing.BeatDistanceAt(StartTime);
            TickDistance = scoringDistance / (baseDifficulty.SliderTickRate * beatmap.Timing.BPMMultiplierAt(StartTime));
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

        public override HitObjectType Type => HitObjectType.Slider;
    }

    public enum CurveTypes
    {
        Catmull,
        Bezier,
        Linear,
        PerfectCurve
    }
}
