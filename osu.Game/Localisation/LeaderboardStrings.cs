// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class LeaderboardStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Leaderboard";

        /// <summary>
        /// "Couldn't fetch scores!"
        /// </summary>
        public static LocalisableString CouldntFetchScores => new TranslatableString(getKey(@"couldnt_fetch_scores"), @"Couldn't fetch scores!");

        /// <summary>
        /// "Please select a beatmap!"
        /// </summary>
        public static LocalisableString PleaseSelectABeatmap => new TranslatableString(getKey(@"please_select_a_beatmap"), @"Please select a beatmap!");

        /// <summary>
        /// "Leaderboards are not available for this ruleset!"
        /// </summary>
        public static LocalisableString LeaderboardsAreNotAvailableForThisRuleset => new TranslatableString(getKey(@"leaderboards_are_not_available_for_this_ruleset"), @"Leaderboards are not available for this ruleset!");

        /// <summary>
        /// "Leaderboards are not available for this beatmap!"
        /// </summary>
        public static LocalisableString LeaderboardsAreNotAvailableForThisBeatmap => new TranslatableString(getKey(@"leaderboards_are_not_available_for_this_beatmap"), @"Leaderboards are not available for this beatmap!");

        /// <summary>
        /// "No records yet!"
        /// </summary>
        public static LocalisableString NoRecordsYet => new TranslatableString(getKey(@"no_records_yet"), @"No records yet!");

        /// <summary>
        /// "Please sign in to view online leaderboards!"
        /// </summary>
        public static LocalisableString PleaseSignInToViewOnlineLeaderboards => new TranslatableString(getKey(@"please_sign_in_to_view_online_leaderboards"), @"Please sign in to view online leaderboards!");

        /// <summary>
        /// "Please invest in an osu!supporter tag to view this leaderboard!"
        /// </summary>
        public static LocalisableString PleaseInvestInAnOsuSupporterTagToViewThisLeaderboard => new TranslatableString(getKey(@"please_invest_in_an_osu_supporter_tag_to_view_this_leaderboard"), @"Please invest in an osu!supporter tag to view this leaderboard!");

        /// <summary>
        /// "You are not on a team. Maybe you should join one!"
        /// </summary>
        public static LocalisableString NoTeam => new TranslatableString(getKey(@"no_team"), @"You are not on a team. Maybe you should join one!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
