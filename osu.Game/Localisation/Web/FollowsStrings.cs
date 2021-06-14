// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class FollowsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Follows";

        /// <summary>
        /// "No comments watched."
        /// </summary>
        public static LocalisableString CommentEmpty => new TranslatableString(getKey(@"comment.empty"), @"No comments watched.");

        /// <summary>
        /// "comment watchlist"
        /// </summary>
        public static LocalisableString CommentPageTitle => new TranslatableString(getKey(@"comment.page_title"), @"comment watchlist");

        /// <summary>
        /// "comment"
        /// </summary>
        public static LocalisableString CommentTitle => new TranslatableString(getKey(@"comment.title"), @"comment");

        /// <summary>
        /// "no comments"
        /// </summary>
        public static LocalisableString CommentTableLatestCommentEmpty => new TranslatableString(getKey(@"comment.table.latest_comment_empty"), @"no comments");

        /// <summary>
        /// "{0} by {1}"
        /// </summary>
        public static LocalisableString CommentTableLatestCommentValue(string time, string username) => new TranslatableString(getKey(@"comment.table.latest_comment_value"), @"{0} by {1}", time, username);

        /// <summary>
        /// "forum topic"
        /// </summary>
        public static LocalisableString ForumTopicTitle => new TranslatableString(getKey(@"forum_topic.title"), @"forum topic");

        /// <summary>
        /// "watchlists"
        /// </summary>
        public static LocalisableString IndexTitleCompact => new TranslatableString(getKey(@"index.title_compact"), @"watchlists");

        /// <summary>
        /// "No mappers watched."
        /// </summary>
        public static LocalisableString MappingEmpty => new TranslatableString(getKey(@"mapping.empty"), @"No mappers watched.");

        /// <summary>
        /// "mapping subscribers"
        /// </summary>
        public static LocalisableString MappingFollowers => new TranslatableString(getKey(@"mapping.followers"), @"mapping subscribers");

        /// <summary>
        /// "mapper watchlist"
        /// </summary>
        public static LocalisableString MappingPageTitle => new TranslatableString(getKey(@"mapping.page_title"), @"mapper watchlist");

        /// <summary>
        /// "mapper"
        /// </summary>
        public static LocalisableString MappingTitle => new TranslatableString(getKey(@"mapping.title"), @"mapper");

        /// <summary>
        /// "stop notifying me when this user uploads new beatmap"
        /// </summary>
        public static LocalisableString MappingTo0 => new TranslatableString(getKey(@"mapping.to_0"), @"stop notifying me when this user uploads new beatmap");

        /// <summary>
        /// "notify me when this user uploads new beatmap"
        /// </summary>
        public static LocalisableString MappingTo1 => new TranslatableString(getKey(@"mapping.to_1"), @"notify me when this user uploads new beatmap");

        /// <summary>
        /// "beatmap discussion"
        /// </summary>
        public static LocalisableString ModdingTitle => new TranslatableString(getKey(@"modding.title"), @"beatmap discussion");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}