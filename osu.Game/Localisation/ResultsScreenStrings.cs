// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class ResultsScreenStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.ResultsScreen";

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
        /// "importing score..."
        /// </summary>
        public static LocalisableString ImportingScore => new TranslatableString(getKey(@"importing_score"), @"importing score...");

        /// <summary>
        /// "save score"
        /// </summary>
        public static LocalisableString SaveScore => new TranslatableString(getKey(@"save_score"), @"save score");

        /// <summary>
        /// "replay unavailable"
        /// </summary>
        public static LocalisableString ReplayUnavailable => new TranslatableString(getKey(@"replay_unavailable"), @"replay unavailable");

        /// <summary>
        /// "Achieved PP"
        /// </summary>
        public static LocalisableString AchievedPP => new TranslatableString(getKey(@"achieved_pp"), @"Achieved PP");

        /// <summary>
        /// "Maximum PP"
        /// </summary>
        public static LocalisableString MaximumPP => new TranslatableString(getKey(@"maximum_pp"), @"Maximum PP");

        /// <summary>
        /// "Extended statistics are only available after watching a replay!"
        /// </summary>
        public static LocalisableString ExtendedStatisticsAvailableAfterWatchingReplay => new TranslatableString(getKey(@"extended_statistics_available_after_watching_replay"), @"Extended statistics are only available after watching a replay!");

        /// <summary>
        /// "More statistics available after watching a replay!"
        /// </summary>
        public static LocalisableString MoreStatisticsAvailableAfterWatchingReplay => new TranslatableString(getKey(@"more_statistics_available_after_watching_replay"), @"More statistics available after watching a replay!");

        /// <summary>
        /// "Overall Ranking"
        /// </summary>
        public static LocalisableString OverallRankingHeader => new TranslatableString(getKey(@"overall_ranking_header"), @"Overall Ranking");

        /// <summary>
        /// "Tag the beatmap!"
        /// </summary>
        public static LocalisableString TagTheBeatmapHeader => new TranslatableString(getKey(@"tag_the_beatmap_header"), @"Tag the beatmap!");

        /// <summary>
        /// "Play the beatmap to contribute to beatmap tags!"
        /// </summary>
        public static LocalisableString PreventTaggingForUnplayedBeatmaps => new TranslatableString(getKey(@"prevent_tagging_for_unplayed_beatmaps"), @"Play the beatmap to contribute to beatmap tags!");

        /// <summary>
        /// "Play the beatmap in its original ruleset to contribute to beatmap tags!"
        /// </summary>
        public static LocalisableString PreventTaggingForConvertedBeatmaps => new TranslatableString(getKey(@"prevent_tagging_for_converted_beatmaps"), @"Play the beatmap in its original ruleset to contribute to beatmap tags!");

        /// <summary>
        /// "Set a better score to contribute to beatmap tags!"
        /// </summary>
        public static LocalisableString PreventTaggingForInappropriateScores => new TranslatableString(getKey(@"prevent_tagging_for_inappropriate_scores"), @"Set a better score to contribute to beatmap tags!");

        /// <summary>
        /// "Play this beatmap without conversion mods to contribute to beatmap tags!"
        /// </summary>
        public static LocalisableString PreventTaggingForConversionMods => new TranslatableString(getKey(@"prevent_tagging_for_conversion_mods"), @"Play this beatmap without conversion mods to contribute to beatmap tags!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
