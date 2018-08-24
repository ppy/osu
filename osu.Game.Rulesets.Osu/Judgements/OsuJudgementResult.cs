// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Osu.Judgements
{
    public class OsuJudgementResult : JudgementResult
    {
        public ComboResult ComboType;

        public OsuJudgementResult(Judgement judgement)
            : base(judgement)
        {
        }
    }
}
