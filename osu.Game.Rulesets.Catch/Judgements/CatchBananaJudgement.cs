// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Judgements
{
    public class CatchBananaJudgement : CatchJudgement
    {
        public override HitResult MaxResult => HitResult.LargeBonusHit;

        public override bool AffectsCombo => false;

        protected override int NumericResultFor(HitResult result) => result == MaxResult ? LARGE_BONUS_RESULT : 0;

        protected override double HealthIncreaseFor(HitResult result) => result == MaxResult ? DEFAULT_MAX_HEALTH_INCREASE * 0.75 : 0;

        public override bool ShouldExplodeFor(JudgementResult result) => true;
    }
}
