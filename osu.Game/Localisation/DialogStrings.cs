// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DialogStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Dialog";

        /// <summary>
        /// "Caution"
        /// </summary>
        public static LocalisableString CautionHeaderText => new TranslatableString(getKey(@"header_text"), @"Caution");

        /// <summary>
        /// "Are you sure you want to delete the following:"
        /// </summary>
        public static LocalisableString DeletionHeaderText => new TranslatableString(getKey(@"deletion_header_text"), @"Are you sure you want to delete the following:");

        /// <summary>
        /// "Yes. Go for it."
        /// </summary>
        public static LocalisableString Confirm => new TranslatableString(getKey(@"confirm"), @"Yes. Go for it.");

        /// <summary>
        /// "No! Abort mission"
        /// </summary>
        public static LocalisableString Cancel => new TranslatableString(getKey(@"cancel"), @"No! Abort mission");

        /// <summary>
        /// "Failed to automatically locate an osu!stable installation."
        /// </summary>
        public static LocalisableString StableDirectoryLocationHeaderText => new TranslatableString(getKey(@"stable_directory_location_header_text"), @"Failed to automatically locate an osu!stable installation.");

        /// <summary>
        /// "An existing install could not be located. If you know where it is, you can help locate it."
        /// </summary>
        public static LocalisableString StableDirectoryLocationBodyText => new TranslatableString(getKey(@"stable_directory_location_body_text"), @"An existing install could not be located. If you know where it is, you can help locate it.");

        /// <summary>
        /// "Sure! I know where it is located!"
        /// </summary>
        public static LocalisableString StableDirectoryLocationOkButton => new TranslatableString(getKey(@"stable_directory_location_ok_button"), @"Sure! I know where it is located!");

        /// <summary>
        /// "Actually I don't have osu!stable installed."
        /// </summary>
        public static LocalisableString StableDirectoryLocationCancelButton => new TranslatableString(getKey(@"stable_directory_location_cancel_button"), @"Actually I don't have osu!stable installed.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
