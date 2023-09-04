// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Beatmaps;

namespace osu.Game.Rulesets.Scoring.Legacy
{
    /// <summary>
    /// Generates attributes which are required to calculate old-style Score V1 scores.
    /// </summary>
    public interface ILegacyScoreSimulator
    {
        /// <summary>
        /// Performs the simulation, scoring values achievable for the given beatmap.
        /// </summary>
        /// <param name="workingBeatmap">The working beatmap.</param>
        /// <param name="playableBeatmap">A playable version of the beatmap for the ruleset.</param>
        LegacyScoreAttributes Simulate(IWorkingBeatmap workingBeatmap, IBeatmap playableBeatmap);
    }
}
