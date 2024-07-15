// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface for a <see cref="Mod"/> that specifies its own conditions for failure.
    /// </summary>
    // todo: maybe IHasFailCondition and IApplicableFailOverride should be combined into a single interface.
    public interface IHasFailCondition : IApplicableFailOverride
    {
        /// <summary>
        /// Determines whether <paramref name="result"/> should trigger a failure. Called every time a
        /// judgement is applied to <see cref="HealthProcessor"/>.
        /// </summary>
        /// <param name="result">The latest <see cref="JudgementResult"/>.</param>
        /// <returns>Whether the fail condition has been met.</returns>
        /// <remarks>
        /// This method should only be used to trigger failures based on <paramref name="result"/>
        /// </remarks>
        bool FailCondition(JudgementResult result);
    }
}
