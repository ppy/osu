// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using JetBrains.Annotations;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Difficulty.Skills;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects;
using osu.Game.Utils;

namespace osu.Game.Rulesets.Difficulty
{
    public abstract class DifficultyCalculator
    {
        /// <summary>
        /// The beatmap for which difficulty will be calculated.
        /// </summary>
        protected IBeatmap Beatmap { get; private set; }

        /// <summary>
        /// The working beatmap for which difficulty will be calculated.
        /// </summary>
        protected readonly IWorkingBeatmap WorkingBeatmap;

        private readonly IRulesetInfo ruleset;

        /// <summary>
        /// A yymmdd version which is used to discern when reprocessing is required.
        /// </summary>
        public virtual int Version => 0;

        protected DifficultyCalculator(IRulesetInfo ruleset, IWorkingBeatmap beatmap)
        {
            this.ruleset = ruleset;
            WorkingBeatmap = beatmap;
        }

        /// <summary>
        /// Calculates the difficulty of the beatmap with no mods applied.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A structure describing the difficulty of the beatmap.</returns>
        public DifficultyAttributes Calculate(CancellationToken cancellationToken = default)
            => Calculate(Array.Empty<Mod>(), cancellationToken);

        /// <summary>
        /// Calculates the difficulty of the beatmap using a specific mod combination.
        /// </summary>
        /// <param name="mods">The mods that should be applied to the beatmap.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A structure describing the difficulty of the beatmap.</returns>
        public DifficultyAttributes Calculate([NotNull] IEnumerable<Mod> mods, CancellationToken cancellationToken = default)
        {
            using var timedCancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            if (!cancellationToken.CanBeCanceled)
                cancellationToken = timedCancellationSource.Token;

            cancellationToken.ThrowIfCancellationRequested();

            var playableMods = mods.Select(m => m.DeepClone()).ToArray();

            // ReSharper disable once PossiblyMistakenUseOfCancellationToken
            preProcess(playableMods, cancellationToken);

            var skillAttributes = ProcessSkills(Beatmap, playableMods, cancellationToken);

            return CreateDifficultyAttributes(Beatmap, playableMods, skillAttributes);
        }

        /// <summary>
        /// Calculates the difficulty of the beatmap with no mods applied and returns a set of <see cref="TimedDifficultyAttributes"/> representing the difficulty at every relevant time value in the beatmap.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The set of <see cref="TimedDifficultyAttributes"/>.</returns>
        public List<TimedDifficultyAttributes> CalculateTimed(CancellationToken cancellationToken = default)
            => CalculateTimed(Array.Empty<Mod>(), cancellationToken);

        /// <summary>
        /// Calculates the difficulty of the beatmap using a specific mod combination and returns a set of <see cref="TimedDifficultyAttributes"/> representing the difficulty at every relevant time value in the beatmap.
        /// </summary>
        /// <param name="mods">The mods that should be applied to the beatmap.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The set of <see cref="TimedDifficultyAttributes"/>.</returns>
        public List<TimedDifficultyAttributes> CalculateTimed([NotNull] IEnumerable<Mod> mods, CancellationToken cancellationToken = default)
        {
            using var timedCancellationSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            if (!cancellationToken.CanBeCanceled)
                cancellationToken = timedCancellationSource.Token;

            cancellationToken.ThrowIfCancellationRequested();

            var playableMods = mods.Select(m => m.DeepClone()).ToArray();

            // ReSharper disable once PossiblyMistakenUseOfCancellationToken
            preProcess(playableMods, cancellationToken);

            var attribs = new List<TimedDifficultyAttributes>();

            if (!Beatmap.HitObjects.Any())
                return attribs;

            var timedSkillAttributes = ProcessSkillsTimed(Beatmap, playableMods, cancellationToken);

            foreach (var skillAttributes in timedSkillAttributes)
            {
                attribs.Add(new TimedDifficultyAttributes(skillAttributes.Key, CreateDifficultyAttributes(Beatmap, playableMods, skillAttributes.Value)));
            }

            return attribs;
        }

