// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Represents a mod which can override (and block) a fail.
    /// </summary>
    public interface IApplicableFailOverride : IApplicableMod
    {
        /// <summary>
        /// Whether we should allow failing at the current point in time.
        /// </summary>
        /// <returns>Whether the fail should be allowed to proceed. Return false to block.</returns>
        bool PerformFail();

        /// <summary>
        /// Whether we want to restart on fail. Only used if <see cref="PerformFail"/> returns true.
        /// </summary>
        bool RestartOnFail { get; }
    }
}
