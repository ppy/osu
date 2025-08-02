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
        public static LocalisableString SpriteName => new TranslatableString(getKey(@"sprite_name"), @"Sprite name");

        /// <summary>
        /// "Font"
        /// </summary>
        public static LocalisableString Font => new TranslatableString(getKey(@"font"), @"Font");

        /// <summary>
        /// "Text"
        /// </summary>
        public static LocalisableString TextElementText => new TranslatableString(getKey(@"text_element_text"), @"Text");

        /// <summary>
        /// "Corner radius"
        /// </summary>
        public static LocalisableString CornerRadius => new TranslatableString(getKey(@"corner_radius"), @"Corner radius");

        /// <summary>
        /// "How rounded the corners should be."
        /// </summary>
        public static LocalisableString CornerRadiusDescription => new TranslatableString(getKey(@"corner_radius_description"), @"How rounded the corners should be.");

        /// <summary>
        /// "Show label"
        /// </summary>
        public static LocalisableString ShowLabel => new TranslatableString(getKey(@"show_label"), @"Show label");

        /// <summary>
        /// "Colour"
        /// </summary>
        public static LocalisableString Colour => new TranslatableString(getKey(@"colour"), @"Colour");

        /// <summary>
        /// "Text colour"
        /// </summary>
        public static LocalisableString TextColour => new TranslatableString(getKey(@"text_colour"), @"Text colour");

        /// <summary>
        /// "Text weight"
        /// </summary>
        public static LocalisableString TextWeight => new TranslatableString(getKey(@"text_weight"), @"Text weight");

        /// <summary>
        /// "Use relative size"
        /// </summary>
        public static LocalisableString UseRelativeSize => new TranslatableString(getKey(@"use_relative_size"), @"Use relative size");

        /// <summary>
        /// "Collapse during gameplay"
        /// </summary>
        public static LocalisableString CollapseDuringGameplay => new TranslatableString(getKey(@"collapse_during_gameplay"), @"Collapse during gameplay");

        /// <summary>
        /// "If enabled, the leaderboard will become more compact during active gameplay."
        /// </summary>
        public static LocalisableString CollapseDuringGameplayDescription =>
            new TranslatableString(getKey(@"if_enabled_the_leaderboard_will"), @"If enabled, the leaderboard will become more compact during active gameplay.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
