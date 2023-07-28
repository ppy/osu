// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mania.Judgements
{
    public class ManiaJudgement : Judgement
    {
        protected override double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.LargeTickHit:
                    return DEFAULT_MAX_HEALTH_INCREASE * 0.1;

                case HitResult.LargeTickMiss:
                    return -DEFAULT_MAX_HEALTH_INCREASE * 0.1;

                case HitResult.Meh:
                    return -DEFAULT_MAX_HEALTH_INCREASE * 0.5;

                case HitResult.Ok:
                    return -DEFAULT_MAX_HEALTH_INCREASE * 0.3;

                case HitResult.Good:
                    return DEFAULT_MAX_HEALTH_INCREASE * 0.1;

                case HitResult.Great:
                    return DEFAULT_MAX_HEALTH_INCREASE * 0.8;

                case HitResult.Perfect:
                    return DEFAULT_MAX_HEALTH_INCREASE;

                default:
                    return base.HealthIncreaseFor(result);
            }
        }

        public static int ToAccuracyWeight(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;

                case HitResult.SmallTickHit:
                    return 10;

                case HitResult.LargeTickHit:
                    return 30;

                case HitResult.Meh:
                    return 50;

                case HitResult.Ok:
                    return 100;

                case HitResult.Good:
                    return 200;

                case HitResult.Great:
                    return 300;

                case HitResult.Perfect:
                    return 300;

                case HitResult.SmallBonus:
                    return SMALL_BONUS_SCORE;

                case HitResult.LargeBonus:
                    return LARGE_BONUS_SCORE;
            }
        }
    }
}
