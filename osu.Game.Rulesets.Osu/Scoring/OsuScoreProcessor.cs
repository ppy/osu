// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public partial class OsuScoreProcessor : ScoreProcessor
    {
        public OsuScoreProcessor()
            : base(new OsuRuleset())
        {
        }

        protected override double ComputeTotalScore()
        {
            double comboRatio = MaxComboPortion > 0 ? ComboPortion / MaxComboPortion : 1;
            double accuracyRatio = MaxBasicJudgements > 0 ? (double)CurrentBasicJudgements / MaxBasicJudgements : 1;

            return (
                700000 * comboRatio +
                300000 * Math.Pow(Accuracy.Value, 10) * accuracyRatio +
                BonusPortion
            ) * ScoreMultiplier;
        }
    }
}
