// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using osu.Game.Rulesets.Mods;

#nullable enable

namespace osu.Game.Utils
{
    /// <summary>
    /// A set of utilities to handle <see cref="Mod"/> combinations.
    /// </summary>
    public static class ModUtils
    {
        /// <summary>
        /// Checks that all <see cref="Mod"/>s are compatible with each-other, and that all appear within a set of allowed types.
        /// </summary>
        /// <remarks>
        /// The allowed types must contain exact <see cref="Mod"/> types for the respective <see cref="Mod"/>s to be allowed.
        /// </remarks>
        /// <param name="combination">The <see cref="Mod"/>s to check.</param>
        /// <param name="allowedTypes">The set of allowed <see cref="Mod"/> types.</param>
        /// <returns>Whether all <see cref="Mod"/>s are compatible with each-other and appear in the set of allowed types.</returns>
        public static bool CheckCompatibleSetAndAllowed(IEnumerable<Mod> combination, IEnumerable<Type> allowedTypes)
        {
            // Prevent multiple-enumeration.
            var combinationList = combination as ICollection<Mod> ?? combination.ToArray();
            return CheckCompatibleSet(combinationList, out _) && CheckAllowed(combinationList, allowedTypes);
        }

        /// <summary>
        /// Checks that all <see cref="Mod"/>s in a combination are compatible with each-other.
        /// </summary>
        /// <param name="combination">The <see cref="Mod"/> combination to check.</param>
        /// <returns>Whether all <see cref="Mod"/>s in the combination are compatible with each-other.</returns>
        public static bool CheckCompatibleSet(IEnumerable<Mod> combination)
            => CheckCompatibleSet(combination, out _);

        /// <summary>
        /// Checks that all <see cref="Mod"/>s in a combination are compatible with each-other.
        /// </summary>
        /// <param name="combination">The <see cref="Mod"/> combination to check.</param>
        /// <param name="invalidMods">Any invalid mods in the set.</param>
        /// <returns>Whether all <see cref="Mod"/>s in the combination are compatible with each-other.</returns>
        public static bool CheckCompatibleSet(IEnumerable<Mod> combination, [NotNullWhen(false)] out List<Mod>? invalidMods)
        {
            combination = FlattenMods(combination).ToArray();
            invalidMods = null;

            foreach (var mod in combination)
            {
                foreach (var type in mod.IncompatibleMods)
                {
                    foreach (var invalid in combination.Where(m => type.IsInstanceOfType(m)))
                    {
                        invalidMods ??= new List<Mod>();
                        invalidMods.Add(invalid);
                    }
                }
            }

            return invalidMods == null;
        }

        /// <summary>
        /// Checks that all <see cref="Mod"/>s in a combination appear within a set of allowed types.
        /// </summary>
        /// <remarks>
        /// The set of allowed types must contain exact <see cref="Mod"/> types for the respective <see cref="Mod"/>s to be allowed.
        /// </remarks>
        /// <param name="combination">The <see cref="Mod"/> combination to check.</param>
        /// <param name="allowedTypes">The set of allowed <see cref="Mod"/> types.</param>
        /// <returns>Whether all <see cref="Mod"/>s in the combination are allowed.</returns>
        public static bool CheckAllowed(IEnumerable<Mod> combination, IEnumerable<Type> allowedTypes)
        {
            var allowedSet = new HashSet<Type>(allowedTypes);

            return combination.SelectMany(FlattenMod)
                              .All(m => allowedSet.Contains(m.GetType()));
        }

        /// <summary>
        /// Check the provided combination of mods are valid for a local gameplay session.
        /// </summary>
        /// <param name="mods">The mods to check.</param>
        /// <param name="invalidMods">Invalid mods, if any were found. Can be null if all mods were valid.</param>
        /// <returns>Whether the input mods were all valid. If false, <paramref name="invalidMods"/> will contain all invalid entries.</returns>
        public static bool CheckValidForGameplay(IEnumerable<Mod> mods, out List<Mod>? invalidMods)
        {
            mods = mods.ToArray();

            CheckCompatibleSet(mods, out invalidMods);

            foreach (var mod in mods)
            {
                if (mod.Type == ModType.System || !mod.HasImplementation || mod is MultiMod)
                {
                    invalidMods ??= new List<Mod>();
                    invalidMods.Add(mod);
                }
            }

            return invalidMods == null;
        }

        /// <summary>
        /// Flattens a set of <see cref="Mod"/>s, returning a new set with all <see cref="MultiMod"/>s removed.
        /// </summary>
        /// <param name="mods">The set of <see cref="Mod"/>s to flatten.</param>
        /// <returns>The new set, containing all <see cref="Mod"/>s in <paramref name="mods"/> recursively with all <see cref="MultiMod"/>s removed.</returns>
        public static IEnumerable<Mod> FlattenMods(IEnumerable<Mod> mods) => mods.SelectMany(FlattenMod);

        /// <summary>
        /// Flattens a <see cref="Mod"/>, returning a set of <see cref="Mod"/>s in-place of any <see cref="MultiMod"/>s.
        /// </summary>
        /// <param name="mod">The <see cref="Mod"/> to flatten.</param>
        /// <returns>A set of singular "flattened" <see cref="Mod"/>s</returns>
        public static IEnumerable<Mod> FlattenMod(Mod mod)
        {
            if (mod is MultiMod multi)
            {
                foreach (var m in multi.Mods.SelectMany(FlattenMod))
                    yield return m;
            }
            else
                yield return mod;
        }
    }
}
