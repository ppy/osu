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
        /// "Skin layout editor"
        /// </summary>
        public static LocalisableString SkinLayoutEditor => new TranslatableString(getKey(@"skin_layout_editor"), @"Skin layout editor");

        /// <summary>
        /// "Gameplay cursor size"
        /// </summary>
        public static LocalisableString GameplayCursorSize => new TranslatableString(getKey(@"gameplay_cursor_size"), @"Gameplay cursor size");

        /// <summary>
        /// "根据谱面物件大小调整光标大小"
        /// </summary>
        public static LocalisableString AutoCursorSize => new TranslatableString(getKey(@"auto_cursor_size"), @"根据谱面物件大小调整光标大小");

        /// <summary>
        /// "Show gameplay cursor during touch input"
        /// </summary>
        public static LocalisableString GameplayCursorDuringTouch => new TranslatableString(getKey(@"gameplay_cursor_during_touch"), @"Show gameplay cursor during touch input");

        /// <summary>
        /// "谱面皮肤"
        /// </summary>
        public static LocalisableString BeatmapSkins => new TranslatableString(getKey(@"beatmap_skins"), @"谱面皮肤");

        /// <summary>
        /// "谱面颜色"
        /// </summary>
        public static LocalisableString BeatmapColours => new TranslatableString(getKey(@"beatmap_colours"), @"谱面颜色");

        /// <summary>
        /// "谱面打击音效"
        /// </summary>
        public static LocalisableString BeatmapHitsounds => new TranslatableString(getKey(@"beatmap_hitsounds"), @"谱面打击音效");

        /// <summary>
        /// "Export selected skin"
        /// </summary>
        public static LocalisableString ExportSkinButton => new TranslatableString(getKey(@"export_skin_button"), @"Export selected skin");

        /// <summary>
        /// "删除选中的皮肤"
        /// </summary>
        public static LocalisableString DeleteSkinButton => new TranslatableString(getKey(@"llin_delete_skin_button"), @"删除选中的皮肤");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
