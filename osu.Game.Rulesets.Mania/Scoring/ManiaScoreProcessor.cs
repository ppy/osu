// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Scoring
{
    public partial class ManiaScoreProcessor : ScoreProcessor
    {
        private const double combo_base = 4;

        public ManiaScoreProcessor()
            : base(new ManiaRuleset())
        {
        }

        protected override double ComputeTotalScore()
        {
            double comboRatio = MaxComboPortion > 0 ? ComboPortion / MaxComboPortion : 1;
            double accuracyRatio = MaxBasicJudgements > 0 ? (double)CurrentBasicJudgements / MaxBasicJudgements : 1;

            return (
                200000 * comboRatio +
                800000 * Math.Pow(Accuracy.Value, 2 + 2 * Accuracy.Value) * accuracyRatio +
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
            if (result.Type.IsBonus())
                return (0, Judgement.ToNumericResult(result.Type));

            return (Judgement.ToNumericResult(result.Type) * Math.Min(Math.Max(0.5, Math.Log(result.ComboAfterJudgement, combo_base)), Math.Log(400, combo_base)), 0);
        }
    }
}
