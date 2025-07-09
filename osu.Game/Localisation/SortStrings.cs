// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class SortStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Sort";

        /// <summary>
        /// "Scope"
        /// </summary>
        public static LocalisableString Scope => new TranslatableString(getKey(@"scope"), @"Scope");

        /// <summary>
        /// "Local"
        /// </summary>
        public static LocalisableString Local => new TranslatableString(getKey(@"local"), @"Local");

        /// <summary>
        /// "Global"
        /// </summary>
        public static LocalisableString Global => new TranslatableString(getKey(@"global"), @"Global");

        /// <summary>
        /// "Country"
        /// </summary>
        public static LocalisableString Country => new TranslatableString(getKey(@"country"), @"Country");

        /// <summary>
        /// "Friend"
        /// </summary>
        public static LocalisableString Friend => new TranslatableString(getKey(@"friend"), @"Friend");

        /// <summary>
        /// "Team"
        /// </summary>
        public static LocalisableString Team => new TranslatableString(getKey(@"team"), @"Team");

        /// <summary>
        /// "Group by"
        /// </summary>
        public static LocalisableString GroupBy => new TranslatableString(getKey(@"group_by"), @"Group by");

        /// <summary>
        /// "None"
        /// </summary>
        public static LocalisableString None => new TranslatableString(getKey(@"none"), @"None");

        /// <summary>
        /// "Author"
        /// </summary>
        public static LocalisableString Author => new TranslatableString(getKey(@"author"), @"Author");

        /// <summary>
        /// "Date Submitted"
        /// </summary>
        public static LocalisableString DateSubmitted => new TranslatableString(getKey(@"date_submitted"), @"Date Submitted");

        /// <summary>
        /// "Date Added"
        /// </summary>
        public static LocalisableString DateAdded => new TranslatableString(getKey(@"date_added"), @"Date Added");

        /// <summary>
        /// "Date Ranked"
        /// </summary>
        public static LocalisableString DateRanked => new TranslatableString(getKey(@"date_ranked"), @"Date Ranked");

        /// <summary>
        /// "Last Played"
        /// </summary>
        public static LocalisableString LastPlayed => new TranslatableString(getKey(@"last_played"), @"Last Played");

        /// <summary>
        /// "My Maps"
        /// </summary>
        public static LocalisableString MyMaps => new TranslatableString(getKey(@"my_maps"), @"My Maps");

        /// <summary>
        /// "Collections"
        /// </summary>
        public static LocalisableString Collections => new TranslatableString(getKey(@"collections"), @"Collections");

        /// <summary>
        /// "Rank Achieved"
        /// </summary>
        public static LocalisableString RankAchieved => new TranslatableString(getKey(@"rank_achieved"), @"Rank Achieved");

        /// <summary>
        /// "Ranked Status"
        /// </summary>
        public static LocalisableString RankedStatus => new TranslatableString(getKey(@"ranked_status"), @"Ranked Status");

        /// <summary>
        /// "Source"
        /// </summary>
        public static LocalisableString Source => new TranslatableString(getKey(@"source"), @"Source");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
