// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class OnlineSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.OnlineSettings";

        /// <summary>
        /// "Online"
        /// </summary>
        public static LocalisableString OnlineSectionHeader => new TranslatableString(getKey(@"online_section_header"), @"Online");

        /// <summary>
        /// "Alerts and Privacy"
        /// </summary>
        public static LocalisableString AlertsAndPrivacyHeader => new TranslatableString(getKey(@"alerts_and_privacy_header"), @"Alerts and Privacy");

        /// <summary>
        /// "Show a notification when someone mentions your name"
        /// </summary>
        public static LocalisableString NotifyOnMentioned => new TranslatableString(getKey(@"notify_on_mentioned"), @"Show a notification when someone mentions your name");

        /// <summary>
        /// "Show a notification when you receive a private message"
        /// </summary>
        public static LocalisableString NotifyOnPrivateMessage => new TranslatableString(getKey(@"notify_on_private_message"), @"Show a notification when you receive a private message");

        /// <summary>
        /// "Show notification popups when friends change status"
        /// </summary>
        public static LocalisableString NotifyOnFriendPresenceChange => new TranslatableString(getKey(@"notify_on_friend_presence_change"), @"Show notification popups when friends change status");

        /// <summary>
        /// "Notifications will be shown when friends go online/offline."
        /// </summary>
        public static LocalisableString NotifyOnFriendPresenceChangeTooltip => new TranslatableString(getKey(@"notify_on_friend_presence_change_tooltip"), @"Notifications will be shown when friends go online/offline.");

        /// <summary>
        /// "Integrations"
        /// </summary>
        public static LocalisableString IntegrationsHeader => new TranslatableString(getKey(@"integrations_header"), @"Integrations");

        /// <summary>
        /// "Discord Rich Presence"
        /// </summary>
        public static LocalisableString DiscordRichPresence => new TranslatableString(getKey(@"discord_rich_presence"), @"Discord Rich Presence");

        /// <summary>
        /// "Web"
        /// </summary>
        public static LocalisableString WebHeader => new TranslatableString(getKey(@"web_header"), @"Web");

        /// <summary>
        /// "Warn about opening external links"
        /// </summary>
        public static LocalisableString ExternalLinkWarning => new TranslatableString(getKey(@"external_link_warning"), @"Warn about opening external links");

        /// <summary>
        /// "Prefer downloads without video"
        /// </summary>
        public static LocalisableString PreferNoVideo => new TranslatableString(getKey(@"prefer_no_video"), @"Prefer downloads without video");

        /// <summary>
        /// "Automatically download missing beatmaps"
        /// </summary>
        public static LocalisableString AutomaticallyDownloadMissingBeatmaps => new TranslatableString(getKey(@"automatically_download_missing_beatmaps"), @"Automatically download missing beatmaps");

        /// <summary>
        /// "Show explicit content in search results"
        /// </summary>
        public static LocalisableString ShowExplicitContent => new TranslatableString(getKey(@"show_explicit_content"), @"Show explicit content in search results");

        /// <summary>
        /// "Hide identifiable information"
        /// </summary>
        public static LocalisableString HideIdentifiableInformation => new TranslatableString(getKey(@"hide_identifiable_information"), @"Hide identifiable information");

        /// <summary>
        /// "Full"
        /// </summary>
        public static LocalisableString DiscordPresenceFull => new TranslatableString(getKey(@"discord_presence_full"), @"Full");

        /// <summary>
        /// "Off"
        /// </summary>
        public static LocalisableString DiscordPresenceOff => new TranslatableString(getKey(@"discord_presence_off"), @"Off");

        /// <summary>
        /// "Hide country flags"
        /// </summary>
        public static LocalisableString HideCountryFlags => new TranslatableString(getKey(@"hide_country_flags"), @"Hide country flags");

        /// <summary>
        /// "Custom API server URL"
        /// </summary>
        public static LocalisableString CustomApiUrl => new TranslatableString(getKey(@"custom_api_url"), @"Custom API server URL");

        /// <summary>
        /// "A restart is required for this setting to take effect."
        /// </summary>
        public static LocalisableString CustomApiUrlRestartRequired => new TranslatableString(getKey(@"custom_api_url_restart_required"), @"A restart is required for this setting to take effect.");

        /// <summary>
        /// "The game will be restarted to apply the new API server settings."
        /// </summary>
        public static LocalisableString CustomApiUrlRestartMessage => new TranslatableString(getKey(@"custom_api_url_restart_message"), @"The game will be restarted to apply the new API server settings.");

        /// <summary>
        /// "Invalid custom API server address. Enter only a hostname, optionally with a port. Paths are not allowed."
        /// </summary>
        public static LocalisableString CustomApiUrlInvalid => new TranslatableString(
            getKey(@"custom_api_url_invalid"),
            @"Invalid custom API server address. Enter only a hostname, optionally with a port. Paths are not allowed."
        );

        /// <summary>
        /// "Connected to default server"
        /// </summary>
        public static LocalisableString ConnectedToDefaultServer => new TranslatableString(getKey(@"connected_to_default_server"), @"Connected to default server");

        /// <summary>
        /// "Current server: {0}"
        /// </summary>
        public static LocalisableString CurrentServer(string serverName) => new TranslatableString(getKey(@"current_server"), @"Current server: {0}", serverName);

        /// <summary>
        /// "Official server (osu.ppy.sh)"
        /// </summary>
        public static LocalisableString OfficialServer => new TranslatableString(getKey(@"official_server"), @"Official server (osu.ppy.sh)");

        /// <summary>
        /// "Development server (dev.ppy.sh)"
        /// </summary>
        public static LocalisableString DevelopmentServer => new TranslatableString(getKey(@"development_server"), @"Development server (dev.ppy.sh)");

        /// <summary>
        /// "Default server"
        /// </summary>
        public static LocalisableString DefaultServer => new TranslatableString(getKey(@"default_server"), @"Default server");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
