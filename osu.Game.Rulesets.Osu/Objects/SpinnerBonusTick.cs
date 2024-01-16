// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SpinnerBonusTick : SpinnerTick
    {
        public override JudgementInfo CreateJudgement() => new OsuSpinnerBonusTickJudgementInfo();

        public class OsuSpinnerBonusTickJudgementInfo : OsuSpinnerTickJudgementInfo
        {
            public override HitResult MaxResult => HitResult.LargeBonus;
        }
    }
}
