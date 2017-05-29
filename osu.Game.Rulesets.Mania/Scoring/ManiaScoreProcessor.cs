// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.Mania.Judgements;
using osu.Game.Rulesets.Mania.Objects;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Mania.Scoring
{
    internal class ManiaScoreProcessor : ScoreProcessor<ManiaHitObject, ManiaJudgement>
    {
        /// <summary>
        /// The maximum score achievable.
        /// Does _not_ include bonus score - for bonus score see <see cref="bonusScore"/>.
        /// </summary>
        private const int max_score = 1000000;

        /// <summary>
        /// The amount of the score attributed to combo.
        /// </summary>
        private const double combo_portion_max = max_score * 0.2;

        /// <summary>
        /// The amount of the score attributed to accuracy.
        /// </summary>
        private const double accuracy_portion_max = max_score * 0.8;

        /// <summary>
        /// The factor used to determine relevance of combos.
        /// </summary>
        private const double combo_base = 4;

        /// <summary>
        /// The combo value at which hit objects result in the max score possible.
        /// </summary>
        private const int combo_relevance_cap = 400;

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

        /// <summary>
        /// The cumulative combo portion of the score.
        /// </summary>
        private double comboScore => combo_portion_max * comboPortion / maxComboPortion;

        /// <summary>
        /// The cumulative accuracy portion of the score.
        /// </summary>
        private double accuracyScore => accuracy_portion_max * Math.Pow(Accuracy, 4) * totalHits / maxTotalHits;

        /// <summary>
        /// The cumulative bonus score.
        /// This is added on top of <see cref="max_score"/>, thus the total score can exceed <see cref="max_score"/>.
        /// </summary>
        private double bonusScore;

        /// <summary>
        /// The <see cref="comboPortion"/> achieved by a perfect playthrough.
        /// </summary>
        private double maxComboPortion;

        /// <summary>
        /// The portion of the score dedicated to combo.
        /// </summary>
        private double comboPortion;

        /// <summary>
        /// The <see cref="totalHits"/> achieved by a perfect playthrough.
        /// </summary>
        private int maxTotalHits;

        /// <summary>
        /// The total hits.
        /// </summary>
        private int totalHits;

        public ManiaScoreProcessor()
        {
        }

        public ManiaScoreProcessor(HitRenderer<ManiaHitObject, ManiaJudgement> hitRenderer)
            : base(hitRenderer)
        {
        }

        protected override void ComputeTargets(Beatmap<ManiaHitObject> beatmap)
        {
            BeatmapDifficulty difficulty = beatmap.BeatmapInfo.Difficulty;
            hpMultiplier = BeatmapDifficulty.DifficultyRange(difficulty.DrainRate, hp_multiplier_min, hp_multiplier_mid, hp_multiplier_max);
            hpMissMultiplier = BeatmapDifficulty.DifficultyRange(difficulty.DrainRate, hp_multiplier_miss_min, hp_multiplier_miss_mid, hp_multiplier_miss_max);

            while (true)
            {
                foreach (var obj in beatmap.HitObjects)
                {
                    var holdNote = obj as HoldNote;

                    if (obj is Note)
                    {
                        AddJudgement(new ManiaJudgement
                        {
                            Result = HitResult.Hit,
                            ManiaResult = ManiaHitResult.Perfect
                        });
                    }
                    else if (holdNote != null)
                    {
                        // Head
                        AddJudgement(new ManiaJudgement
                        {
                            Result = HitResult.Hit,
                            ManiaResult = ManiaJudgement.MAX_HIT_RESULT
                        });

                        // Ticks
                        int tickCount = holdNote.Ticks.Count();
                        for (int i = 0; i < tickCount; i++)
                        {
                            AddJudgement(new HoldNoteTickJudgement
                            {
                                Result = HitResult.Hit,
                                ManiaResult = ManiaJudgement.MAX_HIT_RESULT,
                            });
                        }

                        AddJudgement(new HoldNoteTailJudgement
                        {
                            Result = HitResult.Hit,
                            ManiaResult = ManiaJudgement.MAX_HIT_RESULT
                        });
                    }
                }

                if (!HasFailed)
                    break;

                hpMultiplier *= 1.01;
                hpMissMultiplier *= 0.98;

                Reset();
            }

            maxTotalHits = totalHits;
            maxComboPortion = comboPortion;
        }

        protected override void OnNewJudgement(ManiaJudgement judgement)
        {
            bool isTick = judgement is HoldNoteTickJudgement;

            if (!isTick)
                totalHits++;

            switch (judgement.Result)
            {
                case HitResult.Miss:
                    Health.Value += hpMissMultiplier * hp_increase_miss;
                    break;
                case HitResult.Hit:
                    if (isTick)
                    {
                        Health.Value += hpMultiplier * hp_increase_tick;
                        bonusScore += judgement.ResultValueForScore;
                    }
                    else
                    {
                        switch (judgement.ManiaResult)
                        {
                            case ManiaHitResult.Bad:
                                Health.Value += hpMultiplier * hp_increase_bad;
                                break;
                            case ManiaHitResult.Ok:
                                Health.Value += hpMultiplier * hp_increase_ok;
                                break;
                            case ManiaHitResult.Good:
                                Health.Value += hpMultiplier * hp_increase_good;
                                break;
                            case ManiaHitResult.Great:
                                Health.Value += hpMultiplier * hp_increase_great;
                                break;
                            case ManiaHitResult.Perfect:
                                Health.Value += hpMultiplier * hp_increase_perfect;
                                break;
                        }

                        // A factor that is applied to make higher combos more relevant
                        double comboRelevance = Math.Min(Math.Max(0.5, Math.Log(Combo.Value, combo_base)), Math.Log(combo_relevance_cap, combo_base));
                        comboPortion += judgement.ResultValueForScore * comboRelevance;
                    }
                    break;
            }

            int scoreForAccuracy = 0;
            int maxScoreForAccuracy = 0;

            foreach (var j in Judgements)
            {
                scoreForAccuracy += j.ResultValueForAccuracy;
                maxScoreForAccuracy += j.MaxResultValueForAccuracy;
            }

            Accuracy.Value = (double)scoreForAccuracy / maxScoreForAccuracy;
            TotalScore.Value = comboScore + accuracyScore + bonusScore;
        }

        protected override void Reset()
        {
            base.Reset();

            Health.Value = 1;

            bonusScore = 0;
            comboPortion = 0;
            totalHits = 0;
        }
    }
}
