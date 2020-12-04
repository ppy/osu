// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Audio;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Objects
{
    public class SpinnerBonusTick : SpinnerTick
    {
        public SpinnerBonusTick()
        {
            Samples.Add(new HitSampleInfo("spinnerbonus"));
        }

        public override Judgement CreateJudgement() => new OsuSpinnerBonusTickJudgement();

        public class OsuSpinnerBonusTickJudgement : OsuSpinnerTickJudgement
        {
            public override HitResult MaxResult => HitResult.LargeBonus;
        }
    }
}
