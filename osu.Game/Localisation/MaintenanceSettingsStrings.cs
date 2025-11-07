// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class MaintenanceSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.MaintenanceSettings";

        /// <summary>
        /// "Maintenance"
        /// </summary>
        public static LocalisableString MaintenanceSectionHeader => new TranslatableString(getKey(@"maintenance_section_header"), @"Maintenance");

        /// <summary>
        /// "Import files"
        /// </summary>
        public static LocalisableString ImportFiles => new TranslatableString(getKey(@"import_files"), @"Import files");

        /// <summary>
        /// "Run latency certifier"
        /// </summary>
        public static LocalisableString RunLatencyCertifier => new TranslatableString(getKey(@"run_latency_certifier"), @"Run latency certifier");

        /// <summary>
        /// "Delete ALL beatmaps"
        /// </summary>
        public static LocalisableString DeleteAllBeatmaps => new TranslatableString(getKey(@"delete_all_beatmaps"), @"Delete ALL beatmaps");

        /// <summary>
        /// "Delete ALL beatmap videos"
        /// </summary>
        public static LocalisableString DeleteAllBeatmapVideos => new TranslatableString(getKey(@"delete_all_beatmap_videos"), @"Delete ALL beatmap videos");

        /// <summary>
        /// "Reset ALL beatmap offsets"
        /// </summary>
        public static LocalisableString ResetAllOffsets => new TranslatableString(getKey(@"reset_all_offsets"), @"Reset ALL beatmap offsets");

        /// <summary>
        /// "Delete ALL scores"
        /// </summary>
        public static LocalisableString DeleteAllScores => new TranslatableString(getKey(@"delete_all_scores"), @"Delete ALL scores");

        /// <summary>
        /// "Delete ALL skins"
        /// </summary>
        public static LocalisableString DeleteAllSkins => new TranslatableString(getKey(@"delete_all_skins"), @"Delete ALL skins");

        /// <summary>
        /// "Delete ALL collections"
        /// </summary>
        public static LocalisableString DeleteAllCollections => new TranslatableString(getKey(@"delete_all_collections"), @"Delete ALL collections");

        /// <summary>
        /// "Restore all hidden difficulties"
        /// </summary>
        public static LocalisableString RestoreAllHiddenDifficulties => new TranslatableString(getKey(@"restore_all_hidden_difficulties"), @"Restore all hidden difficulties");

        /// <summary>
        /// "Restore all recently deleted beatmaps"
        /// </summary>
        public static LocalisableString RestoreAllRecentlyDeletedBeatmaps => new TranslatableString(getKey(@"restore_all_recently_deleted_beatmaps"), @"Restore all recently deleted beatmaps");

        /// <summary>
        /// "Delete ALL mod presets"
        /// </summary>
        public static LocalisableString DeleteAllModPresets => new TranslatableString(getKey(@"delete_all_mod_presets"), @"Delete ALL mod presets");

        /// <summary>
        /// "Restore all recently deleted mod presets"
        /// </summary>
        public static LocalisableString RestoreAllRecentlyDeletedModPresets => new TranslatableString(getKey(@"restore_all_recently_deleted_mod_presets"), @"Restore all recently deleted mod presets");

        /// <summary>
        /// "Deleted all collections!"
        /// </summary>
        public static LocalisableString DeletedAllCollections => new TranslatableString(getKey(@"deleted_all_collections"), @"Deleted all collections!");

        /// <summary>
        /// "No collections found to delete!"
        /// </summary>
        public static LocalisableString NoCollectionsFoundToDelete => new TranslatableString(getKey(@"no_collections_found_to_delete"), @"No collections found to delete!");

        /// <summary>
        /// "Deleted all mod presets!"
        /// </summary>
        public static LocalisableString DeletedAllModPresets => new TranslatableString(getKey(@"deleted_all_mod_presets"), @"Deleted all mod presets!");

        /// <summary>
        /// "No mod presets found to delete!"
        /// </summary>
        public static LocalisableString NoModPresetsFoundToDelete => new TranslatableString(getKey(@"no_mod_presets_found_to_delete"), @"No mod presets found to delete!");

        /// <summary>
        /// "Restored all deleted mod presets!"
        /// </summary>
        public static LocalisableString RestoredAllDeletedModPresets => new TranslatableString(getKey(@"restored_all_deleted_mod_presets"), @"Restored all deleted mod presets!");

        /// <summary>
        /// "No mod presets found to restore!"
        /// </summary>
        public static LocalisableString NoModPresetsFoundToRestore => new TranslatableString(getKey(@"no_mod_presets_found_to_restore"), @"No mod presets found to restore!");

        /// <summary>
        /// "Please select your osu!stable install location"
        /// </summary>
        public static LocalisableString StableDirectorySelectHeader => new TranslatableString(getKey(@"stable_directory_select_header"), @"Please select your osu!stable install location");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
