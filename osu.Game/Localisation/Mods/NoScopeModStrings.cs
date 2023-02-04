// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class NoScopeModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.NoScopeMod";

        /// <summary>
        /// "Where's the cursor?"
        /// </summary>
        public static LocalisableString OsuDescription => new TranslatableString(getKey(@"osu_description"), "Where's the cursor?");

        /// <summary>
        /// "Where's the catcher?"
        /// </summary>
        public static LocalisableString CatchDescription => new TranslatableString(getKey(@"catch_description"), "Where's the catcher?");

        /// <summary>
        /// "Hidden at combo"
        /// </summary>
        public static LocalisableString HiddenComboCount => new TranslatableString(getKey(@"hidden_combo_count"), "Hidden at combo");

        /// <summary>
        /// "The combo count at which the cursor becomes completely hidden"
        /// </summary>
        public static LocalisableString HiddenComboCountDescription => new TranslatableString(getKey(@"hidden_combo_count_description"), "The combo count at which the cursor becomes completely hidden");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
