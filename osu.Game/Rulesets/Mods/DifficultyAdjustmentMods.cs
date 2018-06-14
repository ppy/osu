// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
using System.Linq;

namespace osu.Game.Rulesets.Mods
{
    /// <summary>
    /// A provider of <see cref="Mod"/> combinations that affect the <see cref="Beatmap"/> difficulty for this <see cref="Ruleset"/>.
    /// </summary>
    public class DifficultyAdjustmentMods
    {
        /// <summary>
        /// Creates all <see cref="Mod"/> combinations which adjust the <see cref="Beatmap"/> difficulty.
        /// This may return either singular <see cref="Mod"/>s or combinations as <see cref="MultiMod"/>s.
        /// </summary>
        public Mod[] CreateCombinations()
        {
            return createDifficultyAdjustmentModCombinations(Enumerable.Empty<Mod>(), Mods).ToArray();

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
        protected virtual Mod[] Mods => Array.Empty<Mod>();
    }
}
