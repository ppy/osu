﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
        public static LocalisableString ModSelectDescription => new TranslatableString(getKey(@"mod_select_description"), @"Mods provide different ways to enjoy gameplay. Some have an effect on the score you can achieve during ranked play. Others are just for fun.");

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

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
