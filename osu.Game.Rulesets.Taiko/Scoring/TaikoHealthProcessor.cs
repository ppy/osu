// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Scoring
{
    /// <summary>
    /// A <see cref="HealthProcessor"/> for the taiko ruleset.
    /// Taiko fails if the player has not half-filled their health by the end of the map.
    /// </summary>
    public partial class TaikoHealthProcessor : AccumulatingHealthProcessor
    {
        /// <summary>
        /// A value used for calculating <see cref="hpMultiplier"/>.
        /// </summary>
        private const double object_count_factor = 3;

        /// <summary>
        /// HP multiplier for a successful <see cref="HitResult"/>.
        /// </summary>
        private double hpMultiplier;

        /// <summary>
        /// HP multiplier for a <see cref="HitResult"/> that does not satisfy <see cref="HitResultExtensions.IsHit"/>.
        /// </summary>
        private double hpMissMultiplier;

        /// <summary>
        /// Sum of all achievable health increases throughout the map.
        /// Used to determine if there are any objects that give health.
        /// If there are none, health will be forcibly pulled up to 1 to avoid cases of impassable maps.
        /// </summary>
        private double sumOfMaxHealthIncreases;

        public TaikoHealthProcessor()
            : base(0.5)
        {
        }

        protected override void ApplyResultInternal(JudgementResult result)
        {
            base.ApplyResultInternal(result);
            sumOfMaxHealthIncreases += result.Judgement.MaxHealthIncrease;
        }

        protected override void RevertResultInternal(JudgementResult result)
        {
            base.RevertResultInternal(result);
            sumOfMaxHealthIncreases -= result.Judgement.MaxHealthIncrease;
        }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            if (storeResults && sumOfMaxHealthIncreases == 0)
                Health.Value = 1;
            sumOfMaxHealthIncreases = 0;
        }

        public override void ApplyBeatmap(IBeatmap beatmap)
        {
            base.ApplyBeatmap(beatmap);

            hpMultiplier = 1 / (object_count_factor * Math.Max(1, beatmap.HitObjects.OfType<Hit>().Count()) * IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 0.5, 0.75, 0.98));
            hpMissMultiplier = IBeatmapDifficultyInfo.DifficultyRange(beatmap.Difficulty.DrainRate, 0.0018, 0.0075, 0.0120);
        }

        protected override double GetHealthIncreaseFor(JudgementResult result)
            => base.GetHealthIncreaseFor(result) * (result.IsHit ? hpMultiplier : hpMissMultiplier);
    }
}
