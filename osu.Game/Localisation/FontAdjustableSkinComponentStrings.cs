// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class FontAdjustableSkinComponentStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.FontAdjustableSkinComponent";

        /// <summary>
        /// "Font"
        /// </summary>
        public static LocalisableString Font => new TranslatableString(getKey(@"font"), "Font");

        /// <summary>
        /// "The font to use."
        /// </summary>
        public static LocalisableString FontDescription => new TranslatableString(getKey(@"font_description"), "The font to use.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
