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

        protected override double ComputeTotalScore(double comboProgress, double accuracyProgress, double bonusPortion)
        {
            return 10000 * comboProgress
                   + 990000 * Math.Pow(Accuracy.Value, 2 + 2 * Accuracy.Value) * accuracyProgress
                   + bonusPortion;
        }

        protected override double GetComboScoreChange(JudgementResult result)
            => Judgement.ToNumericResult(result.Type) * Math.Min(Math.Max(0.5, Math.Log(result.ComboAfterJudgement, combo_base)), Math.Log(400, combo_base));
    }
}
