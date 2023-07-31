// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Scoring
{
    /// <summary>
    /// Generates attributes which are required to calculate old-style Score V1 scores.
    /// </summary>
    public interface ILegacyScoreSimulator
    {
        /// <summary>
        /// The accuracy portion of the legacy (ScoreV1) total score.
        /// </summary>
        int AccuracyScore { get; }

        /// <summary>
        /// The combo-multiplied portion of the legacy (ScoreV1) total score.
        /// </summary>
        int ComboScore { get; }

        /// <summary>
        /// A ratio of <c>new_bonus_score / old_bonus_score</c> for converting the bonus score of legacy scores to the new scoring.
        /// This is made up of all judgements that would be <see cref="HitResult.SmallBonus"/> or <see cref="HitResult.LargeBonus"/>.
        /// </summary>
        double BonusScoreRatio { get; }

        /// <summary>
        /// Performs the simulation, computing the maximum <see cref="AccuracyScore"/>, <see cref="ComboScore"/>,
        /// and <see cref="BonusScoreRatio"/> achievable for the given beatmap.
        /// </summary>
        /// <param name="workingBeatmap">The working beatmap.</param>
        /// <param name="playableBeatmap">A playable version of the beatmap for the ruleset.</param>
        /// <param name="mods">The applied mods.</param>
        void Simulate(IWorkingBeatmap workingBeatmap, IBeatmap playableBeatmap, IReadOnlyList<Mod> mods);
    }
}
