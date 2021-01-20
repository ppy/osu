// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Utils;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty.Skills
{
    /// <summary>
    /// A bare minimal abstract skill for fully custom skill implementations.
    /// </summary>
    public abstract class Skill
    {
        /// <summary>
        /// <see cref="DifficultyHitObject"/>s that were processed previously. They can affect the difficulty values of the following objects.
        /// </summary>
        protected readonly ReverseQueue<DifficultyHitObject> Previous = new ReverseQueue<DifficultyHitObject>(4);

        /// <summary>
        /// Visual mods for use in skill calculations.
        /// </summary>
        protected IReadOnlyList<Mod> Mods => mods;

        private readonly Mod[] mods;

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
            // Preprocessing
            RemoveExtraneousHistory(current);

            // Processing
            Calculate(current);

            // Postprocessing
            Previous.Enqueue(current);
        }

        /// <summary>
        /// Remove objects from <see cref="Previous"/> that are no longer needed for calculations from the current object onwards.
        /// </summary>
        /// <param name="current">The <see cref="DifficultyHitObject"/> to be processed.</param>
        protected virtual void RemoveExtraneousHistory(DifficultyHitObject current)
        {
            // Default implementation to not retain objects
            Previous.Clear();
        }

        /// <summary>
        /// Calculate the difficulty of a <see cref="DifficultyHitObject"/> and update current strain values accordingly.
        /// </summary>
        /// <param name="current">The <see cref="DifficultyHitObject"/> to calculate the difficulty of.</param>
        protected abstract void Calculate(DifficultyHitObject current);

        /// <summary>
        /// Returns the calculated difficulty value representing all <see cref="DifficultyHitObject"/>s that have been processed up to this point.
        /// </summary>
        public abstract double DifficultyValue();
    }
}
