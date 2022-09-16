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
        /// "Beatmaps"
        /// </summary>
        public static LocalisableString Beatmaps => new TranslatableString(getKey(@"beatmaps"), @"Beatmaps");

        /// <summary>
        /// "Skins"
        /// </summary>
        public static LocalisableString Skins => new TranslatableString(getKey(@"skins"), @"Skins");

        /// <summary>
        /// "Collections"
        /// </summary>
        public static LocalisableString Collections => new TranslatableString(getKey(@"collections"), @"Collections");

        /// <summary>
        /// "Scores"
        /// </summary>
        public static LocalisableString Scores => new TranslatableString(getKey(@"scores"), @"Scores");

        /// <summary>
        /// "Mod presets"
        /// </summary>
        public static LocalisableString ModPresets => new TranslatableString(getKey(@"mod_presets"), @"Mod presets");

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
        /// "Import beatmaps from stable"
        /// </summary>
        public static LocalisableString ImportBeatmapsFromStable => new TranslatableString(getKey(@"import_beatmaps_from_stable"), @"Import beatmaps from stable");

        /// <summary>
        /// "Delete ALL beatmaps"
        /// </summary>
        public static LocalisableString DeleteAllBeatmaps => new TranslatableString(getKey(@"delete_all_beatmaps"), @"Delete ALL beatmaps");

        /// <summary>
        /// "Delete ALL beatmap videos"
        /// </summary>
        public static LocalisableString DeleteAllBeatmapVideos => new TranslatableString(getKey(@"delete_all_beatmap_videos"), @"Delete ALL beatmap videos");

        /// <summary>
        /// "Import scores from stable"
        /// </summary>
        public static LocalisableString ImportScoresFromStable => new TranslatableString(getKey(@"import_scores_from_stable"), @"Import scores from stable");

        /// <summary>
        /// "Delete ALL scores"
        /// </summary>
        public static LocalisableString DeleteAllScores => new TranslatableString(getKey(@"delete_all_scores"), @"Delete ALL scores");

        /// <summary>
        /// "Import skins from stable"
        /// </summary>
        public static LocalisableString ImportSkinsFromStable => new TranslatableString(getKey(@"import_skins_from_stable"), @"Import skins from stable");

        /// <summary>
        /// "Delete ALL skins"
        /// </summary>
        public static LocalisableString DeleteAllSkins => new TranslatableString(getKey(@"delete_all_skins"), @"Delete ALL skins");

        /// <summary>
        /// "Import collections from stable"
        /// </summary>
        public static LocalisableString ImportCollectionsFromStable => new TranslatableString(getKey(@"import_collections_from_stable"), @"Import collections from stable");

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
        /// "Restored all deleted mod presets!"
        /// </summary>
        public static LocalisableString RestoredAllDeletedModPresets => new TranslatableString(getKey(@"restored_all_deleted_mod_presets"), @"Restored all deleted mod presets!");

        /// <summary>
        /// "Everything?"
        /// </summary>
        public static LocalisableString MassDeleteConfirmation => new TranslatableString(getKey(@"mass_delete_confirmation"), @"Everything?");

        /// <summary>
        /// "All beatmap videos? This cannot be undone!"
        /// </summary>
        public static LocalisableString MassVideoDeleteConfirmation => new TranslatableString(getKey(@"mass_video_delete_confirmation"), @"All beatmap videos? This cannot be undone!");

        /// <summary>
        /// "Failed to automatically locate an osu!stable installation."
        /// </summary>
        public static LocalisableString StableDirectoryLocationHeader => new TranslatableString(getKey(@"stable_directory_location_header"), @"Failed to automatically locate an osu!stable installation.");

        /// <summary>
        /// "An existing install could not be located. If you know where it is, you can help locate it."
        /// </summary>
        public static LocalisableString StableDirectoryLocationBody => new TranslatableString(getKey(@"stable_directory_location_body"), @"An existing install could not be located. If you know where it is, you can help locate it.");

        /// <summary>
        /// "Sure! I know where it is located!"
        /// </summary>
        public static LocalisableString StableDirectoryLocationOk => new TranslatableString(getKey(@"stable_directory_location_ok"), @"Sure! I know where it is located!");

        /// <summary>
        /// "Actually I don't have osu!stable installed."
        /// </summary>
        public static LocalisableString StableDirectoryLocationCancel => new TranslatableString(getKey(@"stable_directory_location_cancel"), @"Actually I don't have osu!stable installed.");

        /// <summary>
        /// "Please select your osu!stable install location"
        /// </summary>
        public static LocalisableString StableDirectorySelectHeader => new TranslatableString(getKey(@"stable_directory_select_header"), @"Please select your osu!stable install location");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
