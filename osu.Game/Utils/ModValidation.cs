// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Extensions.TypeExtensions;
using osu.Game.Rulesets.Mods;

#nullable enable

namespace osu.Game.Utils
{
    /// <summary>
    /// A set of utilities to validate <see cref="Mod"/> combinations.
    /// </summary>
    public static class ModValidation
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
        public static bool CheckCompatibleAndAllowed(IEnumerable<Mod> combination, IEnumerable<Type> allowedTypes)
        {
            // Prevent multiple-enumeration.
            var combinationList = combination as ICollection<Mod> ?? combination.ToArray();
            return CheckCompatible(combinationList) && CheckAllowed(combinationList, allowedTypes);
        }

        /// <summary>
        /// Checks that all <see cref="Mod"/>s in a combination are compatible with each-other.
        /// </summary>
        /// <param name="combination">The <see cref="Mod"/> combination to check.</param>
        /// <returns>Whether all <see cref="Mod"/>s in the combination are compatible with each-other.</returns>
        public static bool CheckCompatible(IEnumerable<Mod> combination)
        {
            var incompatibleTypes = new HashSet<Type>();
            var incomingTypes = new HashSet<Type>();

            foreach (var mod in combination.SelectMany(flattenMod))
            {
                // Add the new mod incompatibilities, checking whether any match the existing mod types.
                foreach (var t in mod.IncompatibleMods)
                {
                    if (incomingTypes.Contains(t))
                        return false;

                    incompatibleTypes.Add(t);
                }

                // Add the new mod types, checking whether any match the incompatible types.
                foreach (var t in mod.GetType().EnumerateBaseTypes())
                {
                    if (incomingTypes.Contains(t))
                        return false;

                    incomingTypes.Add(t);
                }
            }

            return true;
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

            return combination.SelectMany(flattenMod)
                              .All(m => allowedSet.Contains(m.GetType()));
        }

        /// <summary>
        /// Determines whether a <see cref="Mod"/> is in a set of incompatible types.
        /// </summary>
        /// <remarks>
        /// A <see cref="Mod"/> can be incompatible through its most-declared type or any of its base types.
        /// </remarks>
        /// <param name="mod">The <see cref="Mod"/> to test.</param>
        /// <param name="incompatibleTypes">The set of incompatible <see cref="Mod"/> types.</param>
        /// <returns>Whether the given <see cref="Mod"/> is incompatible.</returns>
        private static bool isModIncompatible(Mod mod, ICollection<Type> incompatibleTypes)
            => flattenMod(mod)
               .SelectMany(m => m.GetType().EnumerateBaseTypes())
               .Any(incompatibleTypes.Contains);

        /// <summary>
        /// Flattens a <see cref="Mod"/>, returning a set of <see cref="Mod"/>s in-place of any <see cref="MultiMod"/>s.
        /// </summary>
        /// <param name="mod">The <see cref="Mod"/> to flatten.</param>
        /// <returns>A set of singular "flattened" <see cref="Mod"/>s</returns>
        private static IEnumerable<Mod> flattenMod(Mod mod)
        {
            if (mod is MultiMod multi)
            {
                foreach (var m in multi.Mods.SelectMany(flattenMod))
                    yield return m;
            }
            else
                yield return mod;
        }
    }
}
