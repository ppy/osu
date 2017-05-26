// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using osu.Game.Beatmaps;
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

        private double maxComboPortion;
        private double comboPortion;
        private int maxTotalHits;
        private int totalHits;

        private double hpIncreaseBad;
        private double hpIncreaseOk;
        private double hpIncreaseGood;
        private double hpIncreaseGreat;
        private double hpIncreasePerfect;
        private double hpIncreaseTick;
        private double hpIncreaseTickMiss;
        private double hpIncreaseMiss;

        public ManiaScoreProcessor()
        {
        }

        public ManiaScoreProcessor(HitRenderer<ManiaHitObject, ManiaJudgement> hitRenderer)
            : base(hitRenderer)
        {
        }

        protected override void ComputeTargets(Beatmap<ManiaHitObject> beatmap)
        {
            foreach (var obj in beatmap.HitObjects)
            {
                if (obj is Note)
                {
                    AddJudgement(new ManiaJudgement
                    {
                        Result = HitResult.Hit,
                        ManiaResult = ManiaHitResult.Perfect
                    });
                }
                else if (obj is HoldNote)
                {
                    // Head
                    AddJudgement(new ManiaJudgement
                    {
                        Result = HitResult.Hit,
                        ManiaResult = ManiaJudgement.MAX_HIT_RESULT
                    });

                    // Ticks
                    for (int i = 0; i < ((HoldNote)obj).TotalTicks; i++)
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
                    if (isTick)
                        Health.Value += hpIncreaseTickMiss;
                    else
                        Health.Value += hpIncreaseMiss;
                    break;
                case HitResult.Hit:
                    if (isTick)
                    {
                        Health.Value += hpIncreaseTick;
                        bonusScore += judgement.ResultValueForScore;
                    }
                    else
                    {
                        switch (judgement.ManiaResult)
                        {
                            case ManiaHitResult.Bad:
                                Health.Value += hpIncreaseBad;
                                break;
                            case ManiaHitResult.Ok:
                                Health.Value += hpIncreaseOk;
                                break;
                            case ManiaHitResult.Good:
                                Health.Value += hpIncreaseGood;
                                break;
                            case ManiaHitResult.Great:
                                Health.Value += hpIncreaseGreat;
                                break;
                            case ManiaHitResult.Perfect:
                                Health.Value += hpIncreasePerfect;
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