        /// <summary>
        /// Calculates per-skill difficulty of the beatmap using a specific mod combination.
        /// </summary>
        /// <param name="beatmap">Beatmap to calculate difficulty for.</param>
        /// <param name="playableMods">The mods that have been applied to the beatmap.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>List of <see cref="ISkillAttributes"/>.</returns>
        public virtual List<ISkillAttributes> ProcessSkills(IBeatmap beatmap, Mod[] playableMods, CancellationToken cancellationToken = default)
        {
            var difficultyHitObjects = getDifficultyHitObjects(playableMods);

            var skills = CreateSkills(beatmap, playableMods, difficultyHitObjects.ToArray());
            var skillAttributes = new List<ISkillAttributes>();

            foreach (var skill in skills)
            {
                cancellationToken.ThrowIfCancellationRequested();
                skillAttributes.Add(skill.Process());
            }

            return skillAttributes;
        }

        /// <summary>
        /// Calculates per-skill difficulty of the beatmap using a specific mod combination and returns <see cref="ISkillAttributes"/> at every relevant time value.
        /// </summary>
        /// <param name="beatmap">Beatmap to calculate difficulty for.</param>
        /// <param name="playableMods">The mods that have been applied to the beatmap.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Lists of per-skill <see cref="ISkillAttributes"/> at different time points.</returns>
        public virtual Dictionary<double, IReadOnlyList<ISkillAttributes>> ProcessSkillsTimed(IBeatmap beatmap, Mod[] playableMods, CancellationToken cancellationToken = default)
        {
            var difficultyHitObjects = getDifficultyHitObjects(playableMods);

            var skills = CreateSkills(beatmap, playableMods, difficultyHitObjects.ToArray());
            var timedSkillAttributes = new List<TimedSkillAttributes[]>();

            foreach (var skill in skills)
            {
                cancellationToken.ThrowIfCancellationRequested();
                timedSkillAttributes.Add(skill.ProcessTimed().ToArray());
            }

            // Skills have authority over what times they want to calculate skill attributes at
            // so we want to try to calculate difficulty attributes for every timing point available
            var times = timedSkillAttributes.SelectMany(x => x.Select(y => y.Time)).Distinct().ToList();

            var timedAttributes = new Dictionary<double, IReadOnlyList<ISkillAttributes>>();

            foreach (double time in times)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var attributes = new List<ISkillAttributes>();

                foreach (var attribute in timedSkillAttributes)
                {
                    // Different skills might calculate skill attributes at different times
                    // take the latest skill attributes for every skill to make sure we always pass all  skill attributes to CalculateDifficultyAttributes
                    var closestAttributes = attribute.LastOrDefault(x => x.Time <= time);
                    if (closestAttributes != null)
                        attributes.Add(closestAttributes.Attributes);
                }

                timedAttributes.Add(time, attributes);
            }

            return timedAttributes;
        }

        /// <summary>
        /// Calculates the difficulty of the beatmap using all mod combinations applicable to the beatmap.
        /// </summary>
        /// <remarks>
        /// This can only be used to compute difficulties for legacy mod combinations.
        /// </remarks>
        /// <returns>A collection of structures describing the difficulty of the beatmap for each mod combination.</returns>
        public IEnumerable<DifficultyAttributes> CalculateAllLegacyCombinations(CancellationToken cancellationToken = default)
        {
            var rulesetInstance = ruleset.CreateInstance();

            foreach (var combination in CreateDifficultyAdjustmentModCombinations())
            {
                Mod classicMod = rulesetInstance.CreateMod<ModClassic>();

                var finalCombination = ModUtils.FlattenMod(combination);
                if (classicMod != null)
                    finalCombination = finalCombination.Append(classicMod);

                yield return Calculate(finalCombination.ToArray(), cancellationToken);
            }
        }

        /// <summary>
        /// Retrieves the <see cref="DifficultyHitObject"/>s to calculate against.
        /// </summary>
        private IEnumerable<DifficultyHitObject> getDifficultyHitObjects(Mod[] playableMods) => SortObjects(CreateDifficultyHitObjects(Beatmap, playableMods));

        /// <summary>
        /// Performs required tasks before every calculation.
        /// </summary>
        /// <param name="playableMods"></param>
        /// <param name="cancellationToken">The cancellation token.</param>
        private void preProcess([NotNull] Mod[] playableMods, CancellationToken cancellationToken)
        {
            Beatmap = WorkingBeatmap.GetPlayableBeatmap(ruleset, playableMods, cancellationToken);
        }

        /// <summary>
        /// Sorts a given set of <see cref="DifficultyHitObject"/>s.
        /// </summary>
        /// <param name="input">The <see cref="DifficultyHitObject"/>s to sort.</param>
        /// <returns>The sorted <see cref="DifficultyHitObject"/>s.</returns>
        protected virtual IEnumerable<DifficultyHitObject> SortObjects(IEnumerable<DifficultyHitObject> input)
            => input.OrderBy(h => h.BaseObject.StartTime);

