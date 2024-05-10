// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class FooterButtonModsV2Strings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.FooterButtonModsV2";

        /// <summary>
        /// "{0} mods"
        /// </summary>
        public static LocalisableString Mods(int count) => new TranslatableString(getKey(@"mods"), @"{0} mods", count);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
