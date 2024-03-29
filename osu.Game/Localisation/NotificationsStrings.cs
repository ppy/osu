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
        /// "You received a private message from '{0}'. Click to read it!"
        /// </summary>
        public static LocalisableString PrivateMessageReceived(string username) => new TranslatableString(getKey(@"private_message_received"), @"You received a private message from '{0}'. Click to read it!", username);

        /// <summary>
        /// "Your name was mentioned in chat by '{0}'. Click to find out why!"
        /// </summary>
        public static LocalisableString YourNameWasMentioned(string username) => new TranslatableString(getKey(@"your_name_was_mentioned"), @"Your name was mentioned in chat by '{0}'. Click to find out why!", username);

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
        /// "You are now running osu! {version}.
        /// Click to see what's new!"
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
        /// "Installing update..."
        /// </summary>
        public static LocalisableString InstallingUpdate => new TranslatableString(getKey(@"installing_update"), @"Installing update...");

        /// <summary>
        /// "Failed to paste objects: Invalid clipboard content."
        /// </summary>
        public static LocalisableString InvalidHitObjectClipboardContent => new TranslatableString(getKey(@"invalid_hitobject_clipboard_content"), @"Failed to paste objects: Invalid clipboard content.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
