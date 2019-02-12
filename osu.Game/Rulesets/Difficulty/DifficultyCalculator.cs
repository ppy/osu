// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Difficulty
{
    public abstract class DifficultyCalculator : LegacyDifficultyCalculator
    {
        /// <summary>
        /// The length of each strain section.
        /// </summary>
        protected virtual int SectionLength => 400;

        protected DifficultyCalculator(Ruleset ruleset, WorkingBeatmap beatmap)
            : base(ruleset, beatmap)
        {
        }

        protected override DifficultyAttributes Calculate(IBeatmap beatmap, Mod[] mods, double timeRate)
        {
            var attributes = CreateDifficultyAttributes(mods);

            if (!beatmap.HitObjects.Any())
                return attributes;

            var difficultyHitObjects = CreateDifficultyHitObjects(beatmap, timeRate).OrderBy(h => h.BaseObject.StartTime).ToList();
            var skills = CreateSkills();

            double sectionLength = SectionLength * timeRate;

            // The first object doesn't generate a strain, so we begin with an incremented section end
            double currentSectionEnd = Math.Ceiling(beatmap.HitObjects.First().StartTime / sectionLength) * sectionLength;

            foreach (DifficultyHitObject h in difficultyHitObjects)
            {
                while (h.BaseObject.StartTime > currentSectionEnd)
                {
                    foreach (Skill s in skills)
                    {
                        s.SaveCurrentPeak();
                        s.StartNewSectionFrom(currentSectionEnd);
                    }

                    currentSectionEnd += sectionLength;
                }

                foreach (Skill s in skills)
                    s.Process(h);
            }

            // The peak strain will not be saved for the last section in the above loop
            foreach (Skill s in skills)
                s.SaveCurrentPeak();

            PopulateAttributes(attributes, beatmap, skills, timeRate);

            return attributes;
        }

        /// <summary>
        /// Creates all <see cref="Mod"/> combinations which adjust the <see cref="Beatmap"/> difficulty.
        /// </summary>
        public Mod[] CreateDifficultyAdjustmentModCombinations()
        {
            return createDifficultyAdjustmentModCombinations(Enumerable.Empty<Mod>(), DifficultyAdjustmentMods).ToArray();

            IEnumerable<Mod> createDifficultyAdjustmentModCombinations(IEnumerable<Mod> currentSet, Mod[] adjustmentSet, int currentSetCount = 0, int adjustmentSetStart = 0)
            {
                switch (currentSetCount)
                {
                    case 0:
                        // Initial-case: Empty current set
                        yield return new ModNoMod();
                        break;
                    case 1:
                        yield return currentSet.Single();
                        break;
                    default:
                        yield return new MultiMod(currentSet.ToArray());
                        break;
                }

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
        /// Populates <see cref="DifficultyAttributes"/> after difficulty has been processed.
        /// </summary>
        /// <param name="attributes">The <see cref="DifficultyAttributes"/> to populate with information about the difficulty of <paramref name="beatmap"/>.</param>
        /// <param name="beatmap">The <see cref="IBeatmap"/> whose difficulty was processed.</param>
        /// <param name="skills">The skills which processed the difficulty.</param>
        /// <param name="timeRate">The rate of time in <paramref name="beatmap"/>.</param>
        protected abstract void PopulateAttributes(DifficultyAttributes attributes, IBeatmap beatmap, Skill[] skills, double timeRate);

        /// <summary>
        /// Enumerates <see cref="DifficultyHitObject"/>s to be processed from <see cref="HitObject"/>s in the <see cref="IBeatmap"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> providing the <see cref="HitObject"/>s to enumerate.</param>
        /// <param name="timeRate">The rate of time in <paramref name="beatmap"/>.</param>
        /// <returns>The enumerated <see cref="DifficultyHitObject"/>s.</returns>
        protected abstract IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double timeRate);

        /// <summary>
        /// Creates the <see cref="Skill"/>s to calculate the difficulty of <see cref="DifficultyHitObject"/>s.
        /// </summary>
        /// <returns>The <see cref="Skill"/>s.</returns>
        protected abstract Skill[] CreateSkills();

        /// <summary>
        /// Creates an empty <see cref="DifficultyAttributes"/>.
        /// </summary>
        /// <param name="mods">The <see cref="Mod"/>s which difficulty is being processed with.</param>
        /// <returns>The empty <see cref="DifficultyAttributes"/>.</returns>
        protected abstract DifficultyAttributes CreateDifficultyAttributes(Mod[] mods);
    }
}
