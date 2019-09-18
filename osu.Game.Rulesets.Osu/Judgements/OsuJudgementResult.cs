// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Osu.Judgements
{
    public class OsuJudgementResult : JudgementResult
    {
        public ComboResult ComboType;

        public OsuJudgementResult(HitObject hitObject, Judgement judgement)
            : base(hitObject, judgement)
        {
        }
    }
}
