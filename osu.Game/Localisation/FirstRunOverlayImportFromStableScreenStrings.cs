// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class FirstRunOverlayImportFromStableScreenStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ScreenImportFromStable";

        /// <summary>
        /// "Import"
        /// </summary>
        public static LocalisableString Header => new TranslatableString(getKey(@"header"), @"Import");

        /// <summary>
        /// "If you have an installation of a previous osu! version, you can choose to migrate your existing content. Note that this will not affect your existing installation&#39;s files in any way."
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"), @"If you have an installation of a previous osu! version, you can choose to migrate your existing content. Note that this will not affect your existing installation's files in any way.");

        /// <summary>
        /// "previous osu! install"
        /// </summary>
        public static LocalisableString LocateDirectoryLabel => new TranslatableString(getKey(@"locate_directory_label"), @"previous osu! install");

        /// <summary>
        /// "Click to locate a previous osu! install"
        /// </summary>
        public static LocalisableString LocateDirectoryPlaceholder => new TranslatableString(getKey(@"locate_directory_placeholder"), @"Click to locate a previous osu! install");

        /// <summary>
        /// "Import content from previous version"
        /// </summary>
        public static LocalisableString ImportButton => new TranslatableString(getKey(@"import_button"), @"Import content from previous version");

        /// <summary>
        /// "Your import will continue in the background. Check on its progress in the notifications sidebar!"
        /// </summary>
        public static LocalisableString ImportInProgress => new TranslatableString(getKey(@"import_in_progress"), @"Your import will continue in the background. Check on its progress in the notifications sidebar!");

        /// <summary>
        /// "calculating..."
        /// </summary>
        public static LocalisableString Calculating => new TranslatableString(getKey(@"calculating"), @"calculating...");

        /// <summary>
        /// "{0} item(s)"
        /// </summary>
        public static LocalisableString Items(int arg0) => new TranslatableString(getKey(@"items"), @"{0} item(s)", arg0);

        /// <summary>
        /// "Data migration will use &quot;hard links&quot;. No extra disk space will be used, and you can delete either data folder at any point without affecting the other installation."
        /// </summary>
        public static LocalisableString DataMigrationNoExtraSpace => new TranslatableString(getKey(@"data_migration_no_extra_space"), @"Data migration will use ""hard links"". No extra disk space will be used, and you can delete either data folder at any point without affecting the other installation.");

        /// <summary>
        /// "Learn more about how &quot;hard links&quot; work"
        /// </summary>
        public static LocalisableString LearnAboutHardLinks => new TranslatableString(getKey(@"learn_about_hard_links"), @"Learn more about how ""hard links"" work");

        /// <summary>
        /// "Lightweight linking of files is not supported on your operating system yet, so a copy of all files will be made during import."
        /// </summary>
        public static LocalisableString LightweightLinkingNotSupported => new TranslatableString(getKey(@"lightweight_linking_not_supported"), @"Lightweight linking of files is not supported on your operating system yet, so a copy of all files will be made during import.");

        /// <summary>
        /// "A second copy of all files will be made during import. To avoid this, please make sure the lazer data folder is on the same drive as your previous osu! install (and the file system is NTFS)."
        /// </summary>
        public static LocalisableString SecondCopyWillBeMadeWindows => new TranslatableString(getKey(@"second_copy_will_be_made_windows"), @"A second copy of all files will be made during import. To avoid this, please make sure the lazer data folder is on the same drive as your previous osu! install (and the file system is NTFS).");

        /// <summary>
        /// "A second copy of all files will be made during import. To avoid this, please make sure the lazer data folder is on the same drive as your previous osu! install (and the file system supports hard links)."
        /// </summary>
        public static LocalisableString SecondCopyWillBeMadeOtherPlatforms => new TranslatableString(getKey(@"second_copy_will_be_made_other_platforms"), @"A second copy of all files will be made during import. To avoid this, please make sure the lazer data folder is on the same drive as your previous osu! install (and the file system supports hard links).");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
