// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.SkinComponents
{
    public static class SkinnableComponentStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SkinComponents.SkinnableComponentStrings";

        /// <summary>
        /// "Sprite name"
        /// </summary>
        public static LocalisableString SpriteName => new TranslatableString(getKey(@"sprite_name"), "Sprite name");

        /// <summary>
        /// "The filename of the sprite"
        /// </summary>
        public static LocalisableString SpriteNameDescription => new TranslatableString(getKey(@"sprite_name_description"), "The filename of the sprite");

        /// <summary>
        /// "Font"
        /// </summary>
        public static LocalisableString Font => new TranslatableString(getKey(@"font"), "Font");

        /// <summary>
        /// "The font to use."
        /// </summary>
        public static LocalisableString FontDescription => new TranslatableString(getKey(@"font_description"), "The font to use.");

        /// <summary>
        /// "Text"
        /// </summary>
        public static LocalisableString TextElementText => new TranslatableString(getKey(@"text_element_text"), "Text");

        /// <summary>
        /// "The text to be displayed."
        /// </summary>
        public static LocalisableString TextElementTextDescription => new TranslatableString(getKey(@"text_element_text_description"), "The text to be displayed.");

        /// <summary>
        /// "Corner radius"
        /// </summary>
        public static LocalisableString CornerRadius => new TranslatableString(getKey(@"corner_radius"), "Corner radius");

        /// <summary>
        /// "How rounded the corners should be."
        /// </summary>
        public static LocalisableString CornerRadiusDescription => new TranslatableString(getKey(@"corner_radius_description"), "How rounded the corners should be.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
