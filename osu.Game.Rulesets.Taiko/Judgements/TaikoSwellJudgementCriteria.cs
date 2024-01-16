// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoSwellJudgementCriteria : TaikoJudgementCriteria
    {
        public override HitResult MaxResult => HitResult.LargeBonus;

        protected override double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.IgnoreMiss:
                    return -0.65;

                default:
                    return 0;
            }
        }
    }
}
