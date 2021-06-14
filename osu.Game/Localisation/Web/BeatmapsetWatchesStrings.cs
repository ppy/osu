// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class BeatmapsetWatchesStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.BeatmapsetWatches";

        /// <summary>
        /// "These are the beatmap discussions you are following. You will be notified when there are new posts or updates."
        /// </summary>
        public static LocalisableString IndexDescription => new TranslatableString(getKey(@"index.description"), @"These are the beatmap discussions you are following. You will be notified when there are new posts or updates.");

        /// <summary>
        /// "beatmap discussion watchlist"
        /// </summary>
        public static LocalisableString IndexTitleCompact => new TranslatableString(getKey(@"index.title_compact"), @"beatmap discussion watchlist");

        /// <summary>
        /// "Beatmaps watched"
        /// </summary>
        public static LocalisableString IndexCountsTotal => new TranslatableString(getKey(@"index.counts.total"), @"Beatmaps watched");

        /// <summary>
        /// "Beatmaps with new activity"
        /// </summary>
        public static LocalisableString IndexCountsUnread => new TranslatableString(getKey(@"index.counts.unread"), @"Beatmaps with new activity");

        /// <summary>
        /// "No beatmap discussions watched."
        /// </summary>
        public static LocalisableString IndexTableEmpty => new TranslatableString(getKey(@"index.table.empty"), @"No beatmap discussions watched.");

        /// <summary>
        /// "Last update"
        /// </summary>
        public static LocalisableString IndexTableLastUpdate => new TranslatableString(getKey(@"index.table.last_update"), @"Last update");

        /// <summary>
        /// "Open issues"
        /// </summary>
        public static LocalisableString IndexTableOpenIssues => new TranslatableString(getKey(@"index.table.open_issues"), @"Open issues");

        /// <summary>
        /// "State"
        /// </summary>
        public static LocalisableString IndexTableState => new TranslatableString(getKey(@"index.table.state"), @"State");

        /// <summary>
        /// "Title"
        /// </summary>
        public static LocalisableString IndexTableTitle => new TranslatableString(getKey(@"index.table.title"), @"Title");

        /// <summary>
        /// "Read"
        /// </summary>
        public static LocalisableString StatusRead => new TranslatableString(getKey(@"status.read"), @"Read");

        /// <summary>
        /// "Unread"
        /// </summary>
        public static LocalisableString StatusUnread => new TranslatableString(getKey(@"status.unread"), @"Unread");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}