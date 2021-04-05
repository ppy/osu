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
        /// <see cref="DifficultyHitObject"/>s that were processed previously. They can affect the strain values of the following objects.
        /// </summary>
        protected readonly LimitedCapacityStack<DifficultyHitObject> Previous = new LimitedCapacityStack<DifficultyHitObject>(2); // Contained objects not used yet

        /// <summary>
        /// Mods for use in skill calculations.
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
        public virtual void Process(DifficultyHitObject current)
        {
            Previous.Push(current);
        }

        /// <summary>
        /// Returns the calculated difficulty value representing all <see cref="DifficultyHitObject"/>s that have been processed up to this point.
        /// </summary>
        public abstract double DifficultyValue();
    }
}
