// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Scoring.Legacy
{
    /// <summary>
    /// Generates attributes which are required to calculate old-style Score V1 scores.
    /// </summary>
    public interface ILegacyScoreSimulator
    {
        /// <summary>
        /// Performs the simulation, computing the maximum scoring values achievable for the given beatmap.
        /// </summary>
        /// <param name="workingBeatmap">The working beatmap.</param>
        /// <param name="playableBeatmap">A playable version of the beatmap for the ruleset.</param>
        LegacyScoreAttributes Simulate(IWorkingBeatmap workingBeatmap, IBeatmap playableBeatmap);

        /// <summary>
        /// Returns the legacy score multiplier for the mods. This is only used during legacy score conversion.
        /// </summary>
        /// <param name="mods">The mods.</param>
        /// <param name="difficulty">Extra difficulty parameters.</param>
        /// <returns>The legacy multiplier.</returns>
        double GetLegacyScoreMultiplier(IReadOnlyList<Mod> mods, LegacyBeatmapConversionDifficultyInfo difficulty);
    }
}
