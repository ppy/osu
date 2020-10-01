﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoDrumRollTickJudgement : TaikoJudgement
    {
        public override HitResult MaxResult => HitResult.SmallTickHit;

        protected override double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                    return 0.15;

                default:
                    return 0;
            }
        }
    }
}
