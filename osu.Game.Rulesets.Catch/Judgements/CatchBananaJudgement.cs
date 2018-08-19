// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Judgements
{
    public class CatchBananaJudgement : CatchJudgement
    {
        public override bool AffectsCombo => false;

        protected override int NumericResultFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case HitResult.Perfect:
                    return 1100;
            }
        }

        protected override float HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case HitResult.Perfect:
                    return 8;
            }
        }

        public override bool ShouldExplodeFor(JudgementResult result) => true;
    }
}
