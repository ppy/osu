// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class BeatmapCarouselFilterGroupingStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.BeatmapCarouselFilterGrouping";

        /// <summary>
        /// "Never"
        /// </summary>
        public static LocalisableString NeverPlayed => new TranslatableString(getKey(@"never_played"), @"Never");

        /// <summary>
        /// "Other"
        /// </summary>
        public static LocalisableString OtherSymbols => new TranslatableString(getKey(@"other_symbols"), @"Other");

        /// <summary>
        /// "Today"
        /// </summary>
        public static LocalisableString Today => new TranslatableString(getKey(@"today"), @"Today");

        /// <summary>
        /// "Yesterday"
        /// </summary>
        public static LocalisableString Yesterday => new TranslatableString(getKey(@"yesterday"), @"Yesterday");

        /// <summary>
        /// "Last week"
        /// </summary>
        public static LocalisableString LastWeek => new TranslatableString(getKey(@"last_week"), @"Last week");

        /// <summary>
        /// "Last month"
        /// </summary>
        public static LocalisableString LastMonth => new TranslatableString(getKey(@"last_month"), @"Last month");

        /// <summary>
        /// "{0} month ago|{0} months ago"
        /// </summary>
        public static LocalisableString MonthsAgo(int quantity) => new PluralisableString(new TranslatableString(getKey(@"months_ago"), @"{0} month ago|{0} months ago", quantity), quantity, '|');

        /// <summary>
        /// "Over {0} month ago|Over {0} months ago"
        /// </summary>
        public static LocalisableString OverMonthsAgo(int quantity) => new PluralisableString(new TranslatableString(getKey(@"over_months_ago"), @"Over {0} month ago|Over {0} months ago", quantity), quantity, '|');

        /// <summary>
        /// "Unranked"
        /// </summary>
        public static LocalisableString Unranked => new TranslatableString(getKey(@"unranked"), @"Unranked");

        /// <summary>
        /// "Under {0} BPM"
        /// </summary>
        public static LocalisableString UnderBPM(int bpm) => new TranslatableString(getKey(@"under_bpm"), @"Under {0} BPM", bpm);

        /// <summary>
        /// "{0} - {1} BPM"
        /// </summary>
        public static LocalisableString RangeBPM(int minBPM, int maxBPM) => new TranslatableString(getKey(@"range_bpm"), @"{0} - {1} BPM", minBPM, maxBPM);

        /// <summary>
        /// "Over {0} BPM"
        /// </summary>
        public static LocalisableString OverBPM(int bpm) => new TranslatableString(getKey(@"over_bpm"), @"Over {0} BPM", bpm);

        /// <summary>
        /// "Below {0} star|Below {0} stars"
        /// </summary>
        public static LocalisableString BelowStars(int quantity) => new PluralisableString(new TranslatableString(getKey(@"below_stars"), @"Below {0} star|Below {0} stars", quantity), quantity, '|');

        /// <summary>
        /// "{0} star|{0} stars"
        /// </summary>
        public static LocalisableString Stars(int quantity) => new PluralisableString(new TranslatableString(getKey(@"stars"), @"{0} star|{0} stars", quantity), quantity, '|');

        /// <summary>
        /// "Over {0} star|Over {0} stars"
        /// </summary>
        public static LocalisableString OverStars(int quantity) => new PluralisableString(new TranslatableString(getKey(@"over_stars"), @"Over {0} star|Over {0} stars", quantity), quantity, '|');

        /// <summary>
        /// "{0} minute or less|{0} minutes or less"
        /// </summary>
        public static LocalisableString MinutesOrLess(int quantity) => new PluralisableString(new TranslatableString(getKey(@"minutes_or_less"), @"{0} minute or less|{0} minutes or less", quantity), quantity, '|');

        /// <summary>
        /// "Over {0} minute|Over {0} minutes"
        /// </summary>
        public static LocalisableString OverMinutes(int quantity) =>
            new PluralisableString(new TranslatableString(getKey(@"over_minutes"), @"Over {0} minute|Over {0} minutes", quantity), quantity, '|');

        /// <summary>
        /// "Unsourced"
        /// </summary>
        public static LocalisableString Unsourced => new TranslatableString(getKey(@"unsourced"), @"Unsourced");

        /// <summary>
        /// "Not in collection"
        /// </summary>
        public static LocalisableString NotInCollection => new TranslatableString(getKey(@"not_in_collection"), @"Not in collection");

        /// <summary>
        /// "My maps"
        /// </summary>
        public static LocalisableString MyMaps => new TranslatableString(getKey(@"my_maps"), @"My maps");

        /// <summary>
        /// "Unplayed"
        /// </summary>
        public static LocalisableString Unplayed => new TranslatableString(getKey(@"unplayed"), @"Unplayed");

        /// <summary>
        /// "Favourites"
        /// </summary>
        public static LocalisableString Favourites => new TranslatableString(getKey(@"favourites"), @"Favourites");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
