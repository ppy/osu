// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets
{
    public interface ILegacyRuleset
    {
        const int MAX_LEGACY_RULESET_ID = 3;

        /// <summary>
        /// Identifies the server-side ID of a legacy ruleset.
        /// </summary>
        int LegacyID { get; }

        /// <summary>
        /// Creates an object that is able to simulate the maximum legacy scoring values possible on a beatmap.
        /// </summary>
        ILegacyScoreSimulator CreateLegacyScoreSimulator();

        /// <summary>
        /// Returns the legacy score multiplier for the mods. This is only used during legacy score conversion.
        /// </summary>
        /// <param name="mods">The mods.</param>
        /// <param name="difficulty">Extra difficulty parameters.</param>
        /// <returns>The legacy multiplier.</returns>
        double GetLegacyScoreMultiplier(IReadOnlyList<Mod> mods, LegacyBeatmapConversionDifficultyInfo difficulty);
    }
}
