// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class LeaderboardStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Leaderboard";

        /// <summary>
        /// "未能获取分数！"
        /// </summary>
        public static LocalisableString CouldntFetchScores => new TranslatableString(getKey(@"couldnt_fetch_scores"), @"未能获取分数！");

        /// <summary>
        /// "请选择一张谱面！"
        /// </summary>
        public static LocalisableString PleaseSelectABeatmap => new TranslatableString(getKey(@"please_select_a_beatmap"), @"请选择一张谱面！");

        /// <summary>
        /// "此游戏模式的排行榜不可用！"
        /// </summary>
        public static LocalisableString LeaderboardsAreNotAvailableForThisRuleset => new TranslatableString(getKey(@"leaderboards_are_not_available_for_this_ruleset"), @"此游戏模式的排行榜不可用！");

        /// <summary>
        /// "这张图的排行榜不可用！"
        /// </summary>
        public static LocalisableString LeaderboardsAreNotAvailableForThisBeatmap => new TranslatableString(getKey(@"leaderboards_are_not_available_for_this_beatmap"), @"这张图的排行榜不可用！");

        /// <summary>
        /// "暂无成绩！"
        /// </summary>
        public static LocalisableString NoRecordsYet => new TranslatableString(getKey(@"no_records_yet"), @"暂无成绩！");

        /// <summary>
        /// "请登入来查看排行榜！"
        /// </summary>
        public static LocalisableString PleaseSignInToViewOnlineLeaderboards => new TranslatableString(getKey(@"please_sign_in_to_view_online_leaderboards"), @"请登入来查看排行榜！");

        /// <summary>
        /// "请先成为 osu!supporter 来查看此排行！"
        /// </summary>
        public static LocalisableString PleaseInvestInAnOsuSupporterTagToViewThisLeaderboard => new TranslatableString(getKey(@"please_invest_in_an_osu_supporter_tag_to_view_this_leaderboard"), @"请先成为 osu!supporter 来查看此排行！");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
