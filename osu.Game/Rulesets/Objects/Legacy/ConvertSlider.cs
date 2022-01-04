// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Objects.Types;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Objects.Legacy
{
    internal abstract class ConvertSlider : ConvertHitObject, IHasPathWithRepeats, IHasLegacyLastTickOffset
    {
        /// <summary>
        /// Scoring distance with a speed-adjusted beat length of 1 second.
        /// </summary>
        private const float base_scoring_distance = 100;

        /// <summary>
        /// <see cref="ConvertSlider"/>s don't need a curve since they're converted to ruleset-specific hitobjects.
        /// </summary>
        public SliderPath Path { get; set; }

        public double Distance => Path.Distance;

        public IList<IList<HitSampleInfo>> NodeSamples { get; set; }
        public int RepeatCount { get; set; }

        [JsonIgnore]
        public double Duration
        {
            get => this.SpanCount() * Distance / Velocity;
            set => throw new System.NotSupportedException($"Adjust via {nameof(RepeatCount)} instead"); // can be implemented if/when needed.
        }

        public double EndTime => StartTime + Duration;

        public double Velocity = 1;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);

            double scoringDistance = base_scoring_distance * difficulty.SliderMultiplier * DifficultyControlPoint.SliderVelocity;

            Velocity = scoringDistance / timingPoint.BeatLength;
        }

        public double LegacyLastTickOffset => 36;
    }
}
