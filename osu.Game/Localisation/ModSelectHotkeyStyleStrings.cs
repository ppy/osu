// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ModSelectHotkeyStyleStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ModSelectHotkeyStyle";

        /// <summary>
        /// "Sequential"
        /// </summary>
        public static LocalisableString Sequential => new TranslatableString(getKey(@"sequential"), @"Sequential");

        /// <summary>
        /// "Classic"
        /// </summary>
        public static LocalisableString Classic => new TranslatableString(getKey(@"classic"), @"Classic");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}