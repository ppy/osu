// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Game.Rulesets.Objects.Types;
using System.Collections.Generic;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using System;
using System.Linq;

namespace osu.Game.Rulesets.Objects.Legacy
{
    internal abstract class ConvertSlider : ConvertHitObject, IHasPathWithRepeats, IHasSliderVelocity
    {
        /// <summary>
        /// Scoring distance with a speed-adjusted beat length of 1 second.
        /// </summary>
        private const float base_scoring_distance = 100;

        /// <summary>
        /// <see cref="ConvertSlider"/>s don't need a curve since they're converted to ruleset-specific hitobjects.
        /// </summary>
        public SliderPath Path { get; set; } = null!;

        public double Distance => Path.Distance;

        public IList<IList<HitSampleInfo>> NodeSamples { get; set; } = null!;

        public int RepeatCount { get; set; }

        [JsonIgnore]
        public double Duration
        {
            get => this.SpanCount() * Distance / Velocity;
            set => throw new NotSupportedException($"Adjust via {nameof(RepeatCount)} instead"); // can be implemented if/when needed.
        }

        public double EndTime => StartTime + Duration;

        public double Velocity = 1;

        public BindableNumber<double> SliderVelocityMultiplierBindable { get; } = new BindableDouble(1);

        public double SliderVelocityMultiplier
        {
            get => SliderVelocityMultiplierBindable.Value;
            set => SliderVelocityMultiplierBindable.Value = value;
        }

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, IBeatmapDifficultyInfo difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);

            double scoringDistance = base_scoring_distance * difficulty.SliderMultiplier * SliderVelocityMultiplier;

            Velocity = scoringDistance / timingPoint.BeatLength;
        }

        protected override void CopyFrom(HitObject other, IDictionary<object, object> referenceLookup)
        {
            base.CopyFrom(other, referenceLookup);

            if (other is not ConvertSlider convertSlider)
                throw new ArgumentException($"{nameof(other)} must be of type {nameof(ConvertSlider)}");

            Path = convertSlider.Path;
            NodeSamples = convertSlider.NodeSamples.Select(s => (IList<HitSampleInfo>)s.Select(s2 => s2.DeepClone(referenceLookup)).ToList()).ToList();
            RepeatCount = convertSlider.RepeatCount;
            Velocity = convertSlider.Velocity;
            SliderVelocityMultiplier = convertSlider.SliderVelocityMultiplier;
        }
    }
}
