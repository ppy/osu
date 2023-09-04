// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Scoring
{
    public partial class CatchScoreProcessor : ScoreProcessor
    {
        private const int combo_cap = 200;
        private const double combo_base = 4;

        public CatchScoreProcessor()
            : base(new CatchRuleset())
        {
        }

        protected override double ComputeTotalScore(double comboProgress, double accuracyProgress, double bonusPortion)
        {
            return 600000 * comboProgress
                   + 400000 * Accuracy.Value * accuracyProgress
                   + bonusPortion;
        }

        protected override double GetComboScoreChange(JudgementResult result)
            => Judgement.ToNumericResult(result.Type) * Math.Min(Math.Max(0.5, Math.Log(result.ComboAfterJudgement, combo_base)), Math.Log(combo_cap, combo_base));
    }
}
