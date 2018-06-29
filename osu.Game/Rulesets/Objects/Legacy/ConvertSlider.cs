// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Types;
using System.Collections.Generic;
using OpenTK;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;

namespace osu.Game.Rulesets.Objects.Legacy
{
    internal abstract class ConvertSlider : HitObject, IHasCurve, IHasLegacyLastTickOffset
    {
        /// <summary>
        /// Scoring distance with a speed-adjusted beat length of 1 second.
        /// </summary>
        private const float base_scoring_distance = 100;

        /// <summary>
        /// <see cref="ConvertSlider"/>s don't need a curve since they're converted to ruleset-specific hitobjects.
        /// </summary>
        public SliderCurve Curve { get; } = null;
        public List<Vector2> ControlPoints { get; set; }
        public CurveType CurveType { get; set; }

        public double Distance { get; set; }

        public List<List<SampleInfo>> RepeatSamples { get; set; }
        public int RepeatCount { get; set; }

        public double EndTime => StartTime + this.SpanCount() * Distance / Velocity;
        public double Duration => EndTime - StartTime;

        public double Velocity = 1;

        protected override void ApplyDefaultsToSelf(ControlPointInfo controlPointInfo, BeatmapDifficulty difficulty)
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimingControlPoint timingPoint = controlPointInfo.TimingPointAt(StartTime);
            DifficultyControlPoint difficultyPoint = controlPointInfo.DifficultyPointAt(StartTime);

            double scoringDistance = base_scoring_distance * difficulty.SliderMultiplier * difficultyPoint.SpeedMultiplier;

            Velocity = scoringDistance / timingPoint.BeatLength;
        }

        public double LegacyLastTickOffset => 36;
    }
}
