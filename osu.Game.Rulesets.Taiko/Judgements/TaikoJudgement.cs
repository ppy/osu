// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Taiko.Judgements
{
    public class TaikoJudgement : Judgement
    {
        public override HitResult MaxResult => HitResult.Great;

        protected override int NumericResultFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Good:
                    return 100;
                case HitResult.Great:
                    return 300;
                default:
                    return 0;
            }
        }

        protected override double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Miss:
                    return -1.0;
                case HitResult.Good:
                    return 1.1;
                case HitResult.Great:
                    return 3.0;
                default:
                    return 0;
            }
        }
    }
}
