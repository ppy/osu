// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Taiko.Judgements;

namespace osu.Game.Rulesets.Taiko.Objects
{
    public class Hit : TaikoHitObject
    {
        protected override IEnumerable<Judgement> CreateJudgements()
        {
            yield return new TaikoJudgement();

            if (IsStrong)
                yield return new TaikoStrongHitJudgement();
        }
    }
}
