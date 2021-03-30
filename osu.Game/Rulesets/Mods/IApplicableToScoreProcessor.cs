// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// An interface for mods that make general adjustments to score processor.
    /// </summary>
    public interface IApplicableToScoreProcessor : IApplicableMod
    {
        /// <summary>
        /// Provide a <see cref="ScoreProcessor"/> to a mod. Called once on initialisation of a play instance.
        /// </summary>
        void ApplyToScoreProcessor(ScoreProcessor scoreProcessor);

        /// <summary>
        /// Called every time a rank calculation is requested. Allows mods to adjust the final rank.
        /// </summary>
        ScoreRank AdjustRank(ScoreRank rank, double accuracy);
    }
}
