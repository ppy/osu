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
        /// <summary>
        /// Mods for use in skill calculations.
        /// </summary>
        protected IReadOnlyList<Mod> Mods => mods;

        /// <summary>
        /// List of calculated per-object difficulties, populated by Process
        /// </summary>
        protected readonly List<double> ObjectDifficulties = new List<double>();

        private readonly Mod[] mods;

        protected Skill(Mod[] mods)
        {
            this.mods = mods;
        }

        /// <summary>
        /// Process a <see cref="DifficultyHitObject"/>.
        /// </summary>
        /// <param name="current">The <see cref="DifficultyHitObject"/> to process.</param>
        public virtual void Process(DifficultyHitObject current)
        {
            double difficultyValue = ProcessInternal(current);
            ObjectDifficulties.Add(difficultyValue);
        }

        protected abstract double ProcessInternal(DifficultyHitObject current);

        /// <summary>
        /// Returns the calculated difficulty value representing all <see cref="DifficultyHitObject"/>s that have been processed up to this point.
        /// </summary>
        public abstract double DifficultyValue();

        public IReadOnlyList<double> GetObjectDifficulties() => ObjectDifficulties;
    }
}
