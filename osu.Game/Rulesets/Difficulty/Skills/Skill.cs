// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// A bare minimal abstract skill for fully custom skill implementations.
    /// </summary>
    /// <remarks>
    /// This class should be considered a "processing" class and not persisted.
    /// </remarks>
    public abstract class Skill
    {
        public IReadOnlyList<double> ObjectDifficulties => objectDifficulties;

        /// <summary>
        /// Mods for use in skill calculations.
        /// </summary>
        protected IReadOnlyList<Mod> Mods => mods;

        private readonly Mod[] mods;
        private readonly List<double> objectDifficulties = new List<double>();

        protected Skill(Mod[] mods)
        {
            this.mods = mods;
        }

        /// <summary>
        /// Process a <see cref="DifficultyHitObject"/>.
        /// </summary>
        /// <param name="current">The <see cref="DifficultyHitObject"/> to process.</param>
        public void Process(DifficultyHitObject current)
        {
            double difficultyValue = ProcessInternal(current);
            objectDifficulties.Add(difficultyValue);
        }

        protected abstract double ProcessInternal(DifficultyHitObject current);

        /// <summary>
        /// Returns the calculated difficulty value representing all <see cref="DifficultyHitObject"/>s that have been processed up to this point.
        /// </summary>
        public abstract double DifficultyValue();
    }
}
