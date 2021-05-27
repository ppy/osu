// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class SettingsStrings
    {
        private const string prefix = "osu.Game.Localisation.Settings";

        /// <summary>
        /// "settings"
        /// </summary>
        public static LocalisableString HeaderTitle => new TranslatableString(getKey("header_title"), "settings");

        /// <summary>
        /// "change the way osu! behaves"
        /// </summary>
        public static LocalisableString HeaderDescription => new TranslatableString(getKey("header_description"), "change the way osu! behaves");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
