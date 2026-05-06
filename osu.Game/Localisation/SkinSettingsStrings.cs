// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class SkinSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.SkinSettings";

        /// <summary>
        /// "Skin"
        /// </summary>
        public static LocalisableString SkinSectionHeader => new TranslatableString(getKey(@"skin_section_header"), @"Skin");

        /// <summary>
        /// "Current skin"
        /// </summary>
        public static LocalisableString CurrentSkin => new TranslatableString(getKey(@"current_skin"), @"Current skin");

        /// <summary>
        /// "Skin name"
        /// </summary>
        public static LocalisableString SkinName => new TranslatableString(getKey(@"skin_name"), @"Skin name");

        /// <summary>
        /// "Skin layout editor"
        /// </summary>
        public static LocalisableString SkinLayoutEditor => new TranslatableString(getKey(@"skin_layout_editor"), @"Skin layout editor");

        /// <summary>
        /// "Pin"
        /// </summary>
        public static LocalisableString PinSkin => new TranslatableString(getKey(@"pin_skin"), @"Pin");

        /// <summary>
        /// "Unpin"
        /// </summary>
        public static LocalisableString UnpinSkin => new TranslatableString(getKey(@"unpin_skin"), @"Unpin");

        /// <summary>
        /// "Cycle through favourites only"
        /// </summary>
        public static LocalisableString CycleSkinsThroughFavoritesOnly => new TranslatableString(getKey(@"cycle_skins_through_favorites_only"), @"Cycle through favourites only");

        /// <summary>
        /// "Restrict the next/previous skin keybinds to pinned skins only."
        /// </summary>
        public static LocalisableString CycleSkinsThroughFavoritesOnlyDescription => new TranslatableString(getKey(@"cycle_skins_through_favorites_only_description"), @"Restrict the next/previous skin keybinds to pinned skins only.");

        /// <summary>
        /// "Gameplay cursor size"
        /// </summary>
        public static LocalisableString GameplayCursorSize => new TranslatableString(getKey(@"gameplay_cursor_size"), @"Gameplay cursor size");

        /// <summary>
        /// "Adjust gameplay cursor size based on current beatmap"
        /// </summary>
        public static LocalisableString AutoCursorSize => new TranslatableString(getKey(@"auto_cursor_size"), @"Adjust gameplay cursor size based on current beatmap");

        /// <summary>
        /// "Show gameplay cursor during touch input"
        /// </summary>
        public static LocalisableString GameplayCursorDuringTouch => new TranslatableString(getKey(@"gameplay_cursor_during_touch"), @"Show gameplay cursor during touch input");

        /// <summary>
        /// "Beatmap skins"
        /// </summary>
        public static LocalisableString BeatmapSkins => new TranslatableString(getKey(@"beatmap_skins"), @"Beatmap skins");

        /// <summary>
        /// "Beatmap colours"
        /// </summary>
        public static LocalisableString BeatmapColours => new TranslatableString(getKey(@"beatmap_colours"), @"Beatmap colours");

        /// <summary>
        /// "Beatmap hitsounds"
        /// </summary>
        public static LocalisableString BeatmapHitsounds => new TranslatableString(getKey(@"beatmap_hitsounds"), @"Beatmap hitsounds");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
