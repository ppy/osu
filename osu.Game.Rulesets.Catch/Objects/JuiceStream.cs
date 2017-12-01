// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using OpenTK;
using osu.Framework.Lists;

namespace osu.Game.Rulesets.Catch.Objects
{
    public class JuiceStream : CatchHitObject, IHasCurve
    {
        /// <summary>
        /// Positional distance that results in a duration of one second, before any speed adjustments.
        /// </summary>
        private const float base_scoring_distance = 100;

        public readonly SliderCurve Curve = new SliderCurve();

        public int RepeatCount { get; set; } = 1;

        public double Velocity;
        public double TickDistance;

        public override void ApplyDefaults(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaults(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(StartTime);

            double scoringDistance = base_scoring_distance * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

            Velocity = scoringDistance / timingPoint.BeatLength;
            TickDistance = scoringDistance / difficulty.SliderTickRate;
        }

        public IEnumerable<CatchHitObject> Ticks
        {
            get
            {
                SortedList<CatchHitObject> ticks = new SortedList<CatchHitObject>((a, b) => a.StartTime.CompareTo(b.StartTime));

                if (TickDistance == 0)
                    return ticks;

                var length = Curve.Distance;
                var tickDistance = Math.Min(TickDistance, length);
                var repeatDuration = length / Velocity;

                var minDistanceFromEnd = Velocity * 0.01;

                ticks.Add(new Fruit
                {
                    Samples = Samples,
                    ComboColour = ComboColour,
                    StartTime = StartTime,
                    X = X
                });

                for (var repeat = 0; repeat < RepeatCount; repeat++)
                {
                    var repeatStartTime = StartTime + repeat * repeatDuration;
                    var reversed = repeat % 2 == 1;

                    for (var d = tickDistance; d <= length; d += tickDistance)
                    {
                        if (d > length - minDistanceFromEnd)
                            break;

                        var timeProgress = d / length;
                        var distanceProgress = reversed ? 1 - timeProgress : timeProgress;

                        var lastTickTime = repeatStartTime + timeProgress * repeatDuration;
                        ticks.Add(new Droplet
                        {
                            StartTime = lastTickTime,
                            ComboColour = ComboColour,
                            X = Curve.PositionAt(distanceProgress).X / CatchPlayfield.BASE_WIDTH,
                            Samples = new SampleInfoList(Samples.Select(s => new SampleInfo
                            {
                                Bank = s.Bank,
                                Name = @"slidertick",
                                Volume = s.Volume
                            }))
                        });
                    }

                    double tinyTickInterval = tickDistance / length * repeatDuration;
                    while (tinyTickInterval > 100)
                        tinyTickInterval /= 2;

                    for (double t = 0; t < repeatDuration; t += tinyTickInterval)
                    {
                        double progress = reversed ? 1 - t / repeatDuration : t / repeatDuration;

                        ticks.Add(new TinyDroplet
                        {
                            StartTime = repeatStartTime + t,
                            ComboColour = ComboColour,
                            X = Curve.PositionAt(progress).X / CatchPlayfield.BASE_WIDTH,
                            Samples = new SampleInfoList(Samples.Select(s => new SampleInfo
                            {
                                Bank = s.Bank,
                                Name = @"slidertick",
                                Volume = s.Volume
                            }))
                        });
                    }

                    ticks.Add(new Fruit
                    {
                        Samples = Samples,
                        ComboColour = ComboColour,
                        StartTime = repeatStartTime + repeatDuration,
                        X = Curve.PositionAt(reversed ? 0 : 1).X / CatchPlayfield.BASE_WIDTH
                    });
                }

                return ticks;
            }
        }

        public double EndTime => StartTime + RepeatCount * Curve.Distance / Velocity;

        public float EndX => Curve.PositionAt(ProgressAt(1)).X / CatchPlayfield.BASE_WIDTH;

        public double Duration => EndTime - StartTime;

        public double Distance
        {
            get { return Curve.Distance; }
            set { Curve.Distance = value; }
        }

        public List<Vector2> ControlPoints
        {
            get { return Curve.ControlPoints; }
            set { Curve.ControlPoints = value; }
        }

        public List<SampleInfoList> RepeatSamples { get; set; } = new List<SampleInfoList>();

        public CurveType CurveType
        {
            get { return Curve.CurveType; }
            set { Curve.CurveType = value; }
        }

        public Vector2 PositionAt(double progress) => Curve.PositionAt(ProgressAt(progress));

        public double ProgressAt(double progress)
        {
            double p = progress * RepeatCount % 1;
            if (RepeatAt(progress) % 2 == 1)
                p = 1 - p;
            return p;
        }

        public int RepeatAt(double progress) => (int)(progress * RepeatCount);
    }
}
