// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DeleteConfirmationContentStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.DeleteConfirmationContent";

        /// <summary>
        /// "Are you sure you want to delete all beatmaps?"
        /// </summary>
        public static LocalisableString Beatmaps => new TranslatableString(getKey(@"beatmaps"), @"Are you sure you want to delete all beatmaps?");

        /// <summary>
        /// "Are you sure you want to delete all beatmaps videos? This cannot be undone!"
        /// </summary>
        public static LocalisableString BeatmapVideos => new TranslatableString(getKey(@"beatmap_videos"), @"Are you sure you want to delete all beatmaps videos? This cannot be undone!");

        /// <summary>
        /// "Are you sure you want to delete all skins? This cannot be undone!"
        /// </summary>
        public static LocalisableString Skins => new TranslatableString(getKey(@"skins"), @"Are you sure you want to delete all skins? This cannot be undone!");

        /// <summary>
        /// "Are you sure you want to delete all collections? This cannot be undone!"
        /// </summary>
        public static LocalisableString Collections => new TranslatableString(getKey(@"collections"), @"Are you sure you want to delete all collections? This cannot be undone!");

        /// <summary>
        /// "Are you sure you want to delete all scores? This cannot be undone!"
        /// </summary>
        public static LocalisableString Scores => new TranslatableString(getKey(@"collections"), @"Are you sure you want to delete all scores? This cannot be undone!");

        /// <summary>
        /// "Are you sure you want to delete all mod presets?"
        /// </summary>
        public static LocalisableString ModPresets => new TranslatableString(getKey(@"mod_presets"), @"Are you sure you want to delete all mod presets?");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
