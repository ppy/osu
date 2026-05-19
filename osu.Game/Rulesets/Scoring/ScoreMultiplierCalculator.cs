// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Scoring
{
    /// <summary>
    /// Calculates the multiplier to be applied to score with a given combination of mods.
    /// </summary>
    public class ScoreMultiplierCalculator
    {
        private static readonly List<(Type[] mods, Func<Mod[], double> multiplier)> combination_multipliers = [];
        private static readonly Dictionary<Type, Func<Mod, ScoreMultiplierCalculator, double>> single_multipliers_with_context = [];
        private static readonly Dictionary<Type, Func<Mod, double>> single_multipliers = [];

        /// <summary>
        /// Defines a flat, setting-independent score multiplier for the given <typeparamref name="TMod"/>.
        /// </summary>
        public static void Single<TMod>(double hasMultiplier)
            where TMod : Mod
        {
            single_multipliers[typeof(TMod)] = _ => hasMultiplier;
        }

        /// <summary>
        /// Defines a setting-dependent score multiplier for the given <typeparamref name="TMod"/>.
        /// </summary>
        public static void Single<TMod>(Func<TMod, double> hasMultiplier)
            where TMod : Mod
        {
            single_multipliers[typeof(TMod)] = mod => hasMultiplier.Invoke((TMod)mod);
        }

        /// <summary>
        /// Defines a setting-dependent score multiplier for the given <typeparamref name="TMod"/>.
        /// The multiplier calculation is given additional context to calculate the multiplier via the <typeparamref name="TContext"/> type instance.
        /// </summary>
        public static void Single<TMod, TContext>(Func<TMod, TContext, double> hasMultiplier)
            where TMod : Mod
            where TContext : ScoreMultiplierCalculator
        {
            single_multipliers_with_context[typeof(TMod)] = (mod, context) => hasMultiplier.Invoke((TMod)mod, (TContext)context);
        }

        /// <summary>
        /// Defines a score multiplier specific to when both <typeparamref name="T1"/> and <typeparamref name="T2"/> mods are present.
        /// </summary>
        public static void Combination<T1, T2>(Func<T1, T2, double> hasMultiplier)
            where T1 : Mod
            where T2 : Mod
        {
            combination_multipliers.Add(([typeof(T1), typeof(T2)], mods => hasMultiplier((T1)mods[0], (T2)mods[1])));
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
                foreach (var (combination, multiplier) in combination_multipliers)
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
                if (single_multipliers.TryGetValue(modType, out var multiplier))
                    result *= multiplier(allModsByType[modType]);
                else if (single_multipliers_with_context.TryGetValue(modType, out var multiplierWithContext))
                    result *= multiplierWithContext(allModsByType[modType], this);
            }

            return result;
        }
    }
}
