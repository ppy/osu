// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;

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
            var mods = FlattenMods(combination).ToArray();
            invalidMods = null;

            // ensure there are no duplicate mod definitions.
            for (int i = 0; i < mods.Length; i++)
            {
                var candidate = mods[i];

                for (int j = i + 1; j < mods.Length; j++)
                {
                    var m = mods[j];

                    if (candidate.Equals(m))
                    {
                        invalidMods ??= new List<Mod>();
                        invalidMods.Add(m);
                    }
                }
            }

            foreach (var mod in mods)
            {
                foreach (var type in mod.IncompatibleMods)
                {
                    foreach (var invalid in mods.Where(m => type.IsInstanceOfType(m)))
                    {
                        if (invalid == mod)
                            continue;

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
        /// Checks that all <see cref="Mod"/>s in a combination are valid for a local gameplay session.
        /// </summary>
        /// <param name="mods">The mods to check.</param>
        /// <param name="invalidMods">Invalid mods, if any were found. Will be null if all mods were valid.</param>
        /// <returns>Whether the input mods were all valid. If false, <paramref name="invalidMods"/> will contain all invalid entries.</returns>
        public static bool CheckValidForGameplay(IEnumerable<Mod> mods, [NotNullWhen(false)] out List<Mod>? invalidMods)
        {
            mods = mods.ToArray();

            // checking compatibility of multi mods would try to flatten them and return incompatible mods.
            // in gameplay context, we never want MultiMod selected in the first place, therefore check against it first.
            if (!checkValid(mods, m => !(m is MultiMod), out invalidMods))
                return false;

            if (!CheckCompatibleSet(mods, out invalidMods))
                return false;

            return checkValid(mods, m => m.HasImplementation, out invalidMods);
        }

        /// <summary>
        /// Checks that all <see cref="Mod"/>s in a combination are valid as "required mods" in a multiplayer match session.
        /// </summary>
        /// <param name="mods">The mods to check.</param>
        /// <param name="invalidMods">Invalid mods, if any were found. Will be null if all mods were valid.</param>
        /// <returns>Whether the input mods were all valid. If false, <paramref name="invalidMods"/> will contain all invalid entries.</returns>
        public static bool CheckValidRequiredModsForMultiplayer(IEnumerable<Mod> mods, [NotNullWhen(false)] out List<Mod>? invalidMods)
        {
            mods = mods.ToArray();

            // checking compatibility of multi mods would try to flatten them and return incompatible mods.
            // in gameplay context, we never want MultiMod selected in the first place, therefore check against it first.
            if (!checkValid(mods, m => !(m is MultiMod), out invalidMods))
                return false;

            if (!CheckCompatibleSet(mods, out invalidMods))
                return false;

            return checkValid(mods, m => m.Type != ModType.System && m.HasImplementation && m.ValidForMultiplayer, out invalidMods);
        }

        /// <summary>
        /// Checks that all <see cref="Mod"/>s in a combination are valid as "free mods" in a multiplayer match session.
        /// </summary>
        /// <remarks>
        /// Note that this does not check compatibility between mods,
        /// given that the passed mods are expected to be the ones to be allowed for the multiplayer match,
        /// not to be confused with the list of mods the user currently has selected for the multiplayer match.
        /// </remarks>
        /// <param name="mods">The mods to check.</param>
        /// <param name="invalidMods">Invalid mods, if any were found. Will be null if all mods were valid.</param>
        /// <returns>Whether the input mods were all valid. If false, <paramref name="invalidMods"/> will contain all invalid entries.</returns>
        public static bool CheckValidFreeModsForMultiplayer(IEnumerable<Mod> mods, [NotNullWhen(false)] out List<Mod>? invalidMods)
            => checkValid(mods, m => m.Type != ModType.System && m.HasImplementation && m.ValidForMultiplayerAsFreeMod && !(m is MultiMod), out invalidMods);

        private static bool checkValid(IEnumerable<Mod> mods, Predicate<Mod> valid, [NotNullWhen(false)] out List<Mod>? invalidMods)
        {
            mods = mods.ToArray();
            invalidMods = null;

            foreach (var mod in mods)
            {
                if (!valid(mod))
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

        /// <summary>
        /// Verifies all proposed mods are valid for a given ruleset and returns instantiated <see cref="Mod"/>s for further processing.
        /// </summary>
        /// <param name="ruleset">The ruleset to verify mods against.</param>
        /// <param name="proposedMods">The proposed mods.</param>
        /// <param name="valid">Mods instantiated from <paramref name="proposedMods"/> which were valid for the given <paramref name="ruleset"/>.</param>
        /// <returns>Whether all <paramref name="proposedMods"/> were valid for the given <paramref name="ruleset"/>.</returns>
        public static bool InstantiateValidModsForRuleset(Ruleset ruleset, IEnumerable<APIMod> proposedMods, out List<Mod> valid)
        {
            valid = new List<Mod>();
            bool proposedWereValid = true;

            foreach (var apiMod in proposedMods)
            {
                var mod = apiMod.ToMod(ruleset);

                if (mod is UnknownMod)
                {
                    proposedWereValid = false;
                    continue;
                }

                valid.Add(mod);
            }

            return proposedWereValid;
        }
    }
}
