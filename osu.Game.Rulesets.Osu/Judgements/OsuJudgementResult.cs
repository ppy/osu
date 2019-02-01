// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
