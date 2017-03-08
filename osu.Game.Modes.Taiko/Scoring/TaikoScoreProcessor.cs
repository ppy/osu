// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects;
using OpenTK;

namespace osu.Game.Modes.Taiko.Scoring
{
    internal class TaikoScoreProcessor : ScoreProcessor
    {
        private const double combo_base = 4;
        private const double hp_hit_300 = 6;
        private const double hp_hit_100 = 2.2;

        /// <summary>
        /// Old osu! applied a scale factor to every HP increase at the very final stage while
        /// it's getting added to the HP bar. I'm not sure why it does this, but let's replicate.
        /// </summary>
        private const double hp_scale_factor = 0.06;

        private const double hp_max = 200;
        private const double combo_portion_ratio = 0.2f;
        private const double accuracy_portion_ratio = 0.8f;

        public override Score GetScore() => new TaikoScore
        {
            TotalScore = TotalScore,
            Combo = Combo,
            MaxCombo = HighestCombo,
            Accuracy = Accuracy,
            Health = Health,
        };

        protected override bool ShouldFail => totalHits == maxTotalHits && Health.Value <= 0.5;

        private double comboScore => 1000000 * combo_portion_ratio * comboPortion / maxComboPortion;
        private double accuracyScore => 1000000 * accuracy_portion_ratio * Math.Pow(Accuracy, 3.6) * accurateHits / maxAccurateHits;

        private double bonusScore;

        private double hpIncreaseTick;
        private double hpIncreaseGreat;
        private double hpIncreaseGood;
        private double hpIncreaseMiss;

        private double finisherScoreScale;

        private double maxComboPortion;
        private int maxAccurateHits;
        private int maxTotalHits;

        private double comboPortion;
        private int accurateHits;
        private int totalHits;

        public TaikoScoreProcessor(Beatmap beatmap)
            : base(beatmap)
        {
        }

        protected override void CalculateFinalValues(Beatmap beatmap)
        {
            List<TaikoHitObject> objects = new TaikoConverter().Convert(beatmap);

            double hpMultiplierNormal = 1 / (hp_scale_factor * hp_hit_300 * objects.FindAll(o => (o.Type & TaikoHitType.Hit) > 0).Count * Beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.DrainRate, 0.5, 0.75, 0.98, Mods.None));

            hpIncreaseTick = 0.0001 / hp_max * hp_scale_factor;
            hpIncreaseGreat = hpMultiplierNormal * hp_hit_300 * hp_scale_factor;
            hpIncreaseGood = hpMultiplierNormal * Beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.DrainRate, hp_hit_100 * 8, hp_hit_100, hp_hit_100, Mods.None) * hp_scale_factor;
            hpIncreaseMiss = Beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.DrainRate, -6, -25, -40, Mods.None) / hp_max * hp_scale_factor;

            List<TaikoHitObject> finishers = objects.FindAll(o => (o.Type & TaikoHitType.Hit) > 0 && (o.Type & TaikoHitType.Finisher) > 0);
            finisherScoreScale = -7d / 90d * MathHelper.Clamp(finishers.Count, 30, 120) + 111d / 9d;

            foreach (TaikoHitObject obj in objects)
            {
                if ((obj.Type & TaikoHitType.Hit) > 0)
                {
                    AddJudgement(new TaikoJudgementInfo
                    {
                        Result = HitResult.Hit,
                        Score = TaikoScoreResult.Great,
                        SecondHit = (obj.Type & TaikoHitType.Finisher) > 0
                    });
                }
                else if ((obj.Type & TaikoHitType.DrumRoll) > 0)
                {
                    DrumRoll d = (DrumRoll)obj;

                    for (int i = 0; i < d.TotalTicks; i++)
                    {
                        AddJudgement(new TaikoDrumRollTickJudgementInfo
                        {
                            Result = HitResult.Hit,
                            Score = TaikoScoreResult.Great,
                            SecondHit = (obj.Type & TaikoHitType.Finisher) > 0
                        });
                    }

                    AddJudgement(new TaikoJudgementInfo
                    {
                        Result = HitResult.Hit,
                        Score = TaikoScoreResult.Great,
                        SecondHit = (obj.Type & TaikoHitType.Finisher) > 0
                    });
                }
                else if ((obj.Type & TaikoHitType.Bash) > 0)
                {
                    AddJudgement(new TaikoJudgementInfo
                    {
                        Result = HitResult.Hit,
                        Score = TaikoScoreResult.Great
                    });
                }
            }

            maxTotalHits = totalHits;
            maxComboPortion = comboPortion;
            maxAccurateHits = accurateHits;
        }

        protected override void UpdateCalculations(JudgementInfo judgement)
        {
            if (judgement == null)
                return;

            TaikoJudgementInfo tji = (TaikoJudgementInfo)judgement;

            // Score calculations
            if (judgement.Result == HitResult.Hit)
            {
                double baseValue = tji.ScoreValue;

                if (tji.SecondHit)
                    baseValue += baseValue * finisherScoreScale;

                if (tji is TaikoDrumRollTickJudgementInfo)
                    bonusScore += baseValue;
                else
                {
                    Combo.Value++;
                    accurateHits++;

                    comboPortion += baseValue * Math.Min(Math.Max(0.5, Math.Log(Combo.Value, combo_base)), Math.Log(400, combo_base));
                }
            }

            // Increment total hits counter
            if (!(tji is TaikoDrumRollTickJudgementInfo))
                totalHits++;

            // Hp calculations
            switch (judgement.Result)
            {
                case HitResult.Hit:
                    switch (tji.Score)
                    {
                        case TaikoScoreResult.Great:
                            if (tji is TaikoDrumRollTickJudgementInfo)
                                Health.Value += hpIncreaseTick;
                            else
                                Health.Value += hpIncreaseGreat;
                            break;
                        case TaikoScoreResult.Good:
                            Health.Value += hpIncreaseGood;
                            break;
                    }
                    break;
                case HitResult.Miss:
                    if (!(tji is TaikoDrumRollTickJudgementInfo))
                        Health.Value += hpIncreaseMiss;
                    break;
            }

            int score = 0;
            int maxScore = 0;

            foreach (var judgementInfo in Judgements)
            {
                TaikoJudgementInfo taikoJudgement = (TaikoJudgementInfo)judgementInfo;

                score += taikoJudgement.AccuracyScoreValue;
                maxScore += taikoJudgement.MaxAccuracyScoreValue;
            }

            Accuracy.Value = (double)score / maxScore;
            TotalScore.Value = comboScore + accuracyScore + bonusScore;
        }

        protected override void Reset()
        {
            base.Reset();

            Health.Value = 0;

            bonusScore = 0;
            comboPortion = 0;
            accurateHits = 0;
            totalHits = 0;
        }
    }
}
