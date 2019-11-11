// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Judgements
{
    public class CatchDropletJudgement : CatchJudgement
    {
        protected override int NumericResultFor(HitResult result)
            => result switch
            {
                HitResult.Perfect => 30,
                _ => 0,
            };

        protected override double HealthIncreaseFor(HitResult result)
            => result switch
            {
                HitResult.Perfect => 0.007,
                _ => base.HealthIncreaseFor(result),
            };
    }
}
