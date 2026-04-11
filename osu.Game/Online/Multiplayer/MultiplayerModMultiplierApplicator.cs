// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// Provides helpers for applying custom mod multipliers defined by the room host.
    /// These multipliers are purely cosmetic within the lobby and are never submitted to the API.
    /// </summary>
    public static class MultiplayerModMultiplierApplicator
    {
        /// <summary>
        /// The minimum allowed custom multiplier value.
        /// </summary>
        public const double MIN_MULTIPLIER = 0.1;

        /// <summary>
        /// The maximum allowed custom multiplier value.
        /// </summary>
        public const double MAX_MULTIPLIER = 10.0;

        /// <summary>
        /// Computes the effective score multiplier for a set of mods, applying any custom overrides from the room settings.
        /// </summary>
        /// <param name="mods">The mods to compute the multiplier for.</param>
        /// <param name="customMultipliers">
        /// A dictionary mapping mod acronyms to custom multiplier values, as stored in <see cref="MultiplayerRoomSettings.ModMultipliers"/>.
        /// May be null or empty, in which case the original multipliers are used.
        /// </param>
        /// <returns>The effective total score multiplier.</returns>
        public static double GetEffectiveMultiplier(IEnumerable<Mod> mods, IReadOnlyDictionary<string, double>? customMultipliers)
        {
            double multiplier = 1.0;

            foreach (var mod in mods)
            {
                double modMultiplier = GetEffectiveModMultiplier(mod, customMultipliers);
                multiplier *= modMultiplier;
            }

            return multiplier;
        }

        /// <summary>
        /// Returns the effective multiplier for a single mod, using the custom override if available.
        /// </summary>
        /// <param name="mod">The mod.</param>
        /// <param name="customMultipliers">Custom multiplier overrides keyed by mod acronym.</param>
        /// <returns>The effective multiplier for this mod.</returns>
        public static double GetEffectiveModMultiplier(Mod mod, IReadOnlyDictionary<string, double>? customMultipliers)
        {
            if (customMultipliers != null && customMultipliers.TryGetValue(mod.Acronym, out double custom))
                return Math.Clamp(custom, MIN_MULTIPLIER, MAX_MULTIPLIER);

            return mod.ScoreMultiplier;
        }

        /// <summary>
        /// Validates and sanitises a custom multiplier dictionary, clamping all values to the allowed range
        /// and removing entries for unknown mods.
        /// </summary>
        /// <param name="multipliers">The raw dictionary to sanitise.</param>
        /// <returns>A sanitised copy of the dictionary.</returns>
        public static Dictionary<string, double> Sanitise(Dictionary<string, double> multipliers)
        {
            var result = new Dictionary<string, double>(multipliers.Count);

            foreach (var (acronym, value) in multipliers)
            {
                if (string.IsNullOrWhiteSpace(acronym))
                    continue;

                result[acronym] = Math.Clamp(value, MIN_MULTIPLIER, MAX_MULTIPLIER);
            }

            return result;
        }
    }
}
