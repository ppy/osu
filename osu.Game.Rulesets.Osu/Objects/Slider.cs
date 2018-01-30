﻿// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using OpenTK;
using osu.Game.Rulesets.Objects.Types;
using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Objects;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class Slider : OsuHitObject, IHasCurve
    {
        /// <summary>
        /// Scoring distance with a speed-adjusted beat length of 1 second.
        /// </summary>
        private const float base_scoring_distance = 100;

        public double EndTime => StartTime + this.SpanCount() * Curve.Distance / Velocity;
        public double Duration => EndTime - StartTime;

        public override Vector2 EndPosition => this.PositionAt(1);

        public SliderCurve Curve { get; } = new SliderCurve();

        public List<Vector2> ControlPoints
        {
            get { return Curve.ControlPoints; }
            set { Curve.ControlPoints = value; }
        }

        public CurveType CurveType
        {
            get { return Curve.CurveType; }
            set { Curve.CurveType = value; }
        }

        public double Distance
        {
            get { return Curve.Distance; }
            set { Curve.Distance = value; }
        }

        /// <summary>
        /// The position of the cursor at the point of completion of this <see cref="Slider"/> if it was hit
        /// with as few movements as possible. This is set and used by difficulty calculation.
        /// </summary>
        internal Vector2? LazyEndPosition;

        /// <summary>
        /// The distance travelled by the cursor upon completion of this <see cref="Slider"/> if it was hit
        /// with as few movements as possible. This is set and used by difficulty calculation.
        /// </summary>
        internal float LazyTravelDistance;

        public List<List<SampleInfo>> RepeatSamples { get; set; } = new List<List<SampleInfo>>();
        public int RepeatCount { get; set; }

        /// <summary>
        /// The length of one span of this <see cref="Slider"/>.
        /// </summary>
        public double SpanDuration => Duration / this.SpanCount();

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
            createRepeatPoints();
        }

        private void createTicks()
        {
            if (TickDistance == 0) return;

            var length = Curve.Distance;
            var tickDistance = Math.Min(TickDistance, length);

            var minDistanceFromEnd = Velocity * 0.01;

            for (var span = 0; span < this.SpanCount(); span++)
            {
                var spanStartTime = StartTime + span * SpanDuration;
                var reversed = span % 2 == 1;

                for (var d = tickDistance; d <= length; d += tickDistance)
                {
                    if (d > length - minDistanceFromEnd)
                        break;

                    var distanceProgress = d / length;
                    var timeProgress = reversed ? 1 - distanceProgress : distanceProgress;

                    var firstSample = Samples.FirstOrDefault(s => s.Name == SampleInfo.HIT_NORMAL) ?? Samples.FirstOrDefault(); // TODO: remove this when guaranteed sort is present for samples (https://github.com/ppy/osu/issues/1933)
                    var sampleList = new List<SampleInfo>();

                    if (firstSample != null)
                        sampleList.Add(new SampleInfo
                        {
                            Bank = firstSample.Bank,
                            Volume = firstSample.Volume,
                            Name = @"slidertick",
                        });

                    AddNested(new SliderTick
                    {
                        SpanIndex = span,
                        StartTime = spanStartTime + timeProgress * SpanDuration,
                        Position = Curve.PositionAt(distanceProgress),
                        StackHeight = StackHeight,
                        Scale = Scale,
                        ComboColour = ComboColour,
                        Samples = sampleList
                    });
                }
            }
        }

        private void createRepeatPoints()
        {
            for (int repeatIndex = 0, repeat = 1; repeatIndex < RepeatCount; repeatIndex++, repeat++)
            {
                AddNested(new RepeatPoint
                {
                    RepeatIndex = repeatIndex,
                    SpanDuration = SpanDuration,
                    StartTime = StartTime + repeat * SpanDuration,
                    Position = Curve.PositionAt(repeat % 2),
                    StackHeight = StackHeight,
                    Scale = Scale,
                    ComboColour = ComboColour,
                    Samples = new List<SampleInfo>(RepeatSamples[repeatIndex])
                });
            }
        }
    }
}
