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
        /// "If you have an installation of a previous osu! version, you can choose to migrate your existing content. Note that this will not affect your existing installation's files in any way."
        /// </summary>
        public static LocalisableString Description => new TranslatableString(getKey(@"description"),
            @"If you have an installation of a previous osu! version, you can choose to migrate your existing content. Note that this will not affect your existing installation's files in any way.");

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
        public static LocalisableString ImportInProgress =>
            new TranslatableString(getKey(@"import_in_progress"), @"Your import will continue in the background. Check on its progress in the notifications sidebar!");

        /// <summary>
        /// "calculating..."
        /// </summary>
        public static LocalisableString Calculating => new TranslatableString(getKey(@"calculating"), @"calculating...");

        /// <summary>
        /// "{0} items"
        /// </summary>
        public static LocalisableString Items(int arg0) => new TranslatableString(getKey(@"items"), @"{0} item(s)", arg0);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
