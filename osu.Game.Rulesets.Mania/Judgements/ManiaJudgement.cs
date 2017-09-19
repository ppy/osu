// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;

namespace osu.Game.Rulesets.Mania.Judgements
{
    public class ManiaJudgement : Judgement
    {
        protected override int NumericResultFor(HitResult result)
        {
            switch (result)
            {
                default:
                    return 0;
                case HitResult.Meh:
                    return 50;
                case HitResult.Ok:
                    return 100;
                case HitResult.Good:
                    return 200;
                case HitResult.Great:
                case HitResult.Perfect:
                    return 300;
            }
        }
    }
}
