// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Structure representing output of <see cref="ISkill"/> processing.
    /// </summary>
    public interface ISkillAttributes
    {
        /// <summary>
        /// Calculated difficulty value.
        /// </summary>
        double Difficulty { get; init; }

        /// <summary>
        /// Difficulty values of <see cref="ISkill.DifficultyHitObjects"/>.
        /// </summary>
        List<double> ObjectDifficulties { get; init; }
    }
}
