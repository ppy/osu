// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class NotificationsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Notifications";

        /// <summary>
        /// "notifications"
        /// </summary>
        public static LocalisableString HeaderTitle => new TranslatableString(getKey(@"header_title"), @"notifications");

        /// <summary>
        /// "waiting for &#39;ya"
        /// </summary>
        public static LocalisableString HeaderDescription => new TranslatableString(getKey(@"header_description"), @"waiting for 'ya");

        /// <summary>
        /// "Running Tasks"
        /// </summary>
        public static LocalisableString RunningTasks => new TranslatableString(getKey(@"running_tasks"), @"Running Tasks");

        /// <summary>
        /// "Clear All"
        /// </summary>
        public static LocalisableString ClearAll => new TranslatableString(getKey(@"clear_all"), @"Clear All");

        /// <summary>
        /// "Your battery level is low! Charge your device to prevent interruptions during gameplay."
        /// </summary>
        public static LocalisableString BatteryLow => new TranslatableString(getKey(@"battery_low"), @"Your battery level is low! Charge your device to prevent interruptions during gameplay.");

        /// <summary>
        /// "Your game volume is too low to hear anything! Click here to restore it."
        /// </summary>
        public static LocalisableString GameVolumeTooLow => new TranslatableString(getKey(@"game_volume_too_low"), @"Your game volume is too low to hear anything! Click here to restore it.");

        /// <summary>
        /// "The current ruleset doesn&#39;t have an autoplay mod available!"
        /// </summary>
        public static LocalisableString NoAutoplayMod => new TranslatableString(getKey(@"no_autoplay_mod"), @"The current ruleset doesn't have an autoplay mod available!");

        /// <summary>
        /// "osu! doesn&#39;t seem to be able to play audio correctly.
        ///
        /// Please try changing your audio device to a working setting."
        /// </summary>
        public static LocalisableString AudioPlaybackIssue => new TranslatableString(getKey(@"audio_playback_issue"), @"osu! doesn't seem to be able to play audio correctly.

Please try changing your audio device to a working setting.");

        /// <summary>
        /// "The score overlay is currently disabled. You can toggle this by pressing {0}."
        /// </summary>
        public static LocalisableString ScoreOverlayDisabled(LocalisableString arg0) => new TranslatableString(getKey(@"score_overlay_disabled"), @"The score overlay is currently disabled. You can toggle this by pressing {0}.", arg0);

        /// <summary>
        /// "The URL {0} has an unsupported or dangerous protocol and will not be opened."
        /// </summary>
        public static LocalisableString UnsupportedOrDangerousUrlProtocol(string url) => new TranslatableString(getKey(@"unsupported_or_dangerous_url_protocol"), @"The URL {0} has an unsupported or dangerous protocol and will not be opened.", url);

        /// <summary>
        /// "Subsequent messages have been logged. Click to view log files."
        /// </summary>
        public static LocalisableString SubsequentMessagesLogged => new TranslatableString(getKey(@"subsequent_messages_logged"), @"Subsequent messages have been logged. Click to view log files.");

        /// <summary>
        /// "Disabling tablet support due to error: &quot;{0}&quot;"
        /// </summary>
        public static LocalisableString TabletSupportDisabledDueToError(string message) => new TranslatableString(getKey(@"tablet_support_disabled_due_to_error"), @"Disabling tablet support due to error: ""{0}""", message);

        /// <summary>
        /// "Encountered tablet warning, your tablet may not function correctly. Click here for a list of all tablets supported."
        /// </summary>
        public static LocalisableString EncounteredTabletWarning => new TranslatableString(getKey(@"encountered_tablet_warning"), @"Encountered tablet warning, your tablet may not function correctly. Click here for a list of all tablets supported.");

        /// <summary>
        /// "This link type is not yet supported!"
        /// </summary>
        public static LocalisableString LinkTypeNotSupported => new TranslatableString(getKey(@"unsupported_link_type"), @"This link type is not yet supported!");

        /// <summary>
        /// "{0} invited you to the multiplayer match &quot;{1}&quot;! Click to join."
        /// </summary>
        public static LocalisableString InvitedYouToTheMultiplayer(string username, string roomName) => new TranslatableString(getKey(@"invited_you_to_the_multiplayer"), @"{0} invited you to the multiplayer match ""{1}""! Click to join.", username, roomName);

        /// <summary>
        /// "You do not have the beatmap for this replay."
        /// </summary>
        public static LocalisableString MissingBeatmapForReplay => new TranslatableString(getKey(@"missing_beatmap_for_replay"), @"You do not have the beatmap for this replay.");

        /// <summary>
        /// "Downloading missing beatmap for this replay..."
        /// </summary>
        public static LocalisableString DownloadingBeatmapForReplay => new TranslatableString(getKey(@"downloading_beatmap_for_replay"), @"Downloading missing beatmap for this replay...");

        /// <summary>
        /// "Your local copy of the beatmap for this replay appears to be different than expected. You may need to update or re-download it."
        /// </summary>
        public static LocalisableString MismatchingBeatmapForReplay => new TranslatableString(getKey(@"mismatching_beatmap_for_replay"), @"Your local copy of the beatmap for this replay appears to be different than expected. You may need to update or re-download it.");

        /// <summary>
        /// "You are now running osu! {0}.
        /// Click to see what&#39;s new!"
        /// </summary>
        public static LocalisableString GameVersionAfterUpdate(string version) => new TranslatableString(getKey(@"game_version_after_update"), @"You are now running osu! {0}.
Click to see what's new!", version);

        /// <summary>
        /// "Update ready to install. Click to restart!"
        /// </summary>
        public static LocalisableString UpdateReadyToInstall => new TranslatableString(getKey(@"update_ready_to_install"), @"Update ready to install. Click to restart!");

        /// <summary>
        /// "This is not an official build of the game. Scores will not be submitted and other online systems may not work as intended."
        /// </summary>
        public static LocalisableString NotOfficialBuild => new TranslatableString(getKey(@"not_official_build"), @"This is not an official build of the game. Scores will not be submitted and other online systems may not work as intended.");

        /// <summary>
        /// "Downloading update..."
        /// </summary>
        public static LocalisableString DownloadingUpdate => new TranslatableString(getKey(@"downloading_update"), @"Downloading update...");

        /// <summary>
        /// "This multiplayer room has ended. Click to display room results."
        /// </summary>
        public static LocalisableString MultiplayerRoomEnded => new TranslatableString(getKey(@"multiplayer_room_ended"), @"This multiplayer room has ended. Click to display room results.");

        /// <summary>
        /// "Mention"
        /// </summary>
        public static LocalisableString Mention => new TranslatableString(getKey(@"mention"), @"Mention");

        /// <summary>
        /// "{0} in {1}"
        /// </summary>
        public static LocalisableString MentionDetails(string user, string channelName) => new TranslatableString(getKey(@"mention_details"), @"{0} in {1}", user, channelName);

        /// <summary>
        /// "Online: {0}"
        /// </summary>
        public static LocalisableString FriendOnline(string info) => new TranslatableString(getKey(@"friend_online"), @"Online: {0}", info);

        /// <summary>
        /// "Offline: {0}"
        /// </summary>
        public static LocalisableString FriendOffline(string info) => new TranslatableString(getKey(@"friend_offline"), @"Offline: {0}", info);

        /// <summary>
        /// "Connection to online services was interrupted. osu! will be operating with limited functionality."
        /// </summary>
        public static LocalisableString APIConnectionInterrupted => new TranslatableString(getKey(@"api_connection_interrupted"), @"Connection to online services was interrupted. osu! will be operating with limited functionality.");

        /// <summary>
        /// "You have been logged out on this device due to a login to your account on another device."
        /// </summary>
        public static LocalisableString AnotherDeviceDisconnect => new TranslatableString(getKey(@"another_device_disconnect"), @"You have been logged out on this device due to a login to your account on another device.");

        /// <summary>
        /// "You have been logged out due to a change to your account. Please log in again."
        /// </summary>
        public static LocalisableString AccountChangeDisconnect => new TranslatableString(getKey(@"account_change_disconnect"), @"You have been logged out due to a change to your account. Please log in again.");

        /// <summary>
        /// "Downloading {0}"
        /// </summary>
        public static LocalisableString Downloading(string info) => new TranslatableString(getKey(@"downloading"), @"Downloading {0}", info);

        /// <summary>
        /// "Collections import is initialising..."
        /// </summary>
        public static LocalisableString CollectionsImportInitialising => new TranslatableString(getKey(@"collections_import_initialising"), @"Collections import is initialising...");

        /// <summary>
        /// "Reading collections..."
        /// </summary>
        public static LocalisableString ReadingCollections => new TranslatableString(getKey(@"reading_collections"), @"Reading collections...");

        /// <summary>
        /// "Imported {0} collections"
        /// </summary>
        public static LocalisableString CollectionsImportProgress(int count) => new TranslatableString(getKey(@"collections_import_progress"), @"Imported {0} collections", count);

        /// <summary>
        /// "Imported {0} of {1} collections"
        /// </summary>
        public static LocalisableString CollectionsImportProgressTotal(int count, int totalCount) => new TranslatableString(getKey(@"collections_import_progress_total"), @"Imported {0} of {1} collections", count, totalCount);

        /// <summary>
        /// "This error has been automatically reported to the dev team."
        /// </summary>
        public static LocalisableString ErrorAutomaticallyReported => new TranslatableString(getKey(@"error_automatically_reported"), @"This error has been automatically reported to the dev team.");

        /// <summary>
        /// "A newer release of osu! has been found ({0} → {1})."
        /// </summary>
        public static LocalisableString UpdateAvailable(string oldVersion, string newVersion) => new TranslatableString(getKey(@"update_available"), @"A newer release of osu! has been found ({0} → {1}).", oldVersion, newVersion);

        /// <summary>
        /// "Click here to download the new version, which can be installed over the top of your existing installation."
        /// </summary>
        public static LocalisableString UpdateAvailableManualInstall => new TranslatableString(getKey(@"update_available_manual_install"), @"Click here to download the new version, which can be installed over the top of your existing installation.");

        /// <summary>
        /// "Check with your package manager / provider to bring osu! up-to-date!"
        /// </summary>
        public static LocalisableString UpdateAvailablePackageManaged => new TranslatableString(getKey(@"update_available_package_managed"), @"Check with your package manager / provider to bring osu! up-to-date!");

        /// <summary>
        /// "An action was interrupted due to a dialog being displayed."
        /// </summary>
        public static LocalisableString ActionInterruptedByDialog => new TranslatableString(getKey(@"action_interrupted_by_dialog"), @"An action was interrupted due to a dialog being displayed.");

        /// <summary>
        /// "Exporting {0}..."
        /// </summary>
        public static LocalisableString FileExportOngoing(string filename) => new TranslatableString(getKey(@"file_export_ongoing"), @"Exporting {0}...", filename);

        /// <summary>
        /// "Exported {0}! Click to view."
        /// </summary>
        public static LocalisableString FileExportFinished(string filename) => new TranslatableString(getKey(@"file_export_finished"), @"Exported {0}! Click to view.", filename);

        /// <summary>
        /// "Exporting logs..."
        /// </summary>
        public static LocalisableString LogsExportOngoing => new TranslatableString(getKey(@"logs_export_ongoing"), @"Exporting logs...");

        /// <summary>
        /// "Exported logs! Click to view."
        /// </summary>
        public static LocalisableString LogsExportFinished => new TranslatableString(getKey(@"logs_export_finished"), @"Exported logs! Click to view.");

        /// <summary>
        /// "Running osu! as {0} does not improve performance, may break integrations and poses a security risk. Please run the game as a normal user."
        /// </summary>
        public static LocalisableString ElevatedPrivileges(LocalisableString user) => new TranslatableString(getKey(@"elevated_privileges"), @"Running osu! as {0} does not improve performance, may break integrations and poses a security risk. Please run the game as a normal user.", user);

        /// <summary>
        /// "Screenshot saved! Click to view.
        /// {0}"
        /// </summary>
        public static LocalisableString ScreenshotSaved(string filename) => new TranslatableString(getKey(@"screenshot_saved"), @"Screenshot saved! Click to view.
{0}", filename);

        /// <summary>
        /// "The multiplayer server will be right back..."
        /// </summary>
        public static LocalisableString MultiplayerServerShuttingDownImmediately => new TranslatableString(getKey(@"multiplayer_server_shutting_down_immediately"), @"The multiplayer server will be right back...");

        /// <summary>
        /// "The multiplayer server is restarting in {0}."
        /// </summary>
        public static LocalisableString MultiplayerServerShuttingDownRemaining(string remainingTime) => new TranslatableString(getKey(@"multiplayer_server_shutting_down_remaining"), @"The multiplayer server is restarting in {0}.", remainingTime);

        /// <summary>
        /// "Created new collection &quot;{0}&quot; with {1} beatmaps."
        /// </summary>
        public static LocalisableString CollectionCreated(string name, int beatmapsCount) => new TranslatableString(getKey(@"collection_created"), @"Created new collection ""{0}"" with {1} beatmaps.", name, beatmapsCount);

        /// <summary>
        /// "Added {0} beatmaps to collection &quot;{1}&quot;."
        /// </summary>
        public static LocalisableString CollectionBeatmapsAdded(string name, int beatmapsCount) => new TranslatableString(getKey(@"collection_beatmaps_added"), @"Added {0} beatmaps to collection ""{1}"".", beatmapsCount, name);

        /// <summary>
        /// "Reprocessing star rating for beatmaps"
        /// </summary>
        public static LocalisableString ReprocessStarRatingRunning => new TranslatableString(getKey(@"reprocess_star_rating_running"), @"Reprocessing star rating for beatmaps");

        /// <summary>
        /// "{0} beatmaps&#39; star ratings have been updated."
        /// </summary>
        public static LocalisableString ReprocessStarRatingCompleted(int processedCount) => new TranslatableString(getKey(@"reprocess_star_rating_completed"), @"{0} beatmaps' star ratings have been updated.", processedCount);

        /// <summary>
        /// "{0} of {1} beatmaps&#39; star ratings have been updated."
        /// </summary>
        public static LocalisableString ReprocessStarRatingIncompleted(int processedCount, int totalCount) => new TranslatableString(getKey(@"reprocess_star_rating_incompleted"), @"{0} of {1} beatmaps' star ratings have been updated.", processedCount, totalCount);

        /// <summary>
        /// "Updating online data for beatmaps"
        /// </summary>
        public static LocalisableString UpdateOnlineDataRunning => new TranslatableString(getKey(@"update_online_data_running"), @"Updating online data for beatmaps");

        /// <summary>
        /// "{0} beatmaps&#39; online data have been updated."
        /// </summary>
        public static LocalisableString UpdateOnlineDataCompleted(int processedCount) => new TranslatableString(getKey(@"update_online_data_completed"), @"{0} beatmaps' online data have been updated.", processedCount);

        /// <summary>
        /// "{0} of {1} beatmaps&#39; online data have been updated."
        /// </summary>
        public static LocalisableString UpdateOnlineDataIncompleted(int processedCount, int totalCount) => new TranslatableString(getKey(@"update_online_data_incompleted"), @"{0} of {1} beatmaps' online data have been updated.", processedCount, totalCount);

        /// <summary>
        /// "Populating missing statistics for beatmaps"
        /// </summary>
        public static LocalisableString PopulateBeatmapsStatsRunning => new TranslatableString(getKey(@"populate_beatmaps_stats_running"), @"Populating missing statistics for beatmaps");

        /// <summary>
        /// "{0} beatmaps have been populated with missing statistics."
        /// </summary>
        public static LocalisableString PopulateBeatmapsStatsCompleted(int processedCount) => new TranslatableString(getKey(@"populate_beatmaps_stats_completed"), @"{0} beatmaps have been populated with missing statistics.", processedCount);

        /// <summary>
        /// "{0} of {1} beatmaps have been populated with missing statistics."
        /// </summary>
        public static LocalisableString PopulateBeatmapsStatsIncompleted(int processedCount, int totalCount) => new TranslatableString(getKey(@"populate_beatmaps_stats_incompleted"), @"{0} of {1} beatmaps have been populated with missing statistics.", processedCount, totalCount);

        /// <summary>
        /// "Populating missing statistics for scores"
        /// </summary>
        public static LocalisableString PopulateScoresStatsRunning => new TranslatableString(getKey(@"populate_scores_stats_running"), @"Populating missing statistics for scores");

        /// <summary>
        /// "{0} scores have been populated with missing statistics."
        /// </summary>
        public static LocalisableString PopulateScoresStatsCompleted(int processedCount) => new TranslatableString(getKey(@"populate_scores_stats_completed"), @"{0} scores have been populated with missing statistics.", processedCount);

        /// <summary>
        /// "{0} of {1} scores have been populated with missing statistics."
        /// </summary>
        public static LocalisableString PopulateScoresStatsIncompleted(int processedCount, int totalCount) => new TranslatableString(getKey(@"populate_scores_stats_incompleted"), @"{0} of {1} scores have been populated with missing statistics.", processedCount, totalCount);

        /// <summary>
        /// "Upgrading scores to new scoring algorithm"
        /// </summary>
        public static LocalisableString UpgradeScoringAlgorithmRunning => new TranslatableString(getKey(@"upgrade_scoring_algorithm_running"), @"Upgrading scores to new scoring algorithm");

        /// <summary>
        /// "{0} scores have been upgraded to the new scoring algorithm."
        /// </summary>
        public static LocalisableString UpgradeScoringAlgorithmCompleted(int processedCount) => new TranslatableString(getKey(@"upgrade_scoring_algorithm_completed"), @"{0} scores have been upgraded to the new scoring algorithm.", processedCount);

        /// <summary>
        /// "{0} of {1} scores have been upgraded to the new scoring algorithm."
        /// </summary>
        public static LocalisableString UpgradeScoringAlgorithmIncompleted(int processedCount, int totalCount) => new TranslatableString(getKey(@"upgrade_scoring_algorithm_incompleted"), @"{0} of {1} scores have been upgraded to the new scoring algorithm.", processedCount, totalCount);

        /// <summary>
        /// "Adjusting ranks of scores"
        /// </summary>
        public static LocalisableString AdjustScoresRanksRunning => new TranslatableString(getKey(@"adjust_scores_ranks_running"), @"Adjusting ranks of scores");

        /// <summary>
        /// "{0} scores now have more correct ranks."
        /// </summary>
        public static LocalisableString AdjustScoresRanksCompleted(int processedCount) => new TranslatableString(getKey(@"adjust_scores_ranks_completed"), @"{0} scores now have more correct ranks.", processedCount);

        /// <summary>
        /// "{0} of {1} scores now have more correct ranks."
        /// </summary>
        public static LocalisableString AdjustScoresRanksIncompleted(int processedCount, int totalCount) => new TranslatableString(getKey(@"adjust_scores_ranks_incompleted"), @"{0} of {1} scores now have more correct ranks.", processedCount, totalCount);

        /// <summary>
        /// "Populating missing submission and rank dates"
        /// </summary>
        public static LocalisableString PopulateMissingDatesRunning => new TranslatableString(getKey(@"populate_missing_dates_running"), @"Populating missing submission and rank dates");

        /// <summary>
        /// "{0} beatmap sets now have correct submission and rank dates."
        /// </summary>
        public static LocalisableString PopulateMissingDatesCompleted(int processedCount) => new TranslatableString(getKey(@"populate_missing_dates_completed"), @"{0} beatmap sets now have correct submission and rank dates.", processedCount);

        /// <summary>
        /// "{0} of {1} beatmap sets now have correct submission and rank dates."
        /// </summary>
        public static LocalisableString PopulateMissingDatesIncompleted(int processedCount, int totalCount) => new TranslatableString(getKey(@"populate_missing_dates_incompleted"), @"{0} of {1} beatmap sets now have correct submission and rank dates.", processedCount, totalCount);

        /// <summary>
        /// "Populating missing user tags"
        /// </summary>
        public static LocalisableString PopulateMissingTagsRunning => new TranslatableString(getKey(@"populate_missing_tags_running"), @"Populating missing user tags");

        /// <summary>
        /// "{0} beatmaps have had their tags updated."
        /// </summary>
        public static LocalisableString PopulateMissingTagsCompleted(int processedCount) => new TranslatableString(getKey(@"populate_missing_tags_completed"), @"{0} beatmaps have had their tags updated.", processedCount);

        /// <summary>
        /// "{0} of {1} beatmaps have had their tags updated."
        /// </summary>
        public static LocalisableString PopulateMissingTagsIncompleted(int processedCount, int totalCount) => new TranslatableString(getKey(@"populate_missing_tags_incompleted"), @"{0} of {1} beatmaps have had their tags updated.", processedCount, totalCount);

        /// <summary>
        /// "({0} of {1})"
        /// </summary>
        public static LocalisableString BackgroundDataStoreProcessorItemsStatus(int processedItems, int totalItems) => new TranslatableString(getKey(@"data_store_processor_items_status"), @"({0} of {1})", processedItems, totalItems);

        /// <summary>
        /// "Check logs for issues with {0} failed items."
        /// </summary>
        public static LocalisableString BackgroundDataStoreProcessorFailedItems(int failedCount) => new TranslatableString(getKey(@"data_store_processor_failed_items"), @"Check logs for issues with {0} failed items.", failedCount);

        /// <summary>
        /// "All offsets have been reset!"
        /// </summary>
        public static LocalisableString RestoreBeatmapOffsetsCompleted => new TranslatableString(getKey(@"restore_beatmap_offsets_completed"), @"All offsets have been reset!");

        /// <summary>
        /// "No videos found to delete!"
        /// </summary>
        public static LocalisableString DeleteBeatmapVideosAborted => new TranslatableString(getKey(@"delete_beatmap_videos_aborted"), @"No videos found to delete!");

        /// <summary>
        /// "Preparing to delete all beatmap videos..."
        /// </summary>
        public static LocalisableString DeleteBeatmapVideosStarting => new TranslatableString(getKey(@"delete_beatmap_videos_starting"), @"Preparing to delete all beatmap videos...");

        /// <summary>
        /// "Deleting videos from beatmaps ({0} deleted)"
        /// </summary>
        public static LocalisableString DeleteBeatmapVideosRunning(int deletedCount) => new TranslatableString(getKey(@"delete_beatmap_videos_running"), @"Deleting videos from beatmaps ({0} deleted)", deletedCount);

        /// <summary>
        /// "Deleted {0} beatmap video(s)!"
        /// </summary>
        public static LocalisableString DeleteBeatmapVideosCompleted(int deletedCount) => new TranslatableString(getKey(@"delete_beatmap_videos_completed"), @"Deleted {0} beatmap video(s)!", deletedCount);

        /// <summary>
        /// "No beatmaps found to delete!"
        /// </summary>
        public static LocalisableString DeleteBeatmapsAborted => new TranslatableString(getKey(@"delete_beatmaps_aborted"), @"No beatmaps found to delete!");

        /// <summary>
        /// "Preparing to delete all beatmaps..."
        /// </summary>
        public static LocalisableString DeleteBeatmapsStarting => new TranslatableString(getKey(@"delete_beatmaps_starting"), @"Preparing to delete all beatmaps...");

        /// <summary>
        /// "Deleting beatmaps ({0} of {1})"
        /// </summary>
        public static LocalisableString DeleteBeatmapsRunning(int processedCount, int totalCount) => new TranslatableString(getKey(@"delete_beatmaps_running"), @"Deleting beatmaps ({0} of {1})", processedCount, totalCount);

        /// <summary>
        /// "Deleted all beatmaps!"
        /// </summary>
        public static LocalisableString DeleteBeatmapsCompleted => new TranslatableString(getKey(@"delete_beatmaps_completed"), @"Deleted all beatmaps!");

        /// <summary>
        /// "No beatmaps found to restore!"
        /// </summary>
        public static LocalisableString RestoreBeatmapsAborted => new TranslatableString(getKey(@"restore_beatmaps_aborted"), @"No beatmaps found to restore!");

        /// <summary>
        /// "Restoring deleted beatmaps ({0} of {1})"
        /// </summary>
        public static LocalisableString RestoreBeatmapsRunning(int processedCount, int totalCount) => new TranslatableString(getKey(@"restore_beatmaps_running"), @"Restoring deleted beatmaps ({0} of {1})", processedCount, totalCount);

        /// <summary>
        /// "Restored all deleted beatmaps!"
        /// </summary>
        public static LocalisableString RestoreBeatmapsCompleted => new TranslatableString(getKey(@"restore_beatmaps_completed"), @"Restored all deleted beatmaps!");

        /// <summary>
        /// "No scores found to delete!"
        /// </summary>
        public static LocalisableString DeleteScoresAborted => new TranslatableString(getKey(@"delete_scores_aborted"), @"No scores found to delete!");

        /// <summary>
        /// "Preparing to delete all scores..."
        /// </summary>
        public static LocalisableString DeleteScoresStarting => new TranslatableString(getKey(@"delete_scores_starting"), @"Preparing to delete all scores...");

        /// <summary>
        /// "Deleting scores ({0} of {1})"
        /// </summary>
        public static LocalisableString DeleteScoresRunning(int processedCount, int totalCount) => new TranslatableString(getKey(@"delete_scores_running"), @"Deleting scores ({0} of {1})", processedCount, totalCount);

        /// <summary>
        /// "Deleted all scores!"
        /// </summary>
        public static LocalisableString DeleteScoresCompleted => new TranslatableString(getKey(@"delete_scores_completed"), @"Deleted all scores!");

        /// <summary>
        /// "No scores found to restore!"
        /// </summary>
        public static LocalisableString RestoreScoresAborted => new TranslatableString(getKey(@"restore_scores_aborted"), @"No scores found to restore!");

        /// <summary>
        /// "Restoring deleted scores ({0} of {1})"
        /// </summary>
        public static LocalisableString RestoreScoresRunning(int processedCount, int totalCount) => new TranslatableString(getKey(@"restore_scores_running"), @"Restoring deleted scores ({0} of {1})", processedCount, totalCount);

        /// <summary>
        /// "Restored all deleted scores!"
        /// </summary>
        public static LocalisableString RestoreScoresCompleted => new TranslatableString(getKey(@"restore_scores_completed"), @"Restored all deleted scores!");

        /// <summary>
        /// "No skins found to delete!"
        /// </summary>
        public static LocalisableString DeleteSkinsAborted => new TranslatableString(getKey(@"delete_skins_aborted"), @"No skins found to delete!");

        /// <summary>
        /// "Preparing to delete all skins..."
        /// </summary>
        public static LocalisableString DeleteSkinsStarting => new TranslatableString(getKey(@"delete_skins_starting"), @"Preparing to delete all skins...");

        /// <summary>
        /// "Deleting skins ({0} of {1})"
        /// </summary>
        public static LocalisableString DeleteSkinsRunning(int processedCount, int totalCount) => new TranslatableString(getKey(@"delete_skins_running"), @"Deleting skins ({0} of {1})", processedCount, totalCount);

        /// <summary>
        /// "Deleted all skins!"
        /// </summary>
        public static LocalisableString DeleteSkinsCompleted => new TranslatableString(getKey(@"delete_skins_completed"), @"Deleted all skins!");

        /// <summary>
        /// "No skins found to restore!"
        /// </summary>
        public static LocalisableString RestoreSkinsAborted => new TranslatableString(getKey(@"restore_skins_aborted"), @"No skins found to restore!");

        /// <summary>
        /// "Restoring deleted skins ({0} of {1})"
        /// </summary>
        public static LocalisableString RestoreSkinsRunning(int processedCount, int totalCount) => new TranslatableString(getKey(@"restore_skins_running"), @"Restoring deleted skins ({0} of {1})", processedCount, totalCount);

        /// <summary>
        /// "Restored all deleted skins!"
        /// </summary>
        public static LocalisableString RestoreSkinsCompleted => new TranslatableString(getKey(@"restore_skins_completed"), @"Restored all deleted skins!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
