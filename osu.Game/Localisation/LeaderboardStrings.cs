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
        public static LocalisableString CouldntFetchScores => new TranslatableString(getKey(@"couldnt_fetch_scores"), @"未能获取分数！");

        /// <summary>
        /// "Please select a beatmap!"
        /// </summary>
        public static LocalisableString PleaseSelectABeatmap => new TranslatableString(getKey(@"please_select_a_beatmap"), @"请选择一张地图！");

        /// <summary>
        /// "Leaderboards are not available for this ruleset!"
        /// </summary>
        public static LocalisableString LeaderboardsAreNotAvailableForThisRuleset => new TranslatableString(getKey(@"leaderboards_are_not_available_for_this_ruleset"), @"此游戏模式的排行榜不可用！");

        /// <summary>
        /// "Leaderboards are not available for this beatmap!"
        /// </summary>
        public static LocalisableString LeaderboardsAreNotAvailableForThisBeatmap => new TranslatableString(getKey(@"leaderboards_are_not_available_for_this_beatmap"), @"这张图的排行榜不可用！");

        /// <summary>
        /// "No records yet!"
        /// </summary>
        public static LocalisableString NoRecordsYet => new TranslatableString(getKey(@"no_records_yet"), @"暂无成绩！");

        /// <summary>
        /// "Please sign in to view online leaderboards!"
        /// </summary>
        public static LocalisableString PleaseSignInToViewOnlineLeaderboards => new TranslatableString(getKey(@"please_sign_in_to_view_online_leaderboards"), @"请登入来查看排行榜！");

        /// <summary>
        /// "Please invest in an osu!supporter tag to view this leaderboard!"
        /// </summary>
        public static LocalisableString PleaseInvestInAnOsuSupporterTagToViewThisLeaderboard => new TranslatableString(getKey(@"please_invest_in_an_osu_supporter_tag_to_view_this_leaderboard"), @"请先成为 osu!supporter 来查看此排行！");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
