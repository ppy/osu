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
    /// <remarks>
    /// This class should be considered a "processing" class and not persisted, as it keeps references to
    /// gameplay objects after processing is run (see <see cref="Previous"/>).
    /// </remarks>
    public abstract class Skill
    {
        /// <summary>
        /// <see cref="DifficultyHitObject"/>s that were processed previously. They can affect the strain values of the following objects.
        /// </summary>
        protected readonly ReverseQueue<DifficultyHitObject> Previous;

        /// <summary>
        /// Number of previous <see cref="DifficultyHitObject"/>s to keep inside the <see cref="Previous"/> queue.
        /// </summary>
        protected virtual int HistoryLength => 1;

        /// <summary>
        /// Mods for use in skill calculations.
        /// </summary>
        protected IReadOnlyList<Mod> Mods => mods;

        private readonly Mod[] mods;

        protected Skill(Mod[] mods)
        {
            this.mods = mods;
            Previous = new ReverseQueue<DifficultyHitObject>(HistoryLength + 1);
        }

        internal void ProcessInternal(DifficultyHitObject current)
        {
            while (Previous.Count > HistoryLength)
                Previous.Dequeue();

            Process(current);

            Previous.Enqueue(current);
        }

        /// <summary>
        /// Process a <see cref="DifficultyHitObject"/>.
        /// </summary>
        /// <param name="current">The <see cref="DifficultyHitObject"/> to process.</param>
        protected abstract void Process(DifficultyHitObject current);

        /// <summary>
        /// Returns the calculated difficulty value representing all <see cref="DifficultyHitObject"/>s that have been processed up to this point.
        /// </summary>
        public abstract double DifficultyValue();
    }
}
