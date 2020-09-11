// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoDrumRollTickJudgement : TaikoJudgement
    {
        public override HitResult MaxResult => HitResult.SmallBonusHit;

        public override bool AffectsCombo => false;

        protected override int NumericResultFor(HitResult result) => result == MaxResult ? SMALL_BONUS_RESULT : 0;

        protected override double HealthIncreaseFor(HitResult result) => result == MaxResult ? 0.15 : 0;
    }
}
