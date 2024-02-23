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
        /// "Select directory"
        /// </summary>
        public static LocalisableString SelectDirectory => new TranslatableString(getKey(@"select_directory"), @"Select directory");

        /// <summary>
        /// "Migration in progress"
        /// </summary>
        public static LocalisableString MigrationInProgress => new TranslatableString(getKey(@"migration_in_progress"), @"Migration in progress");

        /// <summary>
        /// "This could take a few minutes depending on the speed of your disk(s)."
        /// </summary>
        public static LocalisableString MigrationDescription => new TranslatableString(getKey(@"migration_description"), @"This could take a few minutes depending on the speed of your disk(s).");

        /// <summary>
        /// "Please avoid interacting with the game!"
        /// </summary>
        public static LocalisableString ProhibitedInteractDuringMigration => new TranslatableString(getKey(@"prohibited_interact_during_migration"), @"Please avoid interacting with the game!");

        /// <summary>
        /// "Some files couldn't be cleaned up during migration. Clicking this notification will open the folder so you can manually clean things up."
        /// </summary>
        public static LocalisableString FailedCleanupNotification => new TranslatableString(getKey(@"failed_cleanup_notification"), @"Some files couldn't be cleaned up during migration. Clicking this notification will open the folder so you can manually clean things up.");

        /// <summary>
        /// "Please select a new location"
        /// </summary>
        public static LocalisableString SelectNewLocation => new TranslatableString(getKey(@"select_new_location"), @"Please select a new location");

        /// <summary>
        /// "The target directory already seems to have an osu! install. Use that data instead?"
        /// </summary>
        public static LocalisableString TargetDirectoryAlreadyInstalledOsu => new TranslatableString(getKey(@"target_directory_already_installed_osu"), @"The target directory already seems to have an osu! install. Use that data instead?");

        /// <summary>
        /// "To complete this operation, osu! will close. Please open it again to use the new data location."
        /// </summary>
        public static LocalisableString RestartAndReOpenRequiredForCompletion => new TranslatableString(getKey(@"restart_and_re_open_required_for_completion"), @"To complete this operation, osu! will close. Please open it again to use the new data location.");

        /// <summary>
        /// "Delete ALL beatmaps"
        /// </summary>
        public static LocalisableString DeleteAllBeatmaps => new TranslatableString(getKey(@"delete_all_beatmaps"), @"Delete ALL beatmaps");

        /// <summary>
        /// "Delete ALL beatmap videos"
        /// </summary>
        public static LocalisableString DeleteAllBeatmapVideos => new TranslatableString(getKey(@"delete_all_beatmap_videos"), @"Delete ALL beatmap videos");

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
