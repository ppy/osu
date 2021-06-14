// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class ReportStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Report";

        /// <summary>
        /// "Report"
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionPostButton => new TranslatableString(getKey(@"beatmapset_discussion_post.button"), @"Report");

        /// <summary>
        /// "Report {0}&#39;s post?"
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionPostTitle(string username) => new TranslatableString(getKey(@"beatmapset_discussion_post.title"), @"Report {0}'s post?", username);

        /// <summary>
        /// "Report"
        /// </summary>
        public static LocalisableString CommentButton => new TranslatableString(getKey(@"comment.button"), @"Report");

        /// <summary>
        /// "Report {0}&#39;s comment?"
        /// </summary>
        public static LocalisableString CommentTitle(string username) => new TranslatableString(getKey(@"comment.title"), @"Report {0}'s comment?", username);

        /// <summary>
        /// "Report"
        /// </summary>
        public static LocalisableString ForumPostButton => new TranslatableString(getKey(@"forum_post.button"), @"Report");

        /// <summary>
        /// "Report {0}&#39;s post?"
        /// </summary>
        public static LocalisableString ForumPostTitle(string username) => new TranslatableString(getKey(@"forum_post.title"), @"Report {0}'s post?", username);

        /// <summary>
        /// "Report Score"
        /// </summary>
        public static LocalisableString ScoresButton => new TranslatableString(getKey(@"scores.button"), @"Report Score");

        /// <summary>
        /// "Report {0}&#39;s score?"
        /// </summary>
        public static LocalisableString ScoresTitle(string username) => new TranslatableString(getKey(@"scores.title"), @"Report {0}'s score?", username);

        /// <summary>
        /// "Report"
        /// </summary>
        public static LocalisableString UserButton => new TranslatableString(getKey(@"user.button"), @"Report");

        /// <summary>
        /// "Report {0}?"
        /// </summary>
        public static LocalisableString UserTitle(string username) => new TranslatableString(getKey(@"user.title"), @"Report {0}?", username);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}