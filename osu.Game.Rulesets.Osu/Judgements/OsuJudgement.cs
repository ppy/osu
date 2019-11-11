// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Osu.Judgements
{
    public class OsuJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Great;

        protected override int NumericResultFor(HitResult result)
            => result switch
            {
                HitResult.Meh => 50,
                HitResult.Good => 100,
                HitResult.Great => 300,
                _ => 0,
            };

        protected override double HealthIncreaseFor(HitResult result)
            => result switch
            {
                HitResult.Miss => -0.02,
                HitResult.Meh => 0.01,
                HitResult.Good => 0.01,
                HitResult.Great => 0.01,
                _ => 0,
            };
    }
}
