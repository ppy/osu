// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Judgements
{
    public class ManiaJudgement : Judgement
    {
        protected override int NumericResultFor(HitResult result)
            => result switch
            {
                HitResult.Meh => 50,
                HitResult.Ok => 100,
                HitResult.Good => 200,
                HitResult.Great => 300,
                HitResult.Perfect => 300,
                _ => 0,
            };

        protected override double HealthIncreaseFor(HitResult result)
            => result switch
            {
                HitResult.Miss => -0.125,
                HitResult.Meh => 0.005,
                HitResult.Ok => 0.010,
                HitResult.Good => 0.035,
                HitResult.Great => 0.055,
                HitResult.Perfect => 0.065,
                _ => 0,
            };
    }
}
