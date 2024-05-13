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
        /// "{0} mods"
        /// </summary>
        public static LocalisableString Mods(int count) => new TranslatableString(getKey(@"mods"), @"{0} mods", count);

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
        /// "Use current mods"
        /// </summary>
        public static LocalisableString UseCurrentMods => new TranslatableString(getKey(@"use_current_mods"), @"Use current mods");

        /// <summary>
        /// "tab to search..."
        /// </summary>
        public static LocalisableString TabToSearch => new TranslatableString(getKey(@"tab_to_search"), @"tab to search...");

        /// <summary>
        /// "Score Multiplier"
        /// </summary>
        public static LocalisableString ScoreMultiplier => new TranslatableString(getKey(@"score_multiplier"), @"Score Multiplier");

        /// <summary>
        /// "Ranked"
        /// </summary>
        public static LocalisableString Ranked => new TranslatableString(getKey(@"ranked"), @"Ranked");

        /// <summary>
        /// "Performance points can be granted for the active mods."
        /// </summary>
        public static LocalisableString RankedExplanation => new TranslatableString(getKey(@"ranked_explanation"), @"Performance points can be granted for the active mods.");

        /// <summary>
        /// "Unranked"
        /// </summary>
        public static LocalisableString Unranked => new TranslatableString(getKey(@"unranked"), @"Unranked");

        /// <summary>
        /// "Performance points will not be granted due to active mods."
        /// </summary>
        public static LocalisableString UnrankedExplanation => new TranslatableString(getKey(@"unranked_explanation"), @"Performance points will not be granted due to active mods.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
