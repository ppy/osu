// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class HomeStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Home";

        /// <summary>
        /// "Download now"
        /// </summary>
        public static LocalisableString LandingDownload => new TranslatableString(getKey(@"landing.download"), @"Download now");

        /// <summary>
        /// "&lt;strong&gt;{0}&lt;/strong&gt; currently online in &lt;strong&gt;{1}&lt;/strong&gt; games"
        /// </summary>
        public static LocalisableString LandingOnline(string players, string games) => new TranslatableString(getKey(@"landing.online"), @"<strong>{0}</strong> currently online in <strong>{1}</strong> games", players, games);

        /// <summary>
        /// "Peak, {0} online users"
        /// </summary>
        public static LocalisableString LandingPeak(string count) => new TranslatableString(getKey(@"landing.peak"), @"Peak, {0} online users", count);

        /// <summary>
        /// "&lt;strong&gt;{0}&lt;/strong&gt; registered players"
        /// </summary>
        public static LocalisableString LandingPlayers(string count) => new TranslatableString(getKey(@"landing.players"), @"<strong>{0}</strong> registered players", count);

        /// <summary>
        /// "welcome"
        /// </summary>
        public static LocalisableString LandingTitle => new TranslatableString(getKey(@"landing.title"), @"welcome");

        /// <summary>
        /// "see more news"
        /// </summary>
        public static LocalisableString LandingSeeMoreNews => new TranslatableString(getKey(@"landing.see_more_news"), @"see more news");

        /// <summary>
        /// "the bestest free-to-win rhythm game"
        /// </summary>
        public static LocalisableString LandingSloganMain => new TranslatableString(getKey(@"landing.slogan.main"), @"the bestest free-to-win rhythm game");

        /// <summary>
        /// "rhythm is just a click away"
        /// </summary>
        public static LocalisableString LandingSloganSub => new TranslatableString(getKey(@"landing.slogan.sub"), @"rhythm is just a click away");

        /// <summary>
        /// "Advanced search"
        /// </summary>
        public static LocalisableString SearchAdvancedLink => new TranslatableString(getKey(@"search.advanced_link"), @"Advanced search");

        /// <summary>
        /// "Search"
        /// </summary>
        public static LocalisableString SearchButton => new TranslatableString(getKey(@"search.button"), @"Search");

        /// <summary>
        /// "Nothing found!"
        /// </summary>
        public static LocalisableString SearchEmptyResult => new TranslatableString(getKey(@"search.empty_result"), @"Nothing found!");

        /// <summary>
        /// "A search keyword is required"
        /// </summary>
        public static LocalisableString SearchKeywordRequired => new TranslatableString(getKey(@"search.keyword_required"), @"A search keyword is required");

        /// <summary>
        /// "type to search"
        /// </summary>
        public static LocalisableString SearchPlaceholder => new TranslatableString(getKey(@"search.placeholder"), @"type to search");

        /// <summary>
        /// "search"
        /// </summary>
        public static LocalisableString SearchTitle => new TranslatableString(getKey(@"search.title"), @"search");

        /// <summary>
        /// "Sign in to search beatmaps"
        /// </summary>
        public static LocalisableString SearchBeatmapsetLoginRequired => new TranslatableString(getKey(@"search.beatmapset.login_required"), @"Sign in to search beatmaps");

        /// <summary>
        /// "{0} more beatmap search results"
        /// </summary>
        public static LocalisableString SearchBeatmapsetMore(string count) => new TranslatableString(getKey(@"search.beatmapset.more"), @"{0} more beatmap search results", count);

        /// <summary>
        /// "See more beatmap search results"
        /// </summary>
        public static LocalisableString SearchBeatmapsetMoreSimple => new TranslatableString(getKey(@"search.beatmapset.more_simple"), @"See more beatmap search results");

        /// <summary>
        /// "Beatmaps"
        /// </summary>
        public static LocalisableString SearchBeatmapsetTitle => new TranslatableString(getKey(@"search.beatmapset.title"), @"Beatmaps");

        /// <summary>
        /// "All forums"
        /// </summary>
        public static LocalisableString SearchForumPostAll => new TranslatableString(getKey(@"search.forum_post.all"), @"All forums");

        /// <summary>
        /// "Search the forum"
        /// </summary>
        public static LocalisableString SearchForumPostLink => new TranslatableString(getKey(@"search.forum_post.link"), @"Search the forum");

        /// <summary>
        /// "Sign in to search the forum"
        /// </summary>
        public static LocalisableString SearchForumPostLoginRequired => new TranslatableString(getKey(@"search.forum_post.login_required"), @"Sign in to search the forum");

        /// <summary>
        /// "See more forum search results"
        /// </summary>
        public static LocalisableString SearchForumPostMoreSimple => new TranslatableString(getKey(@"search.forum_post.more_simple"), @"See more forum search results");

        /// <summary>
        /// "Forum"
        /// </summary>
        public static LocalisableString SearchForumPostTitle => new TranslatableString(getKey(@"search.forum_post.title"), @"Forum");

        /// <summary>
        /// "search in forums"
        /// </summary>
        public static LocalisableString SearchForumPostLabelForum => new TranslatableString(getKey(@"search.forum_post.label.forum"), @"search in forums");

        /// <summary>
        /// "include subforums"
        /// </summary>
        public static LocalisableString SearchForumPostLabelForumChildren => new TranslatableString(getKey(@"search.forum_post.label.forum_children"), @"include subforums");

        /// <summary>
        /// "topic #"
        /// </summary>
        public static LocalisableString SearchForumPostLabelTopicId => new TranslatableString(getKey(@"search.forum_post.label.topic_id"), @"topic #");

        /// <summary>
        /// "author"
        /// </summary>
        public static LocalisableString SearchForumPostLabelUsername => new TranslatableString(getKey(@"search.forum_post.label.username"), @"author");

        /// <summary>
        /// "all"
        /// </summary>
        public static LocalisableString SearchModeAll => new TranslatableString(getKey(@"search.mode.all"), @"all");

        /// <summary>
        /// "beatmap"
        /// </summary>
        public static LocalisableString SearchModeBeatmapset => new TranslatableString(getKey(@"search.mode.beatmapset"), @"beatmap");

        /// <summary>
        /// "forum"
        /// </summary>
        public static LocalisableString SearchModeForumPost => new TranslatableString(getKey(@"search.mode.forum_post"), @"forum");

        /// <summary>
        /// "player"
        /// </summary>
        public static LocalisableString SearchModeUser => new TranslatableString(getKey(@"search.mode.user"), @"player");

        /// <summary>
        /// "wiki"
        /// </summary>
        public static LocalisableString SearchModeWikiPage => new TranslatableString(getKey(@"search.mode.wiki_page"), @"wiki");

        /// <summary>
        /// "Sign in to search users"
        /// </summary>
        public static LocalisableString SearchUserLoginRequired => new TranslatableString(getKey(@"search.user.login_required"), @"Sign in to search users");

        /// <summary>
        /// "{0} more player search results"
        /// </summary>
        public static LocalisableString SearchUserMore(string count) => new TranslatableString(getKey(@"search.user.more"), @"{0} more player search results", count);

        /// <summary>
        /// "See more player search results"
        /// </summary>
        public static LocalisableString SearchUserMoreSimple => new TranslatableString(getKey(@"search.user.more_simple"), @"See more player search results");

        /// <summary>
        /// "Player search is limited to {0} players. Try refining search query."
        /// </summary>
        public static LocalisableString SearchUserMoreHidden(string max) => new TranslatableString(getKey(@"search.user.more_hidden"), @"Player search is limited to {0} players. Try refining search query.", max);

        /// <summary>
        /// "Players"
        /// </summary>
        public static LocalisableString SearchUserTitle => new TranslatableString(getKey(@"search.user.title"), @"Players");

        /// <summary>
        /// "Search the wiki"
        /// </summary>
        public static LocalisableString SearchWikiPageLink => new TranslatableString(getKey(@"search.wiki_page.link"), @"Search the wiki");

        /// <summary>
        /// "See more wiki search results"
        /// </summary>
        public static LocalisableString SearchWikiPageMoreSimple => new TranslatableString(getKey(@"search.wiki_page.more_simple"), @"See more wiki search results");

        /// <summary>
        /// "Wiki"
        /// </summary>
        public static LocalisableString SearchWikiPageTitle => new TranslatableString(getKey(@"search.wiki_page.title"), @"Wiki");

        /// <summary>
        /// "let&#39;s get&lt;br&gt;you started!"
        /// </summary>
        public static LocalisableString DownloadTagline => new TranslatableString(getKey(@"download.tagline"), @"let's get<br>you started!");

        /// <summary>
        /// "Download osu!"
        /// </summary>
        public static LocalisableString DownloadAction => new TranslatableString(getKey(@"download.action"), @"Download osu!");

        /// <summary>
        /// "if you have problem starting the game or registering for account, {0} or {1}."
        /// </summary>
        public static LocalisableString DownloadHelpDefault(string helpForumLink, string supportButton) => new TranslatableString(getKey(@"download.help._"), @"if you have problem starting the game or registering for account, {0} or {1}.", helpForumLink, supportButton);

        /// <summary>
        /// "check help forum"
        /// </summary>
        public static LocalisableString DownloadHelpHelpForumLink => new TranslatableString(getKey(@"download.help.help_forum_link"), @"check help forum");

        /// <summary>
        /// "contact support"
        /// </summary>
        public static LocalisableString DownloadHelpSupportButton => new TranslatableString(getKey(@"download.help.support_button"), @"contact support");

        /// <summary>
        /// "for Windows"
        /// </summary>
        public static LocalisableString DownloadOsWindows => new TranslatableString(getKey(@"download.os.windows"), @"for Windows");

        /// <summary>
        /// "for macOS"
        /// </summary>
        public static LocalisableString DownloadOsMacos => new TranslatableString(getKey(@"download.os.macos"), @"for macOS");

        /// <summary>
        /// "for Linux"
        /// </summary>
        public static LocalisableString DownloadOsLinux => new TranslatableString(getKey(@"download.os.linux"), @"for Linux");

        /// <summary>
        /// "mirror"
        /// </summary>
        public static LocalisableString DownloadMirror => new TranslatableString(getKey(@"download.mirror"), @"mirror");

        /// <summary>
        /// "macOS users"
        /// </summary>
        public static LocalisableString DownloadMacosFallback => new TranslatableString(getKey(@"download.macos-fallback"), @"macOS users");

        /// <summary>
        /// "get an account"
        /// </summary>
        public static LocalisableString DownloadStepsRegisterTitle => new TranslatableString(getKey(@"download.steps.register.title"), @"get an account");

        /// <summary>
        /// "follow the prompts when starting the game to sign in or make a new account"
        /// </summary>
        public static LocalisableString DownloadStepsRegisterDescription => new TranslatableString(getKey(@"download.steps.register.description"), @"follow the prompts when starting the game to sign in or make a new account");

        /// <summary>
        /// "install the game"
        /// </summary>
        public static LocalisableString DownloadStepsDownloadTitle => new TranslatableString(getKey(@"download.steps.download.title"), @"install the game");

        /// <summary>
        /// "click the button above to download the installer, then run it!"
        /// </summary>
        public static LocalisableString DownloadStepsDownloadDescription => new TranslatableString(getKey(@"download.steps.download.description"), @"click the button above to download the installer, then run it!");

        /// <summary>
        /// "get beatmaps"
        /// </summary>
        public static LocalisableString DownloadStepsBeatmapsTitle => new TranslatableString(getKey(@"download.steps.beatmaps.title"), @"get beatmaps");

        /// <summary>
        /// "{0} the vast library of user-created beatmaps and start playing!"
        /// </summary>
        public static LocalisableString DownloadStepsBeatmapsDescriptionDefault(string browse) => new TranslatableString(getKey(@"download.steps.beatmaps.description._"), @"{0} the vast library of user-created beatmaps and start playing!", browse);

        /// <summary>
        /// "browse"
        /// </summary>
        public static LocalisableString DownloadStepsBeatmapsDescriptionBrowse => new TranslatableString(getKey(@"download.steps.beatmaps.description.browse"), @"browse");

        /// <summary>
        /// "video guide"
        /// </summary>
        public static LocalisableString DownloadVideoGuide => new TranslatableString(getKey(@"download.video-guide"), @"video guide");

        /// <summary>
        /// "dashboard"
        /// </summary>
        public static LocalisableString UserTitle => new TranslatableString(getKey(@"user.title"), @"dashboard");

        /// <summary>
        /// "News"
        /// </summary>
        public static LocalisableString UserNewsTitle => new TranslatableString(getKey(@"user.news.title"), @"News");

        /// <summary>
        /// "Error loading news, try refreshing the page?..."
        /// </summary>
        public static LocalisableString UserNewsError => new TranslatableString(getKey(@"user.news.error"), @"Error loading news, try refreshing the page?...");

        /// <summary>
        /// "Online Friends"
        /// </summary>
        public static LocalisableString UserHeaderStatsFriends => new TranslatableString(getKey(@"user.header.stats.friends"), @"Online Friends");

        /// <summary>
        /// "Games"
        /// </summary>
        public static LocalisableString UserHeaderStatsGames => new TranslatableString(getKey(@"user.header.stats.games"), @"Games");

        /// <summary>
        /// "Online Users"
        /// </summary>
        public static LocalisableString UserHeaderStatsOnline => new TranslatableString(getKey(@"user.header.stats.online"), @"Online Users");

        /// <summary>
        /// "New Ranked Beatmaps"
        /// </summary>
        public static LocalisableString UserBeatmapsNew => new TranslatableString(getKey(@"user.beatmaps.new"), @"New Ranked Beatmaps");

        /// <summary>
        /// "Popular Beatmaps"
        /// </summary>
        public static LocalisableString UserBeatmapsPopular => new TranslatableString(getKey(@"user.beatmaps.popular"), @"Popular Beatmaps");

        /// <summary>
        /// "by {0}"
        /// </summary>
        public static LocalisableString UserBeatmapsByUser(string user) => new TranslatableString(getKey(@"user.beatmaps.by_user"), @"by {0}", user);

        /// <summary>
        /// "Download osu!"
        /// </summary>
        public static LocalisableString UserButtonsDownload => new TranslatableString(getKey(@"user.buttons.download"), @"Download osu!");

        /// <summary>
        /// "Support osu!"
        /// </summary>
        public static LocalisableString UserButtonsSupport => new TranslatableString(getKey(@"user.buttons.support"), @"Support osu!");

        /// <summary>
        /// "osu!store"
        /// </summary>
        public static LocalisableString UserButtonsStore => new TranslatableString(getKey(@"user.buttons.store"), @"osu!store");

        /// <summary>
        /// "Wow!"
        /// </summary>
        public static LocalisableString SupportOsuTitle => new TranslatableString(getKey(@"support-osu.title"), @"Wow!");

        /// <summary>
        /// "You seem to be having a good time! {0}"
        /// </summary>
        public static LocalisableString SupportOsuSubtitle(string d) => new TranslatableString(getKey(@"support-osu.subtitle"), @"You seem to be having a good time! {0}", d);

        /// <summary>
        /// "Did you know that osu! runs with no advertising, and relies on players to support its development and running costs?"
        /// </summary>
        public static LocalisableString SupportOsuBodyPart1 => new TranslatableString(getKey(@"support-osu.body.part-1"), @"Did you know that osu! runs with no advertising, and relies on players to support its development and running costs?");

        /// <summary>
        /// "Did you also know that by supporting osu! you get a heap of useful features, such as &lt;strong&gt;in-game downloading&lt;/strong&gt; which automatically triggers in spectator and multiplayer games?"
        /// </summary>
        public static LocalisableString SupportOsuBodyPart2 => new TranslatableString(getKey(@"support-osu.body.part-2"), @"Did you also know that by supporting osu! you get a heap of useful features, such as <strong>in-game downloading</strong> which automatically triggers in spectator and multiplayer games?");

        /// <summary>
        /// "Click here to find out more!"
        /// </summary>
        public static LocalisableString SupportOsuFindOutMore => new TranslatableString(getKey(@"support-osu.find-out-more"), @"Click here to find out more!");

        /// <summary>
        /// "Oh, and don&#39;t worry - your download has already been started for you already ;)"
        /// </summary>
        public static LocalisableString SupportOsuDownloadStarting => new TranslatableString(getKey(@"support-osu.download-starting"), @"Oh, and don't worry - your download has already been started for you already ;)");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}