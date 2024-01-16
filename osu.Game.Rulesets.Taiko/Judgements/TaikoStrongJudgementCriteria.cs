// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoStrongJudgementCriteria : TaikoJudgementCriteria
    {
        public override HitResult MaxResult => HitResult.LargeBonus;

        // MainObject already changes the HP
        protected override double HealthIncreaseFor(HitResult result) => 0;
    }
}
