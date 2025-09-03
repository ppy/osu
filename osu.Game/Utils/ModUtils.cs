// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Localisation;
using osu.Game.Online.API;
using osu.Game.Online.Rooms;
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
        /// Checks whether the given combination of mods may be set as the <see cref="MultiplayerPlaylistItem.RequiredMods">required mods</see> of a multiplayer playlist item.
        /// </summary>
        /// <param name="mods">The mods to check.</param>
        /// <param name="freestyle">Whether freestyle is enabled for the playlist item.</param>
        /// <param name="invalidMods">Invalid mods, if any were found. Will be null if all mods were valid.</param>
        /// <returns>Whether the input mods were all valid. If false, <paramref name="invalidMods"/> will contain all invalid entries.</returns>
        public static bool CheckValidRequiredModsForMultiplayer(IEnumerable<Mod> mods, bool freestyle, [NotNullWhen(false)] out List<Mod>? invalidMods)
        {
            mods = mods.ToArray();

            // checking compatibility of multi mods would try to flatten them and return incompatible mods.
            // in gameplay context, we never want MultiMod selected in the first place, therefore check against it first.
            if (!checkValid(mods, m => !(m is MultiMod), out invalidMods))
                return false;

            if (!CheckCompatibleSet(mods, out invalidMods))
                return false;

            return checkValid(mods, m => IsValidModForMatch(m, true, MatchType.HeadToHead, freestyle), out invalidMods);
        }

        /// <summary>
        /// Checks whether the given mods are valid to appear as <see cref="MultiplayerPlaylistItem.AllowedMods">allowed mods</see> in a multiplayer playlist item.
        /// </summary>
        /// <remarks>
        /// Note that this does not check compatibility between mods,
        /// given that the passed mods are expected to be the ones to be allowed for the multiplayer match,
        /// not to be confused with the list of mods the user currently has selected for the multiplayer match.
        /// </remarks>
        /// <param name="mods">The mods to check.</param>
        /// <param name="freestyle">Whether freestyle is enabled for the playlist item.</param>
        /// <param name="invalidMods">Invalid mods, if any were found. Will be null if all mods were valid.</param>
        /// <returns>Whether the input mods were all valid. If false, <paramref name="invalidMods"/> will contain all invalid entries.</returns>
        public static bool CheckValidAllowedModsForMultiplayer(IEnumerable<Mod> mods, bool freestyle, [NotNullWhen(false)] out List<Mod>? invalidMods)
            => checkValid(mods, m => IsValidModForMatch(m, false, MatchType.HeadToHead, freestyle), out invalidMods);

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

        /// <summary>
        /// Verifies all mods provided belong to the given ruleset.
        /// </summary>
        /// <param name="ruleset">The ruleset to check the proposed mods against.</param>
        /// <param name="proposedMods">The mods proposed for checking.</param>
        /// <returns>Whether all <paramref name="proposedMods"/> belong to the given <paramref name="ruleset"/>.</returns>
        public static bool CheckModsBelongToRuleset(Ruleset ruleset, IEnumerable<Mod> proposedMods)
        {
            var rulesetModsTypes = ruleset.AllMods.Select(m => m.GetType()).ToList();

            foreach (var proposedMod in proposedMods)
            {
                bool found = false;

                var proposedModType = proposedMod.GetType();

                foreach (var rulesetModType in rulesetModsTypes)
                {
                    if (rulesetModType.IsAssignableFrom(proposedModType))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Given a value of a score multiplier, returns a string version with special handling for a value near 1.00x.
        /// </summary>
        /// <param name="scoreMultiplier">The value of the score multiplier.</param>
        /// <returns>A formatted score multiplier with a trailing "x" symbol</returns>
        public static LocalisableString FormatScoreMultiplier(double scoreMultiplier)
        {
            // Round multiplier values away from 1.00x to two significant digits.
            if (scoreMultiplier > 1)
                scoreMultiplier = Math.Ceiling(Math.Round(scoreMultiplier * 100, 12)) / 100;
            else
                scoreMultiplier = Math.Floor(Math.Round(scoreMultiplier * 100, 12)) / 100;

            return scoreMultiplier.ToLocalisableString("0.00x");
        }

        /// <summary>
        /// Calculate the rate for the song with the selected mods.
        /// </summary>
        /// <param name="mods">The list of selected mods.</param>
        /// <returns>The rate with mods.</returns>
        public static double CalculateRateWithMods(IEnumerable<Mod> mods)
        {
            double rate = 1;

            // TODO: This doesn't consider mods which apply variable rates, yet.
            foreach (var mod in mods.OfType<IApplicableToRate>())
                rate = mod.ApplyToRate(0, rate);

            return rate;
        }

        /// <summary>
        /// Determines whether a given mod is valid on a playlist item.
        /// </summary>
        /// <param name="mod">The mod to test.</param>
        /// <param name="required">
        /// <c>true</c> if the mod is intended as a <see cref="MultiplayerPlaylistItem.RequiredMods">required mod</see> on the target playlist item.
        /// <c>false</c> if it is intended as an <see cref="MultiplayerPlaylistItem.AllowedMods">allowed mod</see>.
        /// </param>
        /// <param name="matchType">The type of match being played.</param>
        /// <param name="freestyle">Whether the target playlist item enables <see cref="MultiplayerPlaylistItem.Freestyle">freestyle</see> mode.</param>
        /// <seealso href="https://github.com/ppy/osu-web/blob/40936b514c6485b874f6c6496d55d9e8b1b88fd4/app/Singletons/Mods.php#L95-L113">Related osu!web function.</seealso>
        public static bool IsValidModForMatch(Mod mod, bool required, MatchType matchType, bool freestyle)
        {
            if (mod.Type == ModType.System || !mod.UserPlayable || !mod.HasImplementation)
                return false;

            if (freestyle && required && !mod.ValidForFreestyleAsRequiredMod)
                return false;

            switch (matchType)
            {
                case MatchType.Playlists:
                    return true;

                default:
                    return required ? mod.ValidForMultiplayer : mod.ValidForMultiplayerAsFreeMod;
            }
        }

        /// <summary>
        /// Given an online listing of mods and the user's preferred ruleset, gathers the mods which are selectable as free mods by the current user.
        /// </summary>
        /// <param name="matchType">The type of match being played.</param>
        /// <param name="requiredMods">The required mods for the playlist item.</param>
        /// <param name="allowedMods">The allowed mods for the playlist item.</param>
        /// <param name="freestyle">Whether freestyle is enabled for the playlist item.</param>
        /// <param name="userRuleset">The user's preferred ruleset, which may differ from the playlist item's selection on freestyle playlist items.</param>
        public static Mod[] EnumerateUserSelectableFreeMods(MatchType matchType, IEnumerable<APIMod> requiredMods, IEnumerable<APIMod> allowedMods, bool freestyle, Ruleset userRuleset)
        {
            if (freestyle)
            {
                Mod[] rulesetRequiredMods = requiredMods.Select(m => m.ToMod(userRuleset)).ToArray();

                // In freestyle, the playlist item doesn't provide the allowed mods. Instead, all mods are unconditionally allowed by default.
                return userRuleset.AllMods.OfType<Mod>()
                                  // But the mods must still be compatible with the room...
                                  .Where(m => IsValidModForMatch(m, false, matchType, true))
                                  // ... And compatible with the required mods listing (this also handles de-duplication).
                                  .Where(m => CheckCompatibleSet(rulesetRequiredMods.Append(m)))
                                  .ToArray();
            }

            // Without freestyle, only the mods specified by the playlist item are valid.
            return allowedMods.Select(m => m.ToMod(userRuleset)).ToArray();
        }
    }
}
