// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoDrumRollJudgement : TaikoJudgement
    {
        public override bool AffectsCombo => false;

        protected override double HealthIncreaseFor(HitResult result)
        {
            // Drum rolls can be ignored with no health penalty
            switch (result)
            {
                case HitResult.Miss:
                    return 0;

                default:
                    return base.HealthIncreaseFor(result);
            }
        }
    }
}
