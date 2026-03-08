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

        /// <summary>
        /// "All local scores on {0}"
        /// </summary>
        public static LocalisableString BeatmapClearScoresBodyText(string difficulty) => new TranslatableString(getKey(@"beatmap_clear_scores_body_text"), @"All local scores on {0}", difficulty);

        /// <summary>
        /// "Are you sure you want to close the following playlist:"
        /// </summary>
        public static LocalisableString ClosePlaylistHeaderText => new TranslatableString(getKey(@"close_playlist_header_text"), @"Are you sure you want to close the following playlist:");

        /// <summary>
        /// "Are you sure you want to abort the match?"
        /// </summary>
        public static LocalisableString ConfirmAbortMatchHeaderText => new TranslatableString(getKey(@"confirm_abort_match_header_text"), @"Are you sure you want to abort the match?");

        /// <summary>
        /// "Understood"
        /// </summary>
        public static LocalisableString MobileDisclaimerOkButton => new TranslatableString(getKey(@"mobile_disclaimer_ok_button"), @"Understood");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
