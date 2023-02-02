// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class TextElementStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.TextElement";

        /// <summary>
        /// "Text"
        /// </summary>
        public static LocalisableString TextElementText => new TranslatableString(getKey(@"text_element_text"), "Text");

        /// <summary>
        /// "The text to be displayed."
        /// </summary>
        public static LocalisableString TextElementTextDescription => new TranslatableString(getKey(@"text_element_text_description"), "The text to be displayed.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
