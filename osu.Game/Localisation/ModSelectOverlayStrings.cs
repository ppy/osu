// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ModSelectOverlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ModSelectOverlay";

        /// <summary>
        /// "Mod选择"
        /// </summary>
        public static LocalisableString ModSelectTitle => new TranslatableString(getKey(@"llin_mod_select_title"), @"Mod选择");

        /// <summary>
        /// "游戏Mods提供了多种多样的游玩方式。有一些会对您的分数等产生影响,还有一些仅供娱乐"
        /// </summary>
        public static LocalisableString ModSelectDescription => new TranslatableString(getKey(@"llin_mod_select_description"), @"游戏Mods提供了多种多样的游玩方式。有一些会对您的分数等产生影响,还有一些仅供娱乐");

        /// <summary>
        /// "Mod选项"
        /// </summary>
        public static LocalisableString ModCustomisation => new TranslatableString(getKey(@"llin_mod_customisation"), @"Mod选项");

        /// <summary>
        /// "Personal Presets"
        /// </summary>
        public static LocalisableString PersonalPresets => new TranslatableString(getKey(@"personal_presets"), @"Personal Presets");

        /// <summary>
        /// "Add preset"
        /// </summary>
        public static LocalisableString AddPreset => new TranslatableString(getKey(@"add_preset"), @"Add preset");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
