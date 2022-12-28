// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class GeneralSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.GeneralSettings";

        /// <summary>
        /// "General"
        /// </summary>
        public static LocalisableString GeneralSectionHeader => new TranslatableString(getKey(@"general_section_header"), @"General");

        /// <summary>
        /// "Language"
        /// </summary>
        public static LocalisableString LanguageHeader => new TranslatableString(getKey(@"language_header"), @"Language");

        /// <summary>
        /// "Language"
        /// </summary>
        public static LocalisableString LanguageDropdown => new TranslatableString(getKey(@"language_dropdown"), @"Language");

        /// <summary>
        /// "偏好以原语言显示谱面信息"
        /// </summary>
        public static LocalisableString PreferOriginalMetadataLanguage => new TranslatableString(getKey(@"prefer_original"), @"偏好以原语言显示谱面信息");

        /// <summary>
        /// "使用24小时制"
        /// </summary>
        public static LocalisableString Prefer24HourTimeDisplay => new TranslatableString(getKey(@"prefer_24_hour_time_display"), @"使用24小时制");

        /// <summary>
        /// "Updates"
        /// </summary>
        public static LocalisableString UpdateHeader => new TranslatableString(getKey(@"update_header"), @"Updates");

        /// <summary>
        /// "Release stream"
        /// </summary>
        public static LocalisableString ReleaseStream => new TranslatableString(getKey(@"release_stream"), @"Release stream");

        /// <summary>
        /// "Check for updates"
        /// </summary>
        public static LocalisableString CheckUpdate => new TranslatableString(getKey(@"check_update"), @"Check for updates");

        /// <summary>
        /// "Open osu! folder"
        /// </summary>
        public static LocalisableString OpenOsuFolder => new TranslatableString(getKey(@"open_osu_folder"), @"Open osu! folder");

        /// <summary>
        /// "Change folder location..."
        /// </summary>
        public static LocalisableString ChangeFolderLocation => new TranslatableString(getKey(@"change_folder_location"), @"Change folder location...");

        /// <summary>
        /// "打开设置向导"
        /// </summary>
        public static LocalisableString RunSetupWizard => new TranslatableString(getKey(@"llin_run_setup_wizard"), @"打开设置向导");

        /// <summary>
        /// "Learn more about lazer"
        /// </summary>
        public static LocalisableString LearnMoreAboutLazer => new TranslatableString(getKey(@"learn_more_about_lazer"), @"Learn more about lazer");

        /// <summary>
        /// "Check out the feature comparison and FAQ"
        /// </summary>
        public static LocalisableString LearnMoreAboutLazerTooltip => new TranslatableString(getKey(@"check_out_the_feature_comparison"), @"Check out the feature comparison and FAQ");

        /// <summary>
        /// "You are running the latest release ({0})"
        /// </summary>
        public static LocalisableString RunningLatestRelease(string version) => new TranslatableString(getKey(@"running_latest_release"), @"You are running the latest release ({0})", version);

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
