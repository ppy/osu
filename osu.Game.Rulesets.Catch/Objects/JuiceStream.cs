// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
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

namespace osu.Game.Rulesets.Catch.Objects
{
    public class JuiceStream : CatchHitObject, IHasCurve
    {
        /// <summary>
        /// Positional distance that results in a duration of one second, before any speed adjustments.
        /// </summary>
        private const float base_scoring_distance = 100;

        public int RepeatCount { get; set; }

        public double Velocity;
        public double TickDistance;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(StartTime);

            double scoringDistance = base_scoring_distance * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

            Velocity = scoringDistance / timingPoint.BeatLength;
            TickDistance = scoringDistance / difficulty.SliderTickRate;
        }

        protected override void CreateNestedHitObjects()
        {
            base.CreateNestedHitObjects();
            createTicks();
        }

        private void createTicks()
        {
            if (TickDistance == 0)
                return;

            var length = Path.Distance;
            var tickDistance = Math.Min(TickDistance, length);
            var spanDuration = length / Velocity;

            var minDistanceFromEnd = Velocity * 0.01;

            AddNested(new Fruit
            {
                Samples = Samples,
                StartTime = StartTime,
                X = X
            });

            double lastTickTime = StartTime;

            for (int span = 0; span < this.SpanCount(); span++)
            {
                var spanStartTime = StartTime + span * spanDuration;
                var reversed = span % 2 == 1;

                for (double d = tickDistance;; d += tickDistance)
                {
                    bool isLastTick = false;
                    if (d + minDistanceFromEnd >= length)
                    {
                        d = length;
                        isLastTick = true;
                    }

                    var timeProgress = d / length;
                    var distanceProgress = reversed ? 1 - timeProgress : timeProgress;

                    double time = spanStartTime + timeProgress * spanDuration;

                    if (LegacyLastTickOffset != null)
                    {
                        // If we're the last tick, apply the legacy offset
                        if (span == this.SpanCount() - 1 && isLastTick)
                            time = Math.Max(StartTime + Duration / 2, time - LegacyLastTickOffset.Value);
                    }

                    double tinyTickInterval = time - lastTickTime;
                    while (tinyTickInterval > 100)
                        tinyTickInterval /= 2;

                    for (double t = lastTickTime + tinyTickInterval; t < time; t += tinyTickInterval)
                    {
                        double progress = reversed ? 1 - (t - spanStartTime) / spanDuration : (t - spanStartTime) / spanDuration;

                        AddNested(new TinyDroplet
                        {
                            StartTime = t,
                            X = X + Path.PositionAt(progress).X / CatchPlayfield.BASE_WIDTH,
                            Samples = new List<SampleInfo>(Samples.Select(s => new SampleInfo
                            {
                                Bank = s.Bank,
                                Name = @"slidertick",
                                Volume = s.Volume
                            }))
                        });
                    }

                    lastTickTime = time;

                    if (isLastTick)
                        break;

                    AddNested(new Droplet
                    {
                        StartTime = time,
                        X = X + Path.PositionAt(distanceProgress).X / CatchPlayfield.BASE_WIDTH,
                        Samples = new List<SampleInfo>(Samples.Select(s => new SampleInfo
                        {
                            Bank = s.Bank,
                            Name = @"slidertick",
                            Volume = s.Volume
                        }))
                    });
                }

                AddNested(new Fruit
                {
                    Samples = Samples,
                    StartTime = spanStartTime + spanDuration,
                    X = X + Path.PositionAt(reversed ? 0 : 1).X / CatchPlayfield.BASE_WIDTH
                });
            }
        }

        public double EndTime => StartTime + this.SpanCount() * Path.Distance / Velocity;

        public float EndX => X + this.CurvePositionAt(1).X / CatchPlayfield.BASE_WIDTH;

        public double Duration => EndTime - StartTime;

        private SliderPath path;

        public SliderPath Path
        {
            get => path;
            set => path = value;
        }

        public double Distance => Path.Distance;

        public List<List<SampleInfo>> NodeSamples { get; set; } = new List<List<SampleInfo>>();

        public double? LegacyLastTickOffset { get; set; }
    }
}
