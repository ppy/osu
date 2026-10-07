// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// Skill difficulty calculation class.
    /// </summary>
    public interface ISkill
    {
        /// <summary>
        /// Mods for use in skill calculations.
        /// </summary>
        IReadOnlyList<Mod> Mods { get; init; }

        /// <summary>
        /// <see cref="DifficultyHitObject"/>s for use in skill calculations.
        /// </summary>
        IReadOnlyList<DifficultyHitObject> DifficultyHitObjects { get; init; }

        /// <summary>
        /// Processes <see cref="DifficultyHitObjects"/> and calculates <see cref="ISkillAttributes"/> with relevant information to calculate <see cref="DifficultyAttributes"/>.
        /// </summary>
        ISkillAttributes Process();

        /// <summary>
        /// Processes <see cref="DifficultyHitObjects"/> and calculates an array of <see cref="TimedSkillAttributes"/> that can be then used to calculate <see cref="TimedDifficultyAttributes"/>.
        /// </summary>
        IEnumerable<TimedSkillAttributes> ProcessTimed();
    }
}
