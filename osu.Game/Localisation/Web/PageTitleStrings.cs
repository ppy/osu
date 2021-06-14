// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class PageTitleStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.PageTitle";

        /// <summary>
        /// "admin"
        /// </summary>
        public static LocalisableString AdminDefault => new TranslatableString(getKey(@"admin._"), @"admin");

        /// <summary>
        /// "admin"
        /// </summary>
        public static LocalisableString AdminForumDefault => new TranslatableString(getKey(@"admin_forum._"), @"admin");

        /// <summary>
        /// "admin"
        /// </summary>
        public static LocalisableString AdminStoreDefault => new TranslatableString(getKey(@"admin_store._"), @"admin");

        /// <summary>
        /// "invalid request"
        /// </summary>
        public static LocalisableString ErrorError400 => new TranslatableString(getKey(@"error.error.400"), @"invalid request");

        /// <summary>
        /// "missing"
        /// </summary>
        public static LocalisableString ErrorError404 => new TranslatableString(getKey(@"error.error.404"), @"missing");

        /// <summary>
        /// "forbidden"
        /// </summary>
        public static LocalisableString ErrorError403 => new TranslatableString(getKey(@"error.error.403"), @"forbidden");

        /// <summary>
        /// "unauthorized"
        /// </summary>
        public static LocalisableString ErrorError401 => new TranslatableString(getKey(@"error.error.401"), @"unauthorized");

        /// <summary>
        /// "account verification"
        /// </summary>
        public static LocalisableString ErrorError401Verification => new TranslatableString(getKey(@"error.error.401-verification"), @"account verification");

        /// <summary>
        /// "missing"
        /// </summary>
        public static LocalisableString ErrorError405 => new TranslatableString(getKey(@"error.error.405"), @"missing");

        /// <summary>
        /// "invalid request"
        /// </summary>
        public static LocalisableString ErrorError422 => new TranslatableString(getKey(@"error.error.422"), @"invalid request");

        /// <summary>
        /// "too many requests"
        /// </summary>
        public static LocalisableString ErrorError429 => new TranslatableString(getKey(@"error.error.429"), @"too many requests");

        /// <summary>
        /// "something broke"
        /// </summary>
        public static LocalisableString ErrorError500 => new TranslatableString(getKey(@"error.error.500"), @"something broke");

        /// <summary>
        /// "maintenance"
        /// </summary>
        public static LocalisableString ErrorError503 => new TranslatableString(getKey(@"error.error.503"), @"maintenance");

        /// <summary>
        /// "forum"
        /// </summary>
        public static LocalisableString ForumDefault => new TranslatableString(getKey(@"forum._"), @"forum");

        /// <summary>
        /// "dashboard"
        /// </summary>
        public static LocalisableString ForumTopicWatchesControllerIndex => new TranslatableString(getKey(@"forum.topic_watches_controller.index"), @"dashboard");

        /// <summary>
        /// "dashboard"
        /// </summary>
        public static LocalisableString MainAccountControllerEdit => new TranslatableString(getKey(@"main.account_controller.edit"), @"dashboard");

        /// <summary>
        /// "account verification"
        /// </summary>
        public static LocalisableString MainAccountControllerVerifyLink => new TranslatableString(getKey(@"main.account_controller.verify_link"), @"account verification");

        /// <summary>
        /// "featured artists"
        /// </summary>
        public static LocalisableString MainArtistsControllerDefault => new TranslatableString(getKey(@"main.artists_controller._"), @"featured artists");

        /// <summary>
        /// "beatmap discussion posts"
        /// </summary>
        public static LocalisableString MainBeatmapDiscussionPostsControllerDefault => new TranslatableString(getKey(@"main.beatmap_discussion_posts_controller._"), @"beatmap discussion posts");

        /// <summary>
        /// "beatmap discussions"
        /// </summary>
        public static LocalisableString MainBeatmapDiscussionsControllerDefault => new TranslatableString(getKey(@"main.beatmap_discussions_controller._"), @"beatmap discussions");

        /// <summary>
        /// "beatmap packs"
        /// </summary>
        public static LocalisableString MainBeatmapPacksControllerDefault => new TranslatableString(getKey(@"main.beatmap_packs_controller._"), @"beatmap packs");

        /// <summary>
        /// "beatmap discussion votes"
        /// </summary>
        public static LocalisableString MainBeatmapsetDiscussionVotesControllerDefault => new TranslatableString(getKey(@"main.beatmapset_discussion_votes_controller._"), @"beatmap discussion votes");

        /// <summary>
        /// "beatmap history"
        /// </summary>
        public static LocalisableString MainBeatmapsetEventsControllerDefault => new TranslatableString(getKey(@"main.beatmapset_events_controller._"), @"beatmap history");

        /// <summary>
        /// "dashboard"
        /// </summary>
        public static LocalisableString MainBeatmapsetWatchesControllerIndex => new TranslatableString(getKey(@"main.beatmapset_watches_controller.index"), @"dashboard");

        /// <summary>
        /// "beatmap discussion"
        /// </summary>
        public static LocalisableString MainBeatmapsetsControllerDiscussion => new TranslatableString(getKey(@"main.beatmapsets_controller.discussion"), @"beatmap discussion");

        /// <summary>
        /// "beatmap listing"
        /// </summary>
        public static LocalisableString MainBeatmapsetsControllerIndex => new TranslatableString(getKey(@"main.beatmapsets_controller.index"), @"beatmap listing");

        /// <summary>
        /// "beatmap info"
        /// </summary>
        public static LocalisableString MainBeatmapsetsControllerShow => new TranslatableString(getKey(@"main.beatmapsets_controller.show"), @"beatmap info");

        /// <summary>
        /// "changelog"
        /// </summary>
        public static LocalisableString MainChangelogControllerDefault => new TranslatableString(getKey(@"main.changelog_controller._"), @"changelog");

        /// <summary>
        /// "chat"
        /// </summary>
        public static LocalisableString MainChatControllerDefault => new TranslatableString(getKey(@"main.chat_controller._"), @"chat");

        /// <summary>
        /// "comments"
        /// </summary>
        public static LocalisableString MainCommentsControllerDefault => new TranslatableString(getKey(@"main.comments_controller._"), @"comments");

        /// <summary>
        /// "contests"
        /// </summary>
        public static LocalisableString MainContestsControllerDefault => new TranslatableString(getKey(@"main.contests_controller._"), @"contests");

        /// <summary>
        /// "dashboard"
        /// </summary>
        public static LocalisableString MainFollowsControllerIndex => new TranslatableString(getKey(@"main.follows_controller.index"), @"dashboard");

        /// <summary>
        /// "dashboard"
        /// </summary>
        public static LocalisableString MainFriendsControllerIndex => new TranslatableString(getKey(@"main.friends_controller.index"), @"dashboard");

        /// <summary>
        /// "groups"
        /// </summary>
        public static LocalisableString MainGroupsControllerShow => new TranslatableString(getKey(@"main.groups_controller.show"), @"groups");

        /// <summary>
        /// "download"
        /// </summary>
        public static LocalisableString MainHomeControllerGetDownload => new TranslatableString(getKey(@"main.home_controller.get_download"), @"download");

        /// <summary>
        /// "dashboard"
        /// </summary>
        public static LocalisableString MainHomeControllerIndex => new TranslatableString(getKey(@"main.home_controller.index"), @"dashboard");

        /// <summary>
        /// "search"
        /// </summary>
        public static LocalisableString MainHomeControllerSearch => new TranslatableString(getKey(@"main.home_controller.search"), @"search");

        /// <summary>
        /// "support the game"
        /// </summary>
        public static LocalisableString MainHomeControllerSupportTheGame => new TranslatableString(getKey(@"main.home_controller.support_the_game"), @"support the game");

        /// <summary>
        /// "testflight"
        /// </summary>
        public static LocalisableString MainHomeControllerTestflight => new TranslatableString(getKey(@"main.home_controller.testflight"), @"testflight");

        /// <summary>
        /// "information"
        /// </summary>
        public static LocalisableString MainLegalControllerDefault => new TranslatableString(getKey(@"main.legal_controller._"), @"information");

        /// <summary>
        /// "live streams"
        /// </summary>
        public static LocalisableString MainLivestreamsControllerDefault => new TranslatableString(getKey(@"main.livestreams_controller._"), @"live streams");

        /// <summary>
        /// "matches"
        /// </summary>
        public static LocalisableString MainMatchesControllerDefault => new TranslatableString(getKey(@"main.matches_controller._"), @"matches");

        /// <summary>
        /// "news"
        /// </summary>
        public static LocalisableString MainNewsControllerDefault => new TranslatableString(getKey(@"main.news_controller._"), @"news");

        /// <summary>
        /// "notifications history"
        /// </summary>
        public static LocalisableString MainNotificationsControllerDefault => new TranslatableString(getKey(@"main.notifications_controller._"), @"notifications history");

        /// <summary>
        /// "password reset"
        /// </summary>
        public static LocalisableString MainPasswordResetControllerDefault => new TranslatableString(getKey(@"main.password_reset_controller._"), @"password reset");

        /// <summary>
        /// "ranking"
        /// </summary>
        public static LocalisableString MainRankingControllerDefault => new TranslatableString(getKey(@"main.ranking_controller._"), @"ranking");

        /// <summary>
        /// "performance"
        /// </summary>
        public static LocalisableString MainScoresControllerDefault => new TranslatableString(getKey(@"main.scores_controller._"), @"performance");

        /// <summary>
        /// "osu!store"
        /// </summary>
        public static LocalisableString MainStoreControllerDefault => new TranslatableString(getKey(@"main.store_controller._"), @"osu!store");

        /// <summary>
        /// "tournaments"
        /// </summary>
        public static LocalisableString MainTournamentsControllerDefault => new TranslatableString(getKey(@"main.tournaments_controller._"), @"tournaments");

        /// <summary>
        /// "player info"
        /// </summary>
        public static LocalisableString MainUsersControllerDefault => new TranslatableString(getKey(@"main.users_controller._"), @"player info");

        /// <summary>
        /// "notice"
        /// </summary>
        public static LocalisableString MainUsersControllerDisabled => new TranslatableString(getKey(@"main.users_controller.disabled"), @"notice");

        /// <summary>
        /// "knowledge base"
        /// </summary>
        public static LocalisableString MainWikiControllerDefault => new TranslatableString(getKey(@"main.wiki_controller._"), @"knowledge base");

        /// <summary>
        /// "ranking"
        /// </summary>
        public static LocalisableString MultiplayerRoomsControllerDefault => new TranslatableString(getKey(@"multiplayer.rooms_controller._"), @"ranking");

        /// <summary>
        /// "osu!store"
        /// </summary>
        public static LocalisableString StoreDefault => new TranslatableString(getKey(@"store._"), @"osu!store");

        /// <summary>
        /// "modder info"
        /// </summary>
        public static LocalisableString UsersModdingHistoryControllerDefault => new TranslatableString(getKey(@"users.modding_history_controller._"), @"modder info");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}