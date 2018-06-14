// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty
{
    public abstract class DifficultyCalculator
    {
        protected readonly IBeatmap Beatmap;
        protected readonly Mod[] Mods;

        protected double TimeRate { get; private set; } = 1;

        protected DifficultyCalculator(IBeatmap beatmap, Mod[] mods = null)
        {
            Beatmap = beatmap;
            Mods = mods ?? new Mod[0];

            ApplyMods(Mods);
        }

        protected virtual void ApplyMods(Mod[] mods)
        {
            var clock = new StopwatchClock();
            mods.OfType<IApplicableToClock>().ForEach(m => m.ApplyToClock(clock));
            TimeRate = clock.Rate;
        }

        /// <summary>
        /// Creates all <see cref="Mod"/> combinations which adjust the <see cref="Beatmap"/> difficulty.
        /// </summary>
        public Mod[] CreateDifficultyAdjustmentModCombinations()
        {
            return createDifficultyAdjustmentModCombinations(Enumerable.Empty<Mod>(), DifficultyAdjustmentMods).ToArray();

            IEnumerable<Mod> createDifficultyAdjustmentModCombinations(IEnumerable<Mod> currentSet, Mod[] adjustmentSet, int currentSetCount = 0, int adjustmentSetStart = 0)
            {
                // Initial-case: Empty current set
                if (currentSetCount == 0)
                    yield return new NoModMod();

                if (currentSetCount == 1)
                    yield return currentSet.Single();

                if (currentSetCount > 1)
                    yield return new MultiMod(currentSet.ToArray());

                // Apply mods in the adjustment set recursively. Using the entire adjustment set would result in duplicate multi-mod mod
                // combinations in further recursions, so a moving subset is used to eliminate this effect
                for (int i = adjustmentSetStart; i < adjustmentSet.Length; i++)
                {
                    var adjustmentMod = adjustmentSet[i];
                    if (currentSet.Any(c => c.IncompatibleMods.Any(m => m.IsInstanceOfType(adjustmentMod))))
                        continue;

                    foreach (var combo in createDifficultyAdjustmentModCombinations(currentSet.Append(adjustmentMod), adjustmentSet, currentSetCount + 1, i + 1))
                        yield return combo;
                }
            }
        }

        /// <summary>
        /// Retrieves all <see cref="Mod"/>s which adjust the <see cref="Beatmap"/> difficulty.
        /// </summary>
        protected virtual Mod[] DifficultyAdjustmentMods => Array.Empty<Mod>();

        /// <summary>
        /// Calculates the difficulty of the provided beatmap.
        /// Optionally provides additional information relating to the difficulty of the beatmap.
        /// </summary>
        /// <param name="categoryDifficulty">A provided dictionary in which additional information relating to the difficulty of the beatmap is placed.
        /// May be null if no additional information is required.</param>
        /// <returns>The total difficulty rating of the beatmap.</returns>
        public abstract double Calculate(Dictionary<string, object> categoryDifficulty = null);
    }
}
