using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using osu.Game.Modes.Objects.Drawables;
using osu.Game.Modes.Taiko.Objects;
using OpenTK;

namespace osu.Game.Modes.Taiko
{
    class TaikoScoreProcessor : ScoreProcessor
    {
        private const double combo_base = 4;
        private const double hp_hit_300 = 6;
        private const double hp_hit_100 = 2.2;

        private const double hp_max = 200;
        private const double combo_portion_ratio = 0.2f;
        private const double accuracy_portion_ratio = 0.8f;

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

        public TaikoScoreProcessor(int hitObjectCount)
            : base(hitObjectCount)
        {
        }

        public override void Initialize(Beatmap beatmap)
        {
            List<TaikoHitObject> objects = new TaikoConverter().Convert(beatmap);

            double hpMultiplierNormal = 1 / (0.06 * objects.FindAll(o => o is HitCircle).Count * beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.DrainRate, 0.5, 0.75, 0.98, Mods.None));
            hpIncreaseTick = 0.0001 / 200.0;
            hpIncreaseGreat = hpMultiplierNormal * hp_hit_300 / 200.0;
            hpIncreaseGood = hpMultiplierNormal * beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.DrainRate, hp_hit_100 * 8, hp_hit_100, hp_hit_100, Mods.None) / 200.0;
            hpIncreaseMiss = beatmap.MapDifficultyRange(beatmap.BeatmapInfo.BaseDifficulty.DrainRate, -6, -25, -40, Mods.None) / 200.0;

            List<TaikoHitObject> finishers = objects.FindAll(o => (o as HitCircle)?.IsFinisher ?? false);
            finisherScoreScale = -7d / 90d * MathHelper.Clamp(finishers.Count, 30, 120) + 111d / 9d;

            foreach (TaikoHitObject obj in objects)
            {
                if (obj is HitCircle)
                {
                    AddJudgement(new TaikoJudgementInfo()
                    {
                        Result = HitResult.Hit,
                        Score = TaikoScoreResult.Great,
                        SecondHit = obj.IsFinisher
                    });
                }
                else if (obj is DrumRoll)
                {
                    DrumRoll d = obj as DrumRoll;

                    for (int i = 0; i < d.TotalTicks; i++)
                    {
                        AddJudgement(new TaikoDrumRollTickJudgementInfo()
                        {
                            Result = HitResult.Hit,
                            Score = TaikoScoreResult.Great,
                            SecondHit = obj.IsFinisher
                        });
                    }

                    AddJudgement(new TaikoJudgementInfo()
                    {
                        Result = HitResult.Hit,
                        Score = TaikoScoreResult.Great,
                        SecondHit = obj.IsFinisher
                    });
                }
                else if (obj is Bash)
                {
                    AddJudgement(new TaikoJudgementInfo()
                    {
                        Result = HitResult.Hit,
                        Score = TaikoScoreResult.Great
                    });
                }

                totalHits++;
            }

            maxTotalHits = totalHits;
            maxComboPortion = comboPortion;
            maxAccurateHits = accurateHits;
        }

        protected override void UpdateCalculations(JudgementInfo judgement)
        {
            if (judgement == null)
                return;

            TaikoJudgementInfo tji = judgement as TaikoJudgementInfo;

            // Score calculations
            switch (judgement.Result)
            {
                case HitResult.Hit:
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

                    break;
                case HitResult.Miss:
                    Health.Value += hpIncreaseMiss;
                    break;
            }

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
                    Health.Value += hpIncreaseMiss;
                    break;
            }

            // Todo: The following accuracy is wrong
            int score = 0;
            int maxScore = 0;

            foreach (TaikoJudgementInfo j in Judgements)
            {
                score += j.ScoreValue;
                maxScore += j.MaxScoreValue;
            }

            Accuracy.Value = (double)score / maxScore;

            TotalScore.Value = comboScore + accuracyScore + bonusScore;
        }

        public override void Reset()
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
