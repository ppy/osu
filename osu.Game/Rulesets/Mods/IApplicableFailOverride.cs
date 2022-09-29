// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

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
        /// <returns> Whether the fail need to proceed. Return <see cref="FailType"/> to decide whether to fail.</returns>
        FailType PerformFail();

        /// <summary>
        /// Whether we want to restart on fail. Only used if occur fail.
        /// </summary>
        bool RestartOnFail { get; }
    }

    public enum FailType
    {
        /// <summary>
        /// Players will never fail even have ForceFail or AllowFail.
        /// Aims for certain mods <see cref="ModBlockFail"/> that don't allow the player to fail.
        /// </summary>
        BlockFail,

        /// <summary>
        /// Player will fail when <see cref="HealthProcessor"/> handle a fail anyway, will override by BlockFail.
        /// Aims for certain mods <see cref="ModFailCondition"/> that change fai condition and will not override by AvoidFail.
        /// </summary>
        ForceFail,

        /// <summary>
        /// Player will not fail when <see cref="HealthProcessor"/> handle a fail, will override by ForceFail.
        /// Aims not to overwrite when certain mods change fail conditions like <see cref="ModFailCondition"/>.
        /// </summary>
        AvoidFail,

        /// <summary>
        /// Player will fail when <see cref="HealthProcessor"/> handle a fail.
        /// </summary>
        AllowFail
    }
}
