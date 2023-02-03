// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ModSelectOverlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ModSelectOverlay";

        /// <summary>
        /// "Mod Select"
        /// </summary>
        public static LocalisableString ModSelectTitle => new TranslatableString(getKey(@"mod_select_title"), @"Mod Select");

        /// <summary>
        /// "Mods provide different ways to enjoy gameplay. Some have an effect on the score you can achieve during ranked play. Others are just for fun."
        /// </summary>
        public static LocalisableString ModSelectDescription => new TranslatableString(getKey(@"mod_select_description"),
            @"Mods provide different ways to enjoy gameplay. Some have an effect on the score you can achieve during ranked play. Others are just for fun.");

        /// <summary>
        /// "Mod Customisation"
        /// </summary>
        public static LocalisableString ModCustomisation => new TranslatableString(getKey(@"mod_customisation"), @"Mod Customisation");

        /// <summary>
        /// "Personal Presets"
        /// </summary>
        public static LocalisableString PersonalPresets => new TranslatableString(getKey(@"personal_presets"), @"Personal Presets");

        /// <summary>
        /// "Add preset"
        /// </summary>
        public static LocalisableString AddPreset => new TranslatableString(getKey(@"add_preset"), @"Add preset");

        /// <summary>
        /// "Difficulty Reduction"
        /// </summary>
        public static LocalisableString DifficultyReduction => new TranslatableString(getKey(@"difficulty_reduction"), "Difficulty Reduction");

        /// <summary>
        /// "Difficulty Increase"
        /// </summary>
        public static LocalisableString DifficultyIncrease => new TranslatableString(getKey(@"difficulty_increase"), "Difficulty Increase");

        /// <summary>
        /// "Conversion"
        /// </summary>
        public static LocalisableString Conversion => new TranslatableString(getKey(@"conversion"), "Conversion");

        /// <summary>
        /// "Automation"
        /// </summary>
        public static LocalisableString Automation => new TranslatableString(getKey(@"automation"), "Automation");

        /// <summary>
        /// "Fun"
        /// </summary>
        public static LocalisableString Fun => new TranslatableString(getKey(@"fun"), "Fun");

        /// <summary>
        /// "Incompatible with:"
        /// </summary>
        public static LocalisableString IncompatibleWith => new TranslatableString(getKey(@"incompatible_with"), "Incompatible with:");

        /// <summary>
        /// "Compatible with all mods"
        /// </summary>
        public static LocalisableString CompatibleWithAll => new TranslatableString(getKey(@"compatible_with_all"), "Compatible with all mods");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
