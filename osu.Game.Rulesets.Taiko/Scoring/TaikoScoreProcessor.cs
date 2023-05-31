﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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

        protected override double ComputeTotalScore(double comboProgress, double accuracyProgress, double bonusPortion)
        {
            return 250000 * comboProgress
                   + 750000 * Math.Pow(Accuracy.Value, 3.6) * accuracyProgress
                   + bonusPortion;
        }

        protected override double GetBonusScoreChange(JudgementResult result) => base.GetBonusScoreChange(result) * strongScaleValue(result);

        protected override double GetComboScoreChange(JudgementResult result)
        {
            return Judgement.ToNumericResult(result.Type)
                   * Math.Min(Math.Max(0.5, Math.Log(result.ComboAfterJudgement, combo_base)), Math.Log(400, combo_base))
                   * strongScaleValue(result);
        }

        private double strongScaleValue(JudgementResult result)
        {
            if (result.HitObject is StrongNestedHitObject strong)
                return strong.Parent is DrumRollTick ? 3 : 7;

            return 1;
        }
    }
}
