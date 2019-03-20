// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.Scoring
{
    internal class ManiaScoreProcessor : ScoreProcessor<ManiaHitObject>
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
        /// The default BAD hit HP increase.
        /// </summary>
        private const double hp_increase_bad = 0.005;

        /// <summary>
        /// The default OK hit HP increase.
        /// </summary>
        private const double hp_increase_ok = 0.010;

        /// <summary>
        /// The default GOOD hit HP increase.
        /// </summary>
        private const double hp_increase_good = 0.035;

        /// <summary>
        /// The default tick hit HP increase.
        /// </summary>
        private const double hp_increase_tick = 0.040;

        /// <summary>
        /// The default GREAT hit HP increase.
        /// </summary>
        private const double hp_increase_great = 0.055;

        /// <summary>
        /// The default PERFECT hit HP increase.
        /// </summary>
        private const double hp_increase_perfect = 0.065;

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
        /// The default MISS HP increase.
        /// </summary>
        private const double hp_increase_miss = -0.125;

        /// <summary>
        /// The MISS HP multiplier. This is multiplied to the miss hp increase.
        /// </summary>
        private double hpMissMultiplier = 1;

        /// <summary>
        /// The HIT HP multiplier. This is multiplied to hit hp increases.
        /// </summary>
        private double hpMultiplier = 1;

        public ManiaScoreProcessor()
        {
        }

        public ManiaScoreProcessor(DrawableRuleset<ManiaHitObject> drawableRuleset)
            : base(drawableRuleset)
        {
        }

        protected override void ApplyBeatmap(Beatmap<ManiaHitObject> beatmap)
        {
            base.ApplyBeatmap(beatmap);

            BeatmapDifficulty difficulty = beatmap.BeatmapInfo.BaseDifficulty;
            hpMultiplier = BeatmapDifficulty.DifficultyRange(difficulty.DrainRate, hp_multiplier_min, hp_multiplier_mid, hp_multiplier_max);
            hpMissMultiplier = BeatmapDifficulty.DifficultyRange(difficulty.DrainRate, hp_multiplier_miss_min, hp_multiplier_miss_mid, hp_multiplier_miss_max);
        }

        protected override void SimulateAutoplay(Beatmap<ManiaHitObject> beatmap)
        {
            while (true)
            {
                base.SimulateAutoplay(beatmap);

                if (!HasFailed)
                    break;

                hpMultiplier *= 1.01;
                hpMissMultiplier *= 0.98;

                Reset(false);
            }
        }

        protected override void ApplyResult(JudgementResult result)
        {
            base.ApplyResult(result);

            bool isTick = result.Judgement is HoldNoteTickJudgement;

            if (isTick)
            {
                if (result.IsHit)
                    Health.Value += hpMultiplier * hp_increase_tick;
            }
            else
            {
                switch (result.Type)
                {
                    case HitResult.Miss:
                        Health.Value += hpMissMultiplier * hp_increase_miss;
                        break;
                    case HitResult.Meh:
                        Health.Value += hpMultiplier * hp_increase_bad;
                        break;
                    case HitResult.Ok:
                        Health.Value += hpMultiplier * hp_increase_ok;
                        break;
                    case HitResult.Good:
                        Health.Value += hpMultiplier * hp_increase_good;
                        break;
                    case HitResult.Great:
                        Health.Value += hpMultiplier * hp_increase_great;
                        break;
                    case HitResult.Perfect:
                        Health.Value += hpMultiplier * hp_increase_perfect;
                        break;
                }
            }
        }

        public override HitWindows CreateHitWindows() => new ManiaHitWindows();
    }
}
