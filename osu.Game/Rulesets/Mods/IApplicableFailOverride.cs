// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Judgements;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Represents a mod which can override failure, by either hard-blocking it, or forcing it immediately.
    /// </summary>
    public interface IApplicableFailOverride : IApplicableMod
    {
        /// <summary>
        /// Whether we want to restart on fail.
        /// </summary>
        bool RestartOnFail { get; }

        /// <summary>
        /// Check the current failure allowance for this mod.
        /// </summary>
        /// <param name="result">The judgement result which should be considered. Importantly, will be <c>null</c> if a failure has already being triggered.</param>
        /// <returns>The current failure allowance (see <see cref="FailState"/>).</returns>
        FailState CheckFail(JudgementResult? result);
    }

    public enum FailState
    {
        /// <summary>
        /// Failure is being blocked by this mod.
        /// </summary>
        Block,

        /// <summary>
        /// Failure is allowed by this mod (but may be triggered by another mod or base behaviour).
        /// </summary>
        Allow,

        /// <summary>
        /// Failure should be forced immediately.
        /// </summary>
        Force
    }
}
