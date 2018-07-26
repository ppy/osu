// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Catch.Judgements
{
    public class CatchBananaShowerJudgement : CatchJudgement
    {
        public override bool AffectsCombo => false;
        public override bool Required => true;

        protected override int NumericResultFor(HitResult result) => 0;
    }
}
