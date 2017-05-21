// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Judgements;
using osu.Game.Rulesets.Taiko.Objects;
using osu.Game.Rulesets.UI;
using OpenTK;

namespace osu.Game.Rulesets.Taiko.Scoring
{
    internal class TaikoScoreProcessor : ScoreProcessor<TaikoHitObject, TaikoJudgement>
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
        /// The HP awarded by a <see cref="TaikoHitResult.Great"/> hit.
        /// </summary>
        private const double hp_hit_great = 0.03;

        /// <summary>
        /// The HP awarded for a <see cref="TaikoHitResult.Good"/> hit.
        /// </summary>
        private const double hp_hit_good = 0.011;

        /// <summary>
        /// The minimum HP deducted for a <see cref="HitResult.Miss"/>.
        /// This occurs when HP Drain = 0.
        /// </summary>
        private const double hp_miss_min = -0.0018;

        /// <summary>
        /// The median HP deducted for a <see cref="HitResult.Miss"/>.
        /// This occurs when HP Drain = 5.
        /// </summary>
        private const double hp_miss_mid = -0.0075;

        /// <summary>
        /// The maximum HP deducted for a <see cref="HitResult.Miss"/>.
        /// This occurs when HP Drain = 10.
        /// </summary>
        private const double hp_miss_max = -0.12;

        /// <summary>
        /// The HP awarded for a <see cref="DrumRollTick"/> hit.
        /// <para>
        /// <see cref="DrumRollTick"/> hits award less HP as they're more spammable, although in hindsight
        /// this probably awards too little HP and is kept at this value for now for compatibility.
        /// </para>
        /// </summary>
        private const double hp_hit_tick = 0.00000003;

        /// <summary>
        /// Taiko fails at the end of the map if the player has not half-filled their HP bar.
        /// </summary>
        public override bool HasFailed => totalHits == maxTotalHits && Health.Value <= 0.5;

        /// <summary>
        /// The cumulative combo portion of the score.
        /// </summary>
        private double comboScore => combo_portion_max * comboPortion / maxComboPortion;

        /// <summary>
        /// The cumulative accuracy portion of the score.
        /// </summary>
        private double accuracyScore => accuracy_portion_max * Math.Pow(Accuracy, 3.6) * totalHits / maxTotalHits;

        /// <summary>
        /// The cumulative bonus score.
        /// This is added on top of <see cref="max_score"/>, thus the total score can exceed <see cref="max_score"/>.
        /// </summary>
        private double bonusScore;

        /// <summary>
        /// The multiple of the original score added to the combo portion of the score
        /// for correctly hitting a strong hit object with both keys.
        /// </summary>
        private double strongHitScale;

        private double hpIncreaseTick;
        private double hpIncreaseGreat;
        private double hpIncreaseGood;
        private double hpIncreaseMiss;

        private double maxComboPortion;
        private double comboPortion;
        private int maxTotalHits;
        private int totalHits;

        public TaikoScoreProcessor()
        {
        }

        public TaikoScoreProcessor(HitRenderer<TaikoHitObject, TaikoJudgement> hitRenderer)
            : base(hitRenderer)
        {
        }

        protected override void ComputeTargets(Beatmap<TaikoHitObject> beatmap)
        {
            double hpMultiplierNormal = 1 / (hp_hit_great * beatmap.HitObjects.FindAll(o => o is Hit).Count * BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.Difficulty.DrainRate, 0.5, 0.75, 0.98));

            hpIncreaseTick = hp_hit_tick;
            hpIncreaseGreat = hpMultiplierNormal * hp_hit_great;
            hpIncreaseGood = hpMultiplierNormal * hp_hit_good;
            hpIncreaseMiss = BeatmapDifficulty.DifficultyRange(beatmap.BeatmapInfo.Difficulty.DrainRate, hp_miss_min, hp_miss_mid, hp_miss_max);

            var strongHits = beatmap.HitObjects.FindAll(o => o is Hit && o.IsStrong);

            // This is a linear function that awards:
            // 10 times bonus points for hitting a strong hit object with both keys with 30 strong hit objects in the map
            // 3 times bonus points for hitting a strong hit object with both keys with 120 strong hit objects in the map
            strongHitScale = -7d / 90d * MathHelper.Clamp(strongHits.Count, 30, 120) + 111d / 9d;

            foreach (var obj in beatmap.HitObjects)
            {
                if (obj is Hit)
                {
                    AddJudgement(new TaikoJudgement
                    {
                        Result = HitResult.Hit,
                        TaikoResult = TaikoHitResult.Great,
                        SecondHit = obj.IsStrong
                    });
                }
                else if (obj is DrumRoll)
                {
                    for (int i = 0; i < ((DrumRoll)obj).TotalTicks; i++)
                    {
                        AddJudgement(new TaikoDrumRollTickJudgement
                        {
                            Result = HitResult.Hit,
                            TaikoResult = TaikoHitResult.Great,
                            SecondHit = obj.IsStrong
                        });
                    }

                    AddJudgement(new TaikoJudgement
                    {
                        Result = HitResult.Hit,
                        TaikoResult = TaikoHitResult.Great,
                        SecondHit = obj.IsStrong
                    });
                }
                else if (obj is Swell)
                {
                    AddJudgement(new TaikoJudgement
                    {
                        Result = HitResult.Hit,
                        TaikoResult = TaikoHitResult.Great
                    });
                }
            }

            maxTotalHits = totalHits;
            maxComboPortion = comboPortion;
        }

        protected override void OnNewJudgement(TaikoJudgement judgement)
        {
            bool isTick = judgement is TaikoDrumRollTickJudgement;

            // Don't consider ticks as a type of hit that counts towards map completion
            if (!isTick)
                totalHits++;

            // Apply score changes
            addHitScore(judgement);

            // Apply HP changes
            switch (judgement.Result)
            {
                case HitResult.Miss:
                    // Missing ticks shouldn't drop HP
                    if (!isTick)
                        Health.Value += hpIncreaseMiss;
                    break;
                case HitResult.Hit:
                    switch (judgement.TaikoResult)
                    {
                        case TaikoHitResult.Good:
                            Health.Value += hpIncreaseGood;
                            break;
                        case TaikoHitResult.Great:
                            if (isTick)
                                Health.Value += hpIncreaseTick;
                            else
                                Health.Value += hpIncreaseGreat;
                            break;
                    }
                    break;
            }

            calculateScore();
        }

        protected override void OnJudgementChanged(TaikoJudgement judgement)
        {
            // Apply score changes
            addHitScore(judgement);

            calculateScore();
        }

        private void addHitScore(TaikoJudgement judgement)
        {
            if (judgement.Result != HitResult.Hit)
                return;

            double baseValue = judgement.ResultValueForScore;

            // Add increased score for hitting a strong hit object with the second key
            if (judgement.SecondHit)
                baseValue *= strongHitScale;

            // Add score to portions
            if (judgement is TaikoDrumRollTickJudgement)
                bonusScore += baseValue;
            else
            {
                // A relevance factor that needs to be applied to make higher combos more relevant
                // Value is capped at 400 combo
                double comboRelevance = Math.Min(Math.Log(400, combo_base), Math.Max(0.5, Math.Log(Combo.Value, combo_base)));

                comboPortion += baseValue * comboRelevance;
            }
        }

        private void calculateScore()
        {
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

            Health.Value = 0;

            bonusScore = 0;
            comboPortion = 0;
            totalHits = 0;
        }
    }
}
