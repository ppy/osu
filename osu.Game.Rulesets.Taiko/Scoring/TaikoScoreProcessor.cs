// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Taiko.Scoring
{
    internal class TaikoScoreProcessor : ScoreProcessor<TaikoHitObject>
    {
        /// <summary>
        /// A value used for calculating <see cref="hpMultiplier"/>.
        /// </summary>
        private const double object_count_factor = 3;

        /// <summary>
        /// Taiko fails at the end of the map if the player has not half-filled their HP bar.
        /// </summary>
        protected override bool DefaultFailCondition => JudgedHits == MaxHits && Health.Value <= 0.5;

        /// <summary>
        /// HP multiplier for a successful <see cref="HitResult"/>.
        /// </summary>
        private double hpMultiplier;

        /// <summary>
        /// HP multiplier for a <see cref="HitResult.Miss"/>.
        /// </summary>
        private double hpMissMultiplier;

        public TaikoScoreProcessor(DrawableRuleset<TaikoHitObject> drawableRuleset)
            : base(drawableRuleset)
        {
        }

        protected override void ApplyBeatmap(Beatmap<TaikoHitObject> beatmap)
        {
            base.ApplyBeatmap(beatmap);

            hpMultiplier = 1 / (object_count_factor * beatmap.HitObjects.FindAll(o => o is Hit).Count * BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.DrainRate, 0.5, 0.75, 0.98));

            hpMissMultiplier = BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.DrainRate, 0.0018, 0.0075, 0.0120);
        }

        protected override double HealthAdjustmentFactorFor(JudgementResult result)
            => result.Type == HitResult.Miss ? hpMissMultiplier : hpMultiplier;

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);

            Health.Value = 0;
        }

        public override HitWindows CreateHitWindows() => new TaikoHitWindows();
    }
}
