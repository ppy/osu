// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class SortStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Sort";

        /// <summary>
        /// "Sort by"
        /// </summary>
        public static LocalisableString Default => new TranslatableString(getKey(@"_"), @"Sort by");

        /// <summary>
        /// "All"
        /// </summary>
        public static LocalisableString All => new TranslatableString(getKey(@"all"), @"All");

        /// <summary>
        /// "Friends"
        /// </summary>
        public static LocalisableString Friends => new TranslatableString(getKey(@"friends"), @"Friends");

        /// <summary>
        /// "Recently active"
        /// </summary>
        public static LocalisableString LastVisit => new TranslatableString(getKey(@"last_visit"), @"Recently active");

        /// <summary>
        /// "Recent"
        /// </summary>
        public static LocalisableString New => new TranslatableString(getKey(@"new"), @"Recent");

        /// <summary>
        /// "Old"
        /// </summary>
        public static LocalisableString Old => new TranslatableString(getKey(@"old"), @"Old");

        /// <summary>
        /// "Rank"
        /// </summary>
        public static LocalisableString Rank => new TranslatableString(getKey(@"rank"), @"Rank");

        /// <summary>
        /// "Top"
        /// </summary>
        public static LocalisableString Top => new TranslatableString(getKey(@"top"), @"Top");

        /// <summary>
        /// "Username"
        /// </summary>
        public static LocalisableString Username => new TranslatableString(getKey(@"username"), @"Username");

        /// <summary>
        /// "Post time"
        /// </summary>
        public static LocalisableString ForumPostsCreated => new TranslatableString(getKey(@"forum_posts.created"), @"Post time");

        /// <summary>
        /// "Relevance"
        /// </summary>
        public static LocalisableString ForumPostsRelevance => new TranslatableString(getKey(@"forum_posts.relevance"), @"Relevance");

        /// <summary>
        /// "Star priority"
        /// </summary>
        public static LocalisableString ForumTopicsFeatureVotes => new TranslatableString(getKey(@"forum_topics.feature_votes"), @"Star priority");

        /// <summary>
        /// "Last reply"
        /// </summary>
        public static LocalisableString ForumTopicsNew => new TranslatableString(getKey(@"forum_topics.new"), @"Last reply");

        /// <summary>
        /// "Relevance"
        /// </summary>
        public static LocalisableString UsersRelevance => new TranslatableString(getKey(@"users.relevance"), @"Relevance");

        /// <summary>
        /// "Name"
        /// </summary>
        public static LocalisableString UsersUsername => new TranslatableString(getKey(@"users.username"), @"Name");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}