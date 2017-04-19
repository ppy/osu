// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoStrongHitJudgement : TaikoJudgement, IPartialJudgement
    {
        public bool Changed { get; set; }

        public override bool SecondHit
        {
            get { return base.SecondHit; }
            set
            {
                if (base.SecondHit == value)
                    return;
                base.SecondHit = value;

                Changed = true;
            }
        }
    }
}
