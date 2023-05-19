// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Taiko.Objects;

namespace osu.Game.Rulesets.Taiko.Scoring
{
    public partial class TaikoScoreProcessor : ScoreProcessor
    {
        private const double combo_base = 4;

        public TaikoScoreProcessor()
            : base(new TaikoRuleset())
        {
        }

        protected override double ComputeTotalScore()
        {
            double comboRatio = MaxComboPortion > 0 ? ComboPortion / MaxComboPortion : 1;
            double accuracyRatio = MaxBasicJudgements > 0 ? (double)CurrentBasicJudgements / MaxBasicJudgements : 1;

            return (
                250000 * comboRatio +
                750000 * Math.Pow(Accuracy.Value, 3.6) * accuracyRatio +
                BonusPortion
            ) * ScoreMultiplier;
        }

        protected override void AddScoreChange(JudgementResult result)
        {
            var change = computeScoreChange(result);
            ComboPortion += change.combo;
            BonusPortion += change.bonus;
        }

        protected override void RemoveScoreChange(JudgementResult result)
        {
            var change = computeScoreChange(result);
            ComboPortion -= change.combo;
            BonusPortion -= change.bonus;
        }

        private (double combo, double bonus) computeScoreChange(JudgementResult result)
        {
            double hitValue = Judgement.ToNumericResult(result.Type);

            if (result.HitObject is StrongNestedHitObject strong)
            {
                double strongBonus = strong.Parent is DrumRollTick ? 3 : 7;
                hitValue *= strongBonus;
            }

            if (result.Type.IsBonus())
                return (0, hitValue);

            return (hitValue * Math.Min(Math.Max(0.5, Math.Log(result.ComboAfterJudgement, combo_base)), Math.Log(400, combo_base)), 0);
        }
    }
}
