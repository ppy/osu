// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface for mods that force the player to fail.
    /// </summary>
    public interface IForceFail : IApplicableMod
    {
        /// <summary>
        /// Whether to restart on fail.
        /// </summary>
        bool RestartOnFail => false;

        /// <summary>
        /// Check if the player should fail after a given judgement result.
        /// </summary>
        /// <param name="result">The judgement result which should be considered.</param>
        /// <returns>If the player should fail.</returns>
        bool ShouldFail(JudgementResult result);
    }
}
