// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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

        /// <summary>
        /// The length of one span of this <see cref="JuiceStream"/>.
        /// </summary>
        public double SpanDuration => Duration / this.SpanCount();

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

            var tickSamples = Samples.Select(s => new HitSampleInfo
            {
                Bank = s.Bank,
                Name = @"slidertick",
                Volume = s.Volume
            }).ToList();

            SliderEventDescriptor? lastEvent = null;

            foreach (var e in SliderEventGenerator.Generate(StartTime, SpanDuration, Velocity, TickDistance, Path.Distance, this.SpanCount(), LegacyLastTickOffset))
            {
                // generate tiny droplets since the last point
                if (lastEvent != null)
                {
                    double sinceLastTick = e.Time - lastEvent.Value.Time;

                    if (sinceLastTick > 80)
                    {
                        double timeBetweenTiny = sinceLastTick;
                        while (timeBetweenTiny > 100)
                            timeBetweenTiny /= 2;

                        for (double t = timeBetweenTiny; t < sinceLastTick; t += timeBetweenTiny)
                        {
                            AddNested(new TinyDroplet
                            {
                                Samples = tickSamples,
                                StartTime = t + lastEvent.Value.Time,
                                X = X + Path.PositionAt(
                                        lastEvent.Value.PathProgress + (t / sinceLastTick) * (e.PathProgress - lastEvent.Value.PathProgress)).X / CatchPlayfield.BASE_WIDTH,
                            });
                        }
                    }
                }

                // this also includes LegacyLastTick and this is used for TinyDroplet generation above.
                // this means that the final segment of TinyDroplets are increasingly mistimed where LegacyLastTickOffset is being applied.
                lastEvent = e;

                switch (e.Type)
                {
                    case SliderEventType.Tick:
                        AddNested(new Droplet
                        {
                            Samples = tickSamples,
                            StartTime = e.Time,
                            X = X + Path.PositionAt(e.PathProgress).X / CatchPlayfield.BASE_WIDTH,
                        });
                        break;

                    case SliderEventType.Head:
                    case SliderEventType.Tail:
                    case SliderEventType.Repeat:
                        AddNested(new Fruit
                        {
                            Samples = Samples,
                            StartTime = e.Time,
                            X = X + Path.PositionAt(e.PathProgress).X / CatchPlayfield.BASE_WIDTH,
                        });
                        break;
                }
            }
        }

        public double EndTime
        {
            get => StartTime + this.SpanCount() * Path.Distance / Velocity;
            set => throw new System.NotSupportedException($"Adjust via {nameof(RepeatCount)} instead"); // can be implemented if/when needed.
        }

        public float EndX => X + this.CurvePositionAt(1).X / CatchPlayfield.BASE_WIDTH;

        public double Duration => EndTime - StartTime;

        private readonly SliderPath path = new SliderPath();

        public SliderPath Path
        {
            get => path;
            set
            {
                path.ControlPoints.Clear();
                path.ExpectedDistance.Value = null;

                if (value != null)
                {
                    path.ControlPoints.AddRange(value.ControlPoints.Select(c => new PathControlPoint(c.Position.Value, c.Type.Value)));
                    path.ExpectedDistance.Value = value.ExpectedDistance.Value;
                }
            }
        }

        public double Distance => Path.Distance;

        public List<IList<HitSampleInfo>> NodeSamples { get; set; } = new List<IList<HitSampleInfo>>();

        public double? LegacyLastTickOffset { get; set; }
    }
}
