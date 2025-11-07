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
