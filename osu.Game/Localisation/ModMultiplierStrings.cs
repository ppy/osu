// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ModMultiplierStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ModMultiplierStrings";

        /// <summary>
        /// "Mod Multipliers"
        /// </summary>
        public static LocalisableString ModMultipliersTitle => new TranslatableString(getKey(@"mod_multipliers_title"), @"Mod Multipliers");

        /// <summary>
        /// "Adjust Mod Multipliers"
        /// </summary>
        public static LocalisableString AdjustModMultipliers => new TranslatableString(getKey(@"adjust_mod_multipliers"), @"Adjust Mod Multipliers");

        /// <summary>
        /// "Override the score multiplier for each mod. Changes apply to the next map only and do not affect ranked scores."
        /// </summary>
        public static LocalisableString ModMultipliersDescription => new TranslatableString(getKey(@"mod_multipliers_description"), @"Override the score multiplier for each mod. Changes apply to the next map only and do not affect ranked scores.");

        /// <summary>
        /// "Reset"
        /// </summary>
        public static LocalisableString Reset => new TranslatableString(getKey(@"reset"), @"Reset");

        /// <summary>
        /// "Reset All"
        /// </summary>
        public static LocalisableString ResetAll => new TranslatableString(getKey(@"reset_all"), @"Reset All");

        /// <summary>
        /// "No mods selected"
        /// </summary>
        public static LocalisableString NoModsSelected => new TranslatableString(getKey(@"no_mods_selected"), @"No mods selected");

        /// <summary>
        /// "Multiplier: {0:0.00}x"
        /// </summary>
        public static LocalisableString MultiplierValue(double value) => new TranslatableString(getKey(@"multiplier_value"), @"Multiplier: {0:0.00}x", value);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
