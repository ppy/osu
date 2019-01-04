// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoStrongJudgement : TaikoJudgement
    {
        // MainObject already changes the HP
        protected override double HealthIncreaseFor(HitResult result) => 0;

        public override bool AffectsCombo => false;
    }
}