        /// <summary>
        /// Creates all <see cref="Mod"/> combinations which adjust the <see cref="Beatmaps.Beatmap"/> difficulty.
        /// </summary>
        public Mod[] CreateDifficultyAdjustmentModCombinations()
        {
            return createDifficultyAdjustmentModCombinations(DifficultyAdjustmentMods, Array.Empty<Mod>()).ToArray();

            static IEnumerable<Mod> createDifficultyAdjustmentModCombinations(ReadOnlyMemory<Mod> remainingMods, IEnumerable<Mod> currentSet, int currentSetCount = 0)
            {
                // Return the current set.
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

                // Apply the rest of the remaining mods recursively.
                for (int i = 0; i < remainingMods.Length; i++)
                {
                    (var nextSet, int nextCount) = flatten(remainingMods.Span[i]);

                    // Check if any mods in the next set are incompatible with any of the current set.
                    if (currentSet.SelectMany(m => m.IncompatibleMods).Any(c => nextSet.Any(c.IsInstanceOfType)))
                        continue;

                    // Check if any mods in the next set are the same type as the current set. Mods of the exact same type are not incompatible with themselves.
                    if (currentSet.Any(c => nextSet.Any(n => c.GetType() == n.GetType())))
                        continue;

                    // If all's good, attach the next set to the current set and recurse further.
                    foreach (var combo in createDifficultyAdjustmentModCombinations(remainingMods.Slice(i + 1), currentSet.Concat(nextSet), currentSetCount + nextCount))
                        yield return combo;
                }
            }

            // Flattens a mod hierarchy (through MultiMod) as an IEnumerable<Mod>
            static (IEnumerable<Mod> set, int count) flatten(Mod mod)
            {
                if (!(mod is MultiMod multi))
                    return (mod.Yield(), 1);

                IEnumerable<Mod> set = Enumerable.Empty<Mod>();
                int count = 0;

                foreach (var nested in multi.Mods)
                {
                    (var nestedSet, int nestedCount) = flatten(nested);
                    set = set.Concat(nestedSet);
                    count += nestedCount;
                }

                return (set, count);
            }
        }

        /// <summary>
        /// Retrieves all <see cref="Mod"/>s which adjust the <see cref="Beatmaps.Beatmap"/> difficulty.
        /// </summary>
        protected virtual Mod[] DifficultyAdjustmentMods => Array.Empty<Mod>();

        /// <summary>
        /// Creates <see cref="DifficultyAttributes"/> to describe beatmap's calculated difficulty.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> whose difficulty was calculated.
        /// This may differ from <see cref="Beatmap"/> in the case of timed calculation.</param>
        /// <param name="mods">The <see cref="Mod"/>s that difficulty was calculated with.</param>
        /// <param name="skills">The skills which processed the beatmap.</param>
        protected abstract DifficultyAttributes CreateDifficultyAttributes(IBeatmap beatmap, Mod[] mods, IReadOnlyList<ISkillAttributes> skills);

        /// <summary>
        /// Enumerates <see cref="DifficultyHitObject"/>s to be processed from <see cref="HitObject"/>s in the <see cref="IBeatmap"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> providing the <see cref="HitObject"/>s to enumerate.</param>
        /// <param name="mods">Mods to create difficulty objects with.</param>
        /// <returns>The enumerated <see cref="DifficultyHitObject"/>s.</returns>
        protected abstract IEnumerable<DifficultyHitObject> CreateDifficultyHitObjects(IBeatmap beatmap, Mod[] mods);

        /// <summary>
        /// Creates the <see cref="ISkill"/>s to calculate the difficulty of an <see cref="IBeatmap"/>.
        /// </summary>
        /// <param name="beatmap">The <see cref="IBeatmap"/> whose difficulty will be calculated.
        /// This may differ from <see cref="Beatmap"/> in the case of timed calculation.</param>
        /// <param name="mods">Mods to calculate difficulty with.</param>
        /// <param name="difficultyHitObjects"></param>
        /// <returns>The <see cref="ISkill"/>s.</returns>
        protected abstract ISkill[] CreateSkills(IBeatmap beatmap, Mod[] mods, DifficultyHitObject[] difficultyHitObjects);
    }
}
