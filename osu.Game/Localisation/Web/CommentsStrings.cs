// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class CommentsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Comments";

        /// <summary>
        /// "deleted"
        /// </summary>
        public static LocalisableString Deleted => new TranslatableString(getKey(@"deleted"), @"deleted");

        /// <summary>
        /// "{0} deleted comment|{0} deleted comments"
        /// </summary>
        public static LocalisableString DeletedCount(string countDelimited) => new TranslatableString(getKey(@"deleted_count"), @"{0} deleted comment|{0} deleted comments", countDelimited);

        /// <summary>
        /// "edited {0} by {1}"
        /// </summary>
        public static LocalisableString Edited(string timeago, string user) => new TranslatableString(getKey(@"edited"), @"edited {0} by {1}", timeago, user);

        /// <summary>
        /// "pinned"
        /// </summary>
        public static LocalisableString Pinned => new TranslatableString(getKey(@"pinned"), @"pinned");

        /// <summary>
        /// "No comments yet."
        /// </summary>
        public static LocalisableString Empty => new TranslatableString(getKey(@"empty"), @"No comments yet.");

        /// <summary>
        /// "load replies"
        /// </summary>
        public static LocalisableString LoadReplies => new TranslatableString(getKey(@"load_replies"), @"load replies");

        /// <summary>
        /// "{0} reply|{0} replies"
        /// </summary>
        public static LocalisableString RepliesCount(string countDelimited) => new TranslatableString(getKey(@"replies_count"), @"{0} reply|{0} replies", countDelimited);

        /// <summary>
        /// "Comments"
        /// </summary>
        public static LocalisableString Title => new TranslatableString(getKey(@"title"), @"Comments");

        /// <summary>
        /// "Beatmap"
        /// </summary>
        public static LocalisableString CommentableNameBeatmapset => new TranslatableString(getKey(@"commentable_name.beatmapset"), @"Beatmap");

        /// <summary>
        /// "Changelog"
        /// </summary>
        public static LocalisableString CommentableNameBuild => new TranslatableString(getKey(@"commentable_name.build"), @"Changelog");

        /// <summary>
        /// "News"
        /// </summary>
        public static LocalisableString CommentableNameNewsPost => new TranslatableString(getKey(@"commentable_name.news_post"), @"News");

        /// <summary>
        /// "Deleted Item"
        /// </summary>
        public static LocalisableString CommentableNameDeleted => new TranslatableString(getKey(@"commentable_name._deleted"), @"Deleted Item");

        /// <summary>
        /// "Press enter to {0}. Use shift+enter for new line."
        /// </summary>
        public static LocalisableString EditorTextareaHintDefault(string action) => new TranslatableString(getKey(@"editor.textarea_hint._"), @"Press enter to {0}. Use shift+enter for new line.", action);

        /// <summary>
        /// "save"
        /// </summary>
        public static LocalisableString EditorTextareaHintEdit => new TranslatableString(getKey(@"editor.textarea_hint.edit"), @"save");

        /// <summary>
        /// "post"
        /// </summary>
        public static LocalisableString EditorTextareaHintNew => new TranslatableString(getKey(@"editor.textarea_hint.new"), @"post");

        /// <summary>
        /// "reply"
        /// </summary>
        public static LocalisableString EditorTextareaHintReply => new TranslatableString(getKey(@"editor.textarea_hint.reply"), @"reply");

        /// <summary>
        /// "Sign in to comment"
        /// </summary>
        public static LocalisableString GuestButtonNew => new TranslatableString(getKey(@"guest_button.new"), @"Sign in to comment");

        /// <summary>
        /// "Sign in to reply"
        /// </summary>
        public static LocalisableString GuestButtonReply => new TranslatableString(getKey(@"guest_button.reply"), @"Sign in to reply");

        /// <summary>
        /// "comments"
        /// </summary>
        public static LocalisableString IndexNavComments => new TranslatableString(getKey(@"index.nav_comments"), @"comments");

        /// <summary>
        /// "listing"
        /// </summary>
        public static LocalisableString IndexNavTitle => new TranslatableString(getKey(@"index.nav_title"), @"listing");

        /// <summary>
        /// "no comments found..."
        /// </summary>
        public static LocalisableString IndexNoComments => new TranslatableString(getKey(@"index.no_comments"), @"no comments found...");

        /// <summary>
        /// "Edit the comment here"
        /// </summary>
        public static LocalisableString PlaceholderEdit => new TranslatableString(getKey(@"placeholder.edit"), @"Edit the comment here");

        /// <summary>
        /// "Type new comment here"
        /// </summary>
        public static LocalisableString PlaceholderNew => new TranslatableString(getKey(@"placeholder.new"), @"Type new comment here");

        /// <summary>
        /// "Type your response here"
        /// </summary>
        public static LocalisableString PlaceholderReply => new TranslatableString(getKey(@"placeholder.reply"), @"Type your response here");

        /// <summary>
        /// "comments"
        /// </summary>
        public static LocalisableString ShowNavTitle => new TranslatableString(getKey(@"show.nav_title"), @"comments");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}