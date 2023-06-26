// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Scoring
{
    public interface ILegacyScoreProcessor
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

        void Simulate(IWorkingBeatmap workingBeatmap, IBeatmap playableBeatmap, IReadOnlyList<Mod> mods);
    }
}
