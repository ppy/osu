// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Judgements
{
    /// <summary>
    /// A skinnable judgement element which requires the full <see cref="JudgementResult"/>.
    /// </summary>
    public interface IAppliesJudgementResult
    {
        /// <summary>
        /// Associate a result with this judgement element.
        /// </summary>
        void ApplyJudgementResult(JudgementResult result);
    }
}
