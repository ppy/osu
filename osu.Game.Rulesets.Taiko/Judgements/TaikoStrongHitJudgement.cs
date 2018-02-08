// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoStrongHitJudgement : TaikoJudgement
    {
        public override bool AffectsCombo => false;

        public TaikoStrongHitJudgement()
        {
            Final = true;
        }
    }
}
