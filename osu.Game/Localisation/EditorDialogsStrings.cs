// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class EditorDialogsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.EditorDialogs";

        /// <summary>
        /// "Would you like to create a blank difficulty?"
        /// </summary>
        public static LocalisableString NewDifficultyDialogHeader => new TranslatableString(getKey(@"new_difficulty_dialog_header"), @"Would you like to create a blank difficulty?");

        /// <summary>
        /// "Yeah, let&#39;s start from scratch!"
        /// </summary>
        public static LocalisableString CreateNew => new TranslatableString(getKey(@"create_new"), @"Yeah, let's start from scratch!");

        /// <summary>
        /// "No, create an exact copy of this difficulty"
        /// </summary>
        public static LocalisableString CreateCopy => new TranslatableString(getKey(@"create_copy"), @"No, create an exact copy of this difficulty");

        /// <summary>
        /// "I changed my mind, I want to keep editing this difficulty"
        /// </summary>
        public static LocalisableString KeepEditing => new TranslatableString(getKey(@"keep_editing"), @"I changed my mind, I want to keep editing this difficulty");

        /// <summary>
        /// "Did you want to save your changes?"
        /// </summary>
        public static LocalisableString SaveDialogHeader => new TranslatableString(getKey(@"save_dialog_header"), @"Did you want to save your changes?");

        /// <summary>
        /// "Save my masterpiece!"
        /// </summary>
        public static LocalisableString Save => new TranslatableString(getKey(@"save"), @"Save my masterpiece!");

        /// <summary>
        /// "Forget all changes"
        /// </summary>
        public static LocalisableString ForgetAllChanges => new TranslatableString(getKey(@"forget_all_changes"), @"Forget all changes");

        /// <summary>
        /// "Oops, continue editing"
        /// </summary>
        public static LocalisableString ContinueEditing => new TranslatableString(getKey(@"continue_editing"), @"Oops, continue editing");

        /// <summary>
        /// "The editor must be reloaded to apply this change. The beatmap will be saved."
        /// </summary>
        public static LocalisableString EditorReloadDialogHeader => new TranslatableString(getKey(@"editor_reload_dialog_header"), @"The editor must be reloaded to apply this change. The beatmap will be saved.");

        /// <summary>
        /// "Discard all unsaved changes? This cannot be undone."
        /// </summary>
        public static LocalisableString DiscardUnsavedChangesDialogHeader => new TranslatableString(getKey(@"discard_unsaved_changes_dialog_header"), @"Discard all unsaved changes? This cannot be undone.");

        /// <summary>
        /// "The beatmap will be saved to continue with this operation."
        /// </summary>
        public static LocalisableString SaveRequiredDialogHeader => new TranslatableString(getKey(@"save_required_dialog_header"), @"The beatmap will be saved to continue with this operation.");

        /// <summary>
        /// "Sounds good, let&#39;s go!"
        /// </summary>
        public static LocalisableString SoundsGood => new TranslatableString(getKey(@"sounds_good"), @"Sounds good, let's go!");

        /// <summary>
        /// "Difficulty &quot;{0}&quot; with {1} objects"
        /// </summary>
        public static LocalisableString DeleteDifficultyDetails(string difficultyName, int objectCount) => new TranslatableString(getKey(@"delete_difficulty_details"), @"Difficulty ""{0}"" with {1} objects", difficultyName, objectCount);

        /// <summary>
        /// "All Bookmarks"
        /// </summary>
        public static LocalisableString AllBookmarks => new TranslatableString(getKey(@"all_bookmarks"), @"All Bookmarks");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
