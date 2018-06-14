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
        private readonly Ruleset ruleset;
        private readonly WorkingBeatmap beatmap;

        protected DifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
        {
            this.ruleset = ruleset;
            this.beatmap = beatmap;
        }

        /// <summary>
        /// Calculates the difficulty of the beatmap using a specific mod combination.
        /// </summary>
        /// <param name="mods">The mods that should be applied to the beatmap.</param>
        /// <returns>A structure describing the difficulty of the beatmap.</returns>
        public DifficultyAttributes Calculate(params Mod[] mods)
        {
            beatmap.Mods.Value = mods;
            IBeatmap playableBeatmap = beatmap.GetPlayableBeatmap(ruleset.RulesetInfo);

            var clock = new StopwatchClock();
            mods.OfType<IApplicableToClock>().ForEach(m => m.ApplyToClock(clock));

            return Calculate(playableBeatmap, mods, clock.Rate);
        }

        /// <summary>
        /// Calculates the difficulty of the beatmap using all mod combinations applicable to the beatmap.
        /// </summary>
        /// <returns>A collection of structures describing the difficulty of the beatmap for each mod combination.</returns>
        public IEnumerable<DifficultyAttributes> CalculateAll()
        {
            foreach (var combination in CreateDifficultyAdjustmentModCombinations())
            {
                if (combination is MultiMod multi)
                    yield return Calculate(multi.Mods);
                else
                    yield return Calculate(combination);
            }
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
        /// Calculates the difficulty of a <see cref="Beatmap"/> using a specific <see cref="Mod"/> combination.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> to compute the difficulty for.</param>
        /// <param name="mods">The <see cref="Mod"/>s that should be applied.</param>
        /// <param name="timeRate">The rate of time in <paramref name="beatmap"/>.</param>
        /// <returns>A structure containing the difficulty attributes.</returns>
        protected abstract DifficultyAttributes Calculate(IBeatmap beatmap, Mod[] mods, double timeRate);
    }
}
