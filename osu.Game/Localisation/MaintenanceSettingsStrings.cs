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

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
