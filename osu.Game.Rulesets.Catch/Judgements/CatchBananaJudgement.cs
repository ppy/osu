// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Judgements
{
    public class CatchBananaJudgement : CatchJudgement
    {
        public override bool AffectsCombo => false;

        protected override int NumericResultFor(HitResult result)
            => result switch
            {
                HitResult.Perfect => 1100,
                _ => 0,
            };

        protected override double HealthIncreaseFor(HitResult result)
            => result switch
            {
                HitResult.Perfect => 0.008,
                _ => 0,
            };

        public override bool ShouldExplodeFor(JudgementResult result) => true;
    }
}
