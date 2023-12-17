// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// Interface for a <see cref="Mod"/> that provides its own health processor.
    /// </summary>
    public interface IApplicableHealthProcessor
    {
        /// <summary>
        /// Creates the <see cref="HealthProcessor"/>.
        /// </summary>
        HealthProcessor CreateHealthProcessor(double drainStartTime);
    }
}
