// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Scoring
{
    public partial class OsuScoreProcessor : ScoreProcessor
    {
        protected override double ClassicScoreMultiplier => 36;

        public OsuScoreProcessor(Ruleset ruleset)
            : base(ruleset)
        {
        }

        protected override double ComputeTotalScore()
        {
            return
                (int)Math.Round
                ((
                    700000 * ComboPortion / MaxComboPortion +
                    300000 * Math.Pow(Accuracy.Value, 10) * ((double)CurrentBasicJudgements / MaxBasicJudgements) +
                    BonusPortion
                ) * ScoreMultiplier);
        }
    }
}
