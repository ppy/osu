﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        void ApplyToScoreProcessor(ScoreProcessor scoreProcessor);
        
        /// <summary>
        /// Adjusts rank on specific mods, mostly used for S and SS to be S+ and SS+ 
        /// </summary>
        ScoreRank AdjustRank(ScoreRank rank);
    }
}
