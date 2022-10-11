// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class HomeStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Home";

        /// <summary>
        /// "home"
        /// </summary>
        public static LocalisableString HeaderTitle => new TranslatableString(getKey(@"header_title"), @"home");

        /// <summary>
        /// "return to the main menu"
        /// </summary>
        public static LocalisableString HeaderDescription => new TranslatableString(getKey(@"header_description"), @"return to the main menu");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}