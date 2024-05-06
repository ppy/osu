// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DeleteConfirmationContentStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.DeleteConfirmationContent";

        /// <summary>
        /// "All beatmaps?"
        /// </summary>
        public static LocalisableString Beatmaps => new TranslatableString(getKey(@"beatmaps"), @"All beatmaps?");

        /// <summary>
        /// "All beatmaps videos? This cannot be undone!"
        /// </summary>
        public static LocalisableString BeatmapVideos => new TranslatableString(getKey(@"beatmap_videos"), @"All beatmaps videos? This cannot be undone!");

        /// <summary>
        /// "All skins? This cannot be undone!"
        /// </summary>
        public static LocalisableString Skins => new TranslatableString(getKey(@"skins"), @"All skins? This cannot be undone!");

        /// <summary>
        /// "All collections? This cannot be undone!"
        /// </summary>
        public static LocalisableString Collections => new TranslatableString(getKey(@"collections"), @"All collections? This cannot be undone!");

        /// <summary>
        /// "All scores? This cannot be undone!"
        /// </summary>
        public static LocalisableString Scores => new TranslatableString(getKey(@"collections"), @"All scores? This cannot be undone!");

        /// <summary>
        /// "All mod presets?"
        /// </summary>
        public static LocalisableString ModPresets => new TranslatableString(getKey(@"mod_presets"), @"All mod presets?");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
