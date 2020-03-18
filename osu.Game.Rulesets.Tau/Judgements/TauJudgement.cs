// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Tau.Judgements
{
    public class TauJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Perfect;

        protected override int NumericResultFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;

                case HitResult.Great:
                    return 100;

                case HitResult.Perfect:
                    return 300;
            }
        }

        protected override double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;

                case HitResult.Miss:
                    return -0.2;

                case HitResult.Great:
                    return 0.1;

                case HitResult.Perfect:
                    return 0.2;
            }
        }
    }
}
