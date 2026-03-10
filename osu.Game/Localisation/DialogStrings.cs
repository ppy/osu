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
        /// "Are you sure you want to exit osu!?"
        /// </summary>
        public static LocalisableString ConfirmExitHeaderText => new TranslatableString(getKey(@"confirm_exit_header_text"), @"Are you sure you want to exit osu!?");

        /// <summary>
        /// "Last chance to turn back"
        /// </summary>
        public static LocalisableString ConfirmDialogBodyText => new TranslatableString(getKey(@"confirm_exit_body_text"), @"Last chance to turn back");

        /// <summary>
        /// "There are currently some background operations which will be aborted if you continue:
        ///
        /// {0}"
        /// </summary>
        public static LocalisableString ConfirmExitBodyTextOngoingOperations(string ongoingOperationsText) => new TranslatableString(getKey(@"confirm_exit_body_text_ongoing_operations"), @"There are currently some background operations which will be aborted if you continue:

{0}", ongoingOperationsText);

        /// <summary>
        /// "There are currently some background operations which will be aborted if you continue:
        ///
        /// {0}
        ///
        /// and {1} other operation(s)."
        /// </summary>
        public static LocalisableString ConfirmExitBodyTextOtherOngoingOperations(string ongoingOperationsText, int count) => new TranslatableString(getKey(@"confirm_exit_body_text_other_ongoing_operations"), @"There are currently some background operations which will be aborted if you continue:

{0}

and {1} other operation(s).", ongoingOperationsText, count);

        /// <summary>
        /// "Let me out!"
        /// </summary>
        public static LocalisableString ConfirmExitOkButton => new TranslatableString(getKey(@"confirm_exit_ok_button"), @"Let me out!");

        /// <summary>
        /// "Just a little more..."
        /// </summary>
        public static LocalisableString ConfirmExitCancelButton => new TranslatableString(getKey(@"confirm_exit_cancel_button"), @"Just a little more...");

        /// <summary>
        /// "Are you sure you want to go back?"
        /// </summary>
        public static LocalisableString ConfirmDiscardChangesHeaderText => new TranslatableString(getKey(@"confirm_discard_changes_header_text"), @"Are you sure you want to go back?");

        /// <summary>
        /// "This will discard any unsaved changes"
        /// </summary>
        public static LocalisableString ConfirmDiscardChangesBodyText => new TranslatableString(getKey(@"confirm_discard_changes_body_text"), @"This will discard any unsaved changes");

        /// <summary>
        /// "No I didn't mean to"
        /// </summary>
        public static LocalisableString ConfirmDiscardChangesCancelButton => new TranslatableString(getKey(@"confirm_discard_changes_cancel_button"), @"No I didn't mean to");

        /// <summary>
        /// "Are you sure you want to open the following link in a web browser?
        ///
        /// {0}"
        /// </summary>
        public static LocalisableString ExternalLinkBodyText(string url) => new TranslatableString(getKey(@"external_link_body_text"), @"Are you sure you want to open the following link in a web browser?

{0}", url);

        /// <summary>
        /// "Open in browser"
        /// </summary>
        public static LocalisableString ExternalLinkOkButton => new TranslatableString(getKey(@"external_link_ok_button"), @"Open in browser");

        /// <summary>
        /// "Do you really want to delete your comment?"
        /// </summary>
        public static LocalisableString DeleteCommentBodyText => new TranslatableString(getKey(@"delete_comment_body_text"), @"Do you really want to delete your comment?");

        /// <summary>
        /// "Are you sure you want to leave this multiplayer match?"
        /// </summary>
        public static LocalisableString ConfirmExitMultiplayerMatchBodyText => new TranslatableString(getKey(@"confirm_exit_multiplayer_match_body_text"), @"Are you sure you want to leave this multiplayer match?");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
