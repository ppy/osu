// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Scoring
{
    /// <summary>
    /// Calculates the multiplier to be applied to score with a given combination of mods.
    /// </summary>
    public class ScoreMultiplierCalculator
    {
        protected ScoreMultiplierContext Context { get; }

        private readonly List<(Type[] mods, Func<Mod[], double> multiplier)> combinationMultipliers = [];
        private readonly Dictionary<Type, Func<Mod, double>> singleMultipliers = [];

        public ScoreMultiplierCalculator(ScoreMultiplierContext context)
        {
            Context = context;
        }

        /// <summary>
        /// Defines a flat, setting-independent score multiplier for the given <typeparamref name="TMod"/>.
        /// </summary>
        protected void Single<TMod>(double hasMultiplier)
            where TMod : Mod
        {
            singleMultipliers[typeof(TMod)] = _ => hasMultiplier;
        }

        /// <summary>
        /// Defines a setting-dependent score multiplier for the given <typeparamref name="TMod"/>.
        /// </summary>
        protected void Single<TMod>(Func<TMod, double> hasMultiplier)
            where TMod : Mod
        {
            singleMultipliers[typeof(TMod)] = mod => hasMultiplier.Invoke((TMod)mod);
        }

        /// <summary>
        /// Defines a score multiplier specific to when both <typeparamref name="T1"/> and <typeparamref name="T2"/> mods are present.
        /// </summary>
        protected void Combination<T1, T2>(Func<T1, T2, double> hasMultiplier)
            where T1 : Mod
            where T2 : Mod
        {
            combinationMultipliers.Add(([typeof(T1), typeof(T2)], mods => hasMultiplier((T1)mods[0], (T2)mods[1])));
        }

        /// <summary>
        /// Calculates the multiplier to be applied to score with the given <paramref name="mods"/>.
        /// </summary>
        public double CalculateFor(IEnumerable<Mod> mods)
        {
            var allModsByType = mods.ToDictionary(m => m.GetType());

            if (allModsByType.Count == 0)
                return 1;

            var remainingModTypes = allModsByType.Keys.ToHashSet();

            double result = 1;

            if (allModsByType.Count > 1)
            {
                foreach (var (combination, multiplier) in combinationMultipliers)
                {
                    if (remainingModTypes.IsSupersetOf(combination))
                    {
                        var instances = combination.Select(t => allModsByType[t]).ToArray();
                        result *= multiplier(instances);
                        remainingModTypes.ExceptWith(combination);
                    }
                }
            }

            foreach (var modType in remainingModTypes)
            {
                if (singleMultipliers.TryGetValue(modType, out var multiplier))
                    result *= multiplier(allModsByType[modType]);
            }

            return result;
        }
    }

    /// <summary>
    /// Contextual information to pass to a <see cref="ScoreMultiplierContext"/>
    /// in order for it to calculate the correct multiplier.
    /// </summary>
    public class ScoreMultiplierContext
    {
        /// <summary>
        /// The difficulty info for the beatmap that the multipliers are calculated for.
        /// This must be the difficulty info for the beatmap BEFORE any mod application.
        /// </summary>
        public IBeatmapDifficultyInfo BeatmapDifficultyWithoutMods { get; }

        /// <summary>
        /// The score that the multipliers are calculated for.
        /// Mostly relevant and present in backwards compatibility scenarios.
        /// In usages where the current valid score multipliers are required, pass <see langword="null"/> or use a constructor that does not require this.
        /// </summary>
        public ScoreInfo? Score { get; }

        /// <summary>
        /// Constructs a new instance.
        /// </summary>
        /// <param name="beatmapDifficultyWithoutMods">
        /// The difficulty info for the beatmap that the multipliers are calculated for.
        /// This must be the difficulty info for the beatmap BEFORE any mod application.
        /// </param>
        /// <param name="score">
        /// The score that the multipliers are calculated for.
        /// Mostly relevant and present in backwards compatibility scenarios.
        /// In usages where the current valid score multipliers are required, pass <see langword="null"/> or omit this parameter entirely.
        /// </param>
        public ScoreMultiplierContext(IBeatmapDifficultyInfo beatmapDifficultyWithoutMods, ScoreInfo? score = null)
        {
            BeatmapDifficultyWithoutMods = beatmapDifficultyWithoutMods;
            Score = score;
        }
    }
}
