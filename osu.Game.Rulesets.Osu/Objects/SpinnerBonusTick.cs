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
            Samples.Add(new HitSampleInfo { Name = "spinnerbonus" });
        }

        public override Judgement CreateJudgement() => new OsuSpinnerBonusTickJudgement();

        public class OsuSpinnerBonusTickJudgement : OsuSpinnerTickJudgement
        {
            protected override int NumericResultFor(HitResult result) => 1100;

            protected override double HealthIncreaseFor(HitResult result) => base.HealthIncreaseFor(result) * 2;
        }
    }
}
