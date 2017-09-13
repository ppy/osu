// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoStrongHitJudgement : TaikoJudgement
    {
        public override bool AffectsCombo => false;

        public TaikoStrongHitJudgement()
        {
            base.Result = HitResult.Perfect;
        }

        public new HitResult Result => base.Result;
    }
}
