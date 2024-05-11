// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Catch.UI;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Catch.Judgements
{
    public class CatchJudgementResult : JudgementResult
    {
        /// <summary>
        /// The catcher animation state prior to this judgement.
        /// </summary>
        public CatcherAnimationState CatcherAnimationState;

        /// <summary>
        /// Whether the catcher was hyper dashing prior to this judgement.
        /// </summary>
        public bool CatcherHyperDash;

        public CatchJudgementResult(HitObject hitObject, Judgement judgement)
            : base(hitObject, judgement)
        {
        }
    }
}
