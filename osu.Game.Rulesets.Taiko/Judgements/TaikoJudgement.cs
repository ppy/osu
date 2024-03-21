// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Great;

        protected override double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Miss:
                    return -1.0;

                case HitResult.Ok:
                    return 1.1;

                case HitResult.Great:
                    return 3.0;

                default:
                    return 0;
            }
        }
    }
}
