// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Timing;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;

namespace osu.Game.Rulesets.Difficulty
{
    public abstract class DifficultyCalculator
    {
        /// <summary>
        /// The length of each strain section.
        /// </summary>
        protected virtual int SectionLength => 400;

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
        public DifficultyAttributes Calculate(params Mod[] mods) => CalculateTimed(mods).Last().Attributes;

        public IEnumerable<TimedDifficultyAttributes> CalculateTimed(params Mod[] mods)
        {
            mods = mods.Select(m => m.CreateCopy()).ToArray();

            IBeatmap playableBeatmap = beatmap.GetPlayableBeatmap(ruleset.RulesetInfo, mods);

            var clock = new StopwatchClock();
            mods.OfType<IApplicableToClock>().ForEach(m => m.ApplyToClock(clock));

            return calculate(playableBeatmap, mods, clock.Rate);
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

        private IEnumerable<TimedDifficultyAttributes> calculate(IBeatmap beatmap, Mod[] mods, double clockRate)
        {
            var skills = CreateSkills(beatmap);

            if (!beatmap.HitObjects.Any())
            {
                yield return new TimedDifficultyAttributes(0, CreateDifficultyAttributes(beatmap, mods, skills, clockRate));

                yield break;
            }

            var difficultyHitObjects = CreateDifficultyHitObjects(beatmap, clockRate).OrderBy(h => h.BaseObject.StartTime).ToList();

            double sectionLength = SectionLength * clockRate;

            // The first object doesn't generate a strain, so we begin with an incremented section end
            double currentSectionEnd = Math.Ceiling(beatmap.HitObjects.First().StartTime / sectionLength) * sectionLength;

            //Generate strain for start section of beatmap
            double startSectionEnd = currentSectionEnd - sectionLength;
            yield return new TimedDifficultyAttributes(startSectionEnd, CreateDifficultyAttributes(beatmap, mods, skills, clockRate, startSectionEnd));

            foreach (DifficultyHitObject h in difficultyHitObjects)
            {
                while (h.BaseObject.StartTime > currentSectionEnd)
                {
                    foreach (Skill s in skills)
                    {
                        s.SaveCurrentPeak();
                        s.StartNewSectionFrom(currentSectionEnd);
                    }

                    yield return new TimedDifficultyAttributes(currentSectionEnd, CreateDifficultyAttributes(beatmap, mods, skills, clockRate, currentSectionEnd));

                    currentSectionEnd += sectionLength;
                }

                foreach (Skill s in skills)
                    s.Process(h);
            }

            // The peak strain will not be saved for the last section in the above loop
            foreach (Skill s in skills)
                s.SaveCurrentPeak();

            yield return new TimedDifficultyAttributes(currentSectionEnd, CreateDifficultyAttributes(beatmap, mods, skills, clockRate));
        }

        /// <summary>
        /// Creates all <see cref="Mod"/> combinations which adjust the <see cref="Beatmap"/> difficulty.
        /// </summary>
        public Mod[] CreateDifficultyAdjustmentModCombinations()
        {
            return createDifficultyAdjustmentModCombinations(Array.Empty<Mod>(), DifficultyAdjustmentMods).ToArray();

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
        /// Creates <see cref="DifficultyAttributes"/> to describe beatmap's calculated difficulty.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> whose difficulty was calculated.</param>
        /// <param name="mods">The <see cref="Mod"/>s that difficulty was calculated with.</param>
        /// <param name="skills">The skills which processed the beatmap.</param>
        /// <param name="clockRate">The rate at which the gameplay clock is run at.</param>
        /// <param name="upTo">Time in ms to with beatmap should be processed</param>
        protected abstract DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, Skill[] skills, double clockRate, double upTo = double.PositiveInfinity);

        /// <summary>
        /// Enumerates <see cref="DifficultyHitObject"/>s to be processed from <see cref="HitObject"/>s in the <see cref="IBeatmap"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> providing the <see cref="HitObject"/>s to enumerate.</param>
        /// <param name="clockRate">The rate at which the gameplay clock is run at.</param>
        /// <returns>The enumerated <see cref="DifficultyHitObject"/>s.</returns>
        protected abstract IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, double clockRate);

        /// <summary>
        /// Creates the <see cref="Skill"/>s to calculate the difficulty of an <see cref="IBeatmap"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> whose difficulty will be calculated.</param>
        /// <returns>The <see cref="Skill"/>s.</returns>
        protected abstract Skill[] CreateSkills(IBeatmap beatmap);
    }
}
