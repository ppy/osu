// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ResultsScreenStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ResultsScreen";

        /// <summary>
        /// "Achieved on {0:g}"
        /// </summary>
        public static LocalisableString AchievedOnDate(DateTimeOffset date) => new TranslatableString(getKey(@"achieved_on_date"), @"Achieved on {0:g}", date);

        /// <summary>
        /// "Performance points are not granted for this score because the beatmap is not ranked."
        /// </summary>
        public static LocalisableString NoPPForUnrankedBeatmaps => new TranslatableString(getKey(@"no_pp_for_unranked_beatmaps"), @"Performance points are not granted for this score because the beatmap is not ranked.");

        /// <summary>
        /// "Performance points are not granted for this score because of unranked mods."
        /// </summary>
        public static LocalisableString NoPPForUnrankedMods => new TranslatableString(getKey(@"no_pp_for_unranked_mods"), @"Performance points are not granted for this score because of unranked mods.");

        /// <summary>
        /// "Performance points are not granted for failed scores."
        /// </summary>
        public static LocalisableString NoPPForFailedScores => new TranslatableString(getKey(@"no_pp_for_failed_scores"), @"Performance points are not granted for failed scores.");

        /// <summary>
        /// "Performance"
        /// </summary>
        public static LocalisableString PerformancePointsHeader => new TranslatableString(getKey(@"performance_points_header"), "Performance");

        /// <summary>
        /// "Personal best"
        /// </summary>
        public static LocalisableString PersonalBest => new TranslatableString(getKey(@"personal_best"), @"Personal best");

        /// <summary>
        /// "Accuracy:
        /// Error: {0:0.00}ms - {1:0.00}ms avg
        /// Unstable Rate: {2:0.00}"
        /// </summary>
        public static LocalisableString LegacyHealthGraphTooltipText(double earlyAverage, double lateAverage, double unstableRate) => new TranslatableString(getKey(@"legacy_health_graph_tooltip_text"), @"Accuracy:
Error: {0:0.00}ms - {1:0.00}ms avg
Unstable Rate: {2:0.00}", earlyAverage, lateAverage, unstableRate);

        /// <summary>
        /// "▼ Online Ranking ▼"
        /// </summary>
        public static LocalisableString LegacyOnlineRankingButtonText => new TranslatableString(getKey(@"legacy_online_ranking_button_text"), @"▼ Online Ranking ▼");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
