// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class RankingsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Rankings";

        /// <summary>
        /// "All"
        /// </summary>
        public static LocalisableString CountriesAll => new TranslatableString(getKey(@"countries.all"), @"All");

        /// <summary>
        /// "Country"
        /// </summary>
        public static LocalisableString CountriesTitle => new TranslatableString(getKey(@"countries.title"), @"Country");

        /// <summary>
        /// "Show"
        /// </summary>
        public static LocalisableString FilterTitle => new TranslatableString(getKey(@"filter.title"), @"Show");

        /// <summary>
        /// "Variant"
        /// </summary>
        public static LocalisableString FilterVariantTitle => new TranslatableString(getKey(@"filter.variant.title"), @"Variant");

        /// <summary>
        /// "spotlights"
        /// </summary>
        public static LocalisableString TypeCharts => new TranslatableString(getKey(@"type.charts"), @"spotlights");

        /// <summary>
        /// "country"
        /// </summary>
        public static LocalisableString TypeCountry => new TranslatableString(getKey(@"type.country"), @"country");

        /// <summary>
        /// "multiplayer"
        /// </summary>
        public static LocalisableString TypeMultiplayer => new TranslatableString(getKey(@"type.multiplayer"), @"multiplayer");

        /// <summary>
        /// "performance"
        /// </summary>
        public static LocalisableString TypePerformance => new TranslatableString(getKey(@"type.performance"), @"performance");

        /// <summary>
        /// "score"
        /// </summary>
        public static LocalisableString TypeScore => new TranslatableString(getKey(@"type.score"), @"score");

        /// <summary>
        /// "End Date"
        /// </summary>
        public static LocalisableString SpotlightEndDate => new TranslatableString(getKey(@"spotlight.end_date"), @"End Date");

        /// <summary>
        /// "Map Count"
        /// </summary>
        public static LocalisableString SpotlightMapCount => new TranslatableString(getKey(@"spotlight.map_count"), @"Map Count");

        /// <summary>
        /// "Participants"
        /// </summary>
        public static LocalisableString SpotlightParticipants => new TranslatableString(getKey(@"spotlight.participants"), @"Participants");

        /// <summary>
        /// "Start Date"
        /// </summary>
        public static LocalisableString SpotlightStartDate => new TranslatableString(getKey(@"spotlight.start_date"), @"Start Date");

        /// <summary>
        /// "Accuracy"
        /// </summary>
        public static LocalisableString StatAccuracy => new TranslatableString(getKey(@"stat.accuracy"), @"Accuracy");

        /// <summary>
        /// "Active Users"
        /// </summary>
        public static LocalisableString StatActiveUsers => new TranslatableString(getKey(@"stat.active_users"), @"Active Users");

        /// <summary>
        /// "Country"
        /// </summary>
        public static LocalisableString StatCountry => new TranslatableString(getKey(@"stat.country"), @"Country");

        /// <summary>
        /// "Play Count"
        /// </summary>
        public static LocalisableString StatPlayCount => new TranslatableString(getKey(@"stat.play_count"), @"Play Count");

        /// <summary>
        /// "Performance"
        /// </summary>
        public static LocalisableString StatPerformance => new TranslatableString(getKey(@"stat.performance"), @"Performance");

        /// <summary>
        /// "Total Score"
        /// </summary>
        public static LocalisableString StatTotalScore => new TranslatableString(getKey(@"stat.total_score"), @"Total Score");

        /// <summary>
        /// "Ranked Score"
        /// </summary>
        public static LocalisableString StatRankedScore => new TranslatableString(getKey(@"stat.ranked_score"), @"Ranked Score");

        /// <summary>
        /// "Avg. Score"
        /// </summary>
        public static LocalisableString StatAverageScore => new TranslatableString(getKey(@"stat.average_score"), @"Avg. Score");

        /// <summary>
        /// "Avg. Perf."
        /// </summary>
        public static LocalisableString StatAveragePerformance => new TranslatableString(getKey(@"stat.average_performance"), @"Avg. Perf.");

        /// <summary>
        /// "SS"
        /// </summary>
        public static LocalisableString Statss => new TranslatableString(getKey(@"stat.ss"), @"SS");

        /// <summary>
        /// "S"
        /// </summary>
        public static LocalisableString Stats => new TranslatableString(getKey(@"stat.s"), @"S");

        /// <summary>
        /// "A"
        /// </summary>
        public static LocalisableString Stata => new TranslatableString(getKey(@"stat.a"), @"A");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}