// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Scoring
{
    public class ManiaHealthProcessor : HealthProcessor
    {
        /// <summary>
        /// The hit HP multiplier at OD = 0.
        /// </summary>
        private const double hp_multiplier_min = 0.75;

        /// <summary>
        /// The hit HP multiplier at OD = 0.
        /// </summary>
        private const double hp_multiplier_mid = 0.85;

        /// <summary>
        /// The hit HP multiplier at OD = 0.
        /// </summary>
        private const double hp_multiplier_max = 1;

        /// <summary>
        /// The MISS HP multiplier at OD = 0.
        /// </summary>
        private const double hp_multiplier_miss_min = 0.5;

        /// <summary>
        /// The MISS HP multiplier at OD = 5.
        /// </summary>
        private const double hp_multiplier_miss_mid = 0.75;

        /// <summary>
        /// The MISS HP multiplier at OD = 10.
        /// </summary>
        private const double hp_multiplier_miss_max = 1;

        /// <summary>
        /// The MISS HP multiplier. This is multiplied to the miss hp increase.
        /// </summary>
        private double hpMissMultiplier = 1;

        /// <summary>
        /// The HIT HP multiplier. This is multiplied to hit hp increases.
        /// </summary>
        private double hpMultiplier = 1;

        public ManiaHealthProcessor(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override void ApplyBeatmap(IBeatmap beatmap)
        {
            base.ApplyBeatmap(beatmap);

            BeatmapDifficulty difficulty = beatmap.BeatmapInfo.BaseDifficulty;
            hpMultiplier = BeatmapDifficulty.DifficultyRange(difficulty.DrainRate, hp_multiplier_min, hp_multiplier_mid, hp_multiplier_max);
            hpMissMultiplier = BeatmapDifficulty.DifficultyRange(difficulty.DrainRate, hp_multiplier_miss_min, hp_multiplier_miss_mid, hp_multiplier_miss_max);
        }

        protected override double HealthAdjustmentFactorFor(JudgementResult result)
            => result.Type == HitResult.Miss ? hpMissMultiplier : hpMultiplier;
    }
}
