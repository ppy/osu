// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class LayoutStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Layout";

        /// <summary>
        /// "Play next track automatically"
        /// </summary>
        public static LocalisableString AudioAutoplay => new TranslatableString(getKey(@"audio.autoplay"), @"Play next track automatically");

        /// <summary>
        /// "osu! - Rhythm is just a *click* away!  With Ouendan/EBA, Taiko and original gameplay modes, as well as a fully functional level editor."
        /// </summary>
        public static LocalisableString DefaultsPageDescription => new TranslatableString(getKey(@"defaults.page_description"), @"osu! - Rhythm is just a *click* away!  With Ouendan/EBA, Taiko and original gameplay modes, as well as a fully functional level editor.");

        /// <summary>
        /// "beatmapset"
        /// </summary>
        public static LocalisableString HeaderAdminBeatmapset => new TranslatableString(getKey(@"header.admin.beatmapset"), @"beatmapset");

        /// <summary>
        /// "beatmapset covers"
        /// </summary>
        public static LocalisableString HeaderAdminBeatmapsetCovers => new TranslatableString(getKey(@"header.admin.beatmapset_covers"), @"beatmapset covers");

        /// <summary>
        /// "contest"
        /// </summary>
        public static LocalisableString HeaderAdminContest => new TranslatableString(getKey(@"header.admin.contest"), @"contest");

        /// <summary>
        /// "contests"
        /// </summary>
        public static LocalisableString HeaderAdminContests => new TranslatableString(getKey(@"header.admin.contests"), @"contests");

        /// <summary>
        /// "console"
        /// </summary>
        public static LocalisableString HeaderAdminRoot => new TranslatableString(getKey(@"header.admin.root"), @"console");

        /// <summary>
        /// "store admin"
        /// </summary>
        public static LocalisableString HeaderAdminStoreOrders => new TranslatableString(getKey(@"header.admin.store_orders"), @"store admin");

        /// <summary>
        /// "listing"
        /// </summary>
        public static LocalisableString HeaderArtistsIndex => new TranslatableString(getKey(@"header.artists.index"), @"listing");

        /// <summary>
        /// "listing"
        /// </summary>
        public static LocalisableString HeaderChangelogIndex => new TranslatableString(getKey(@"header.changelog.index"), @"listing");

        /// <summary>
        /// "index"
        /// </summary>
        public static LocalisableString HeaderHelpIndex => new TranslatableString(getKey(@"header.help.index"), @"index");

        /// <summary>
        /// "Sitemap"
        /// </summary>
        public static LocalisableString HeaderHelpSitemap => new TranslatableString(getKey(@"header.help.sitemap"), @"Sitemap");

        /// <summary>
        /// "cart"
        /// </summary>
        public static LocalisableString HeaderStoreCart => new TranslatableString(getKey(@"header.store.cart"), @"cart");

        /// <summary>
        /// "order history"
        /// </summary>
        public static LocalisableString HeaderStoreOrders => new TranslatableString(getKey(@"header.store.orders"), @"order history");

        /// <summary>
        /// "products"
        /// </summary>
        public static LocalisableString HeaderStoreProducts => new TranslatableString(getKey(@"header.store.products"), @"products");

        /// <summary>
        /// "listing"
        /// </summary>
        public static LocalisableString HeaderTournamentsIndex => new TranslatableString(getKey(@"header.tournaments.index"), @"listing");

        /// <summary>
        /// "modding"
        /// </summary>
        public static LocalisableString HeaderUsersModding => new TranslatableString(getKey(@"header.users.modding"), @"modding");

        /// <summary>
        /// "info"
        /// </summary>
        public static LocalisableString HeaderUsersShow => new TranslatableString(getKey(@"header.users.show"), @"info");

        /// <summary>
        /// "Close (Esc)"
        /// </summary>
        public static LocalisableString GalleryClose => new TranslatableString(getKey(@"gallery.close"), @"Close (Esc)");

        /// <summary>
        /// "Toggle fullscreen"
        /// </summary>
        public static LocalisableString GalleryFullscreen => new TranslatableString(getKey(@"gallery.fullscreen"), @"Toggle fullscreen");

        /// <summary>
        /// "Zoom in/out"
        /// </summary>
        public static LocalisableString GalleryZoom => new TranslatableString(getKey(@"gallery.zoom"), @"Zoom in/out");

        /// <summary>
        /// "Previous (arrow left)"
        /// </summary>
        public static LocalisableString GalleryPrevious => new TranslatableString(getKey(@"gallery.previous"), @"Previous (arrow left)");

        /// <summary>
        /// "Next (arrow right)"
        /// </summary>
        public static LocalisableString GalleryNext => new TranslatableString(getKey(@"gallery.next"), @"Next (arrow right)");

        /// <summary>
        /// "beatmaps"
        /// </summary>
        public static LocalisableString MenuBeatmapsDefault => new TranslatableString(getKey(@"menu.beatmaps._"), @"beatmaps");

        /// <summary>
        /// "featured artists"
        /// </summary>
        public static LocalisableString MenuBeatmapsArtists => new TranslatableString(getKey(@"menu.beatmaps.artists"), @"featured artists");

        /// <summary>
        /// "listing"
        /// </summary>
        public static LocalisableString MenuBeatmapsIndex => new TranslatableString(getKey(@"menu.beatmaps.index"), @"listing");

        /// <summary>
        /// "packs"
        /// </summary>
        public static LocalisableString MenuBeatmapsPacks => new TranslatableString(getKey(@"menu.beatmaps.packs"), @"packs");

        /// <summary>
        /// "community"
        /// </summary>
        public static LocalisableString MenuCommunityDefault => new TranslatableString(getKey(@"menu.community._"), @"community");

        /// <summary>
        /// "chat"
        /// </summary>
        public static LocalisableString MenuCommunityChat => new TranslatableString(getKey(@"menu.community.chat"), @"chat");

        /// <summary>
        /// "contests"
        /// </summary>
        public static LocalisableString MenuCommunityContests => new TranslatableString(getKey(@"menu.community.contests"), @"contests");

        /// <summary>
        /// "development"
        /// </summary>
        public static LocalisableString MenuCommunityDev => new TranslatableString(getKey(@"menu.community.dev"), @"development");

        /// <summary>
        /// "forums"
        /// </summary>
        public static LocalisableString MenuCommunityForumForumsIndex => new TranslatableString(getKey(@"menu.community.forum-forums-index"), @"forums");

        /// <summary>
        /// "live"
        /// </summary>
        public static LocalisableString MenuCommunityGetLive => new TranslatableString(getKey(@"menu.community.getlive"), @"live");

        /// <summary>
        /// "tournaments"
        /// </summary>
        public static LocalisableString MenuCommunityTournaments => new TranslatableString(getKey(@"menu.community.tournaments"), @"tournaments");

        /// <summary>
        /// "help"
        /// </summary>
        public static LocalisableString MenuHelpDefault => new TranslatableString(getKey(@"menu.help._"), @"help");

        /// <summary>
        /// "report abuse"
        /// </summary>
        public static LocalisableString MenuHelpGetAbuse => new TranslatableString(getKey(@"menu.help.getabuse"), @"report abuse");

        /// <summary>
        /// "faq"
        /// </summary>
        public static LocalisableString MenuHelpGetFaq => new TranslatableString(getKey(@"menu.help.getfaq"), @"faq");

        /// <summary>
        /// "rules"
        /// </summary>
        public static LocalisableString MenuHelpGetRules => new TranslatableString(getKey(@"menu.help.getrules"), @"rules");

        /// <summary>
        /// "no, really, i need help!"
        /// </summary>
        public static LocalisableString MenuHelpGetSupport => new TranslatableString(getKey(@"menu.help.getsupport"), @"no, really, i need help!");

        /// <summary>
        /// "wiki"
        /// </summary>
        public static LocalisableString MenuHelpGetWiki => new TranslatableString(getKey(@"menu.help.getwiki"), @"wiki");

        /// <summary>
        /// "home"
        /// </summary>
        public static LocalisableString MenuHomeDefault => new TranslatableString(getKey(@"menu.home._"), @"home");

        /// <summary>
        /// "changelog"
        /// </summary>
        public static LocalisableString MenuHomeChangelogIndex => new TranslatableString(getKey(@"menu.home.changelog-index"), @"changelog");

        /// <summary>
        /// "download"
        /// </summary>
        public static LocalisableString MenuHomeGetDownload => new TranslatableString(getKey(@"menu.home.getdownload"), @"download");

        /// <summary>
        /// "news"
        /// </summary>
        public static LocalisableString MenuHomeNewsIndex => new TranslatableString(getKey(@"menu.home.news-index"), @"news");

        /// <summary>
        /// "search"
        /// </summary>
        public static LocalisableString MenuHomeSearch => new TranslatableString(getKey(@"menu.home.search"), @"search");

        /// <summary>
        /// "team"
        /// </summary>
        public static LocalisableString MenuHomeTeam => new TranslatableString(getKey(@"menu.home.team"), @"team");

        /// <summary>
        /// "rankings"
        /// </summary>
        public static LocalisableString MenuRankingsDefault => new TranslatableString(getKey(@"menu.rankings._"), @"rankings");

        /// <summary>
        /// "spotlights"
        /// </summary>
        public static LocalisableString MenuRankingsCharts => new TranslatableString(getKey(@"menu.rankings.charts"), @"spotlights");

        /// <summary>
        /// "country"
        /// </summary>
        public static LocalisableString MenuRankingsCountry => new TranslatableString(getKey(@"menu.rankings.country"), @"country");

        /// <summary>
        /// "performance"
        /// </summary>
        public static LocalisableString MenuRankingsIndex => new TranslatableString(getKey(@"menu.rankings.index"), @"performance");

        /// <summary>
        /// "kudosu"
        /// </summary>
        public static LocalisableString MenuRankingsKudosu => new TranslatableString(getKey(@"menu.rankings.kudosu"), @"kudosu");

        /// <summary>
        /// "multiplayer"
        /// </summary>
        public static LocalisableString MenuRankingsMultiplayer => new TranslatableString(getKey(@"menu.rankings.multiplayer"), @"multiplayer");

        /// <summary>
        /// "score"
        /// </summary>
        public static LocalisableString MenuRankingsScore => new TranslatableString(getKey(@"menu.rankings.score"), @"score");

        /// <summary>
        /// "store"
        /// </summary>
        public static LocalisableString MenuStoreDefault => new TranslatableString(getKey(@"menu.store._"), @"store");

        /// <summary>
        /// "cart"
        /// </summary>
        public static LocalisableString MenuStoreCartShow => new TranslatableString(getKey(@"menu.store.cart-show"), @"cart");

        /// <summary>
        /// "listing"
        /// </summary>
        public static LocalisableString MenuStoreGetListing => new TranslatableString(getKey(@"menu.store.getlisting"), @"listing");

        /// <summary>
        /// "order history"
        /// </summary>
        public static LocalisableString MenuStoreOrdersIndex => new TranslatableString(getKey(@"menu.store.orders-index"), @"order history");

        /// <summary>
        /// "General"
        /// </summary>
        public static LocalisableString FooterGeneralDefault => new TranslatableString(getKey(@"footer.general._"), @"General");

        /// <summary>
        /// "Home"
        /// </summary>
        public static LocalisableString FooterGeneralHome => new TranslatableString(getKey(@"footer.general.home"), @"Home");

        /// <summary>
        /// "Changelog"
        /// </summary>
        public static LocalisableString FooterGeneralChangelogIndex => new TranslatableString(getKey(@"footer.general.changelog-index"), @"Changelog");

        /// <summary>
        /// "Beatmap Listing"
        /// </summary>
        public static LocalisableString FooterGeneralBeatmaps => new TranslatableString(getKey(@"footer.general.beatmaps"), @"Beatmap Listing");

        /// <summary>
        /// "Download osu!"
        /// </summary>
        public static LocalisableString FooterGeneralDownload => new TranslatableString(getKey(@"footer.general.download"), @"Download osu!");

        /// <summary>
        /// "Help &amp; Community"
        /// </summary>
        public static LocalisableString FooterHelpDefault => new TranslatableString(getKey(@"footer.help._"), @"Help & Community");

        /// <summary>
        /// "Frequently Asked Questions"
        /// </summary>
        public static LocalisableString FooterHelpFaq => new TranslatableString(getKey(@"footer.help.faq"), @"Frequently Asked Questions");

        /// <summary>
        /// "Community Forums"
        /// </summary>
        public static LocalisableString FooterHelpForum => new TranslatableString(getKey(@"footer.help.forum"), @"Community Forums");

        /// <summary>
        /// "Live Streams"
        /// </summary>
        public static LocalisableString FooterHelpLivestreams => new TranslatableString(getKey(@"footer.help.livestreams"), @"Live Streams");

        /// <summary>
        /// "Report an Issue"
        /// </summary>
        public static LocalisableString FooterHelpReport => new TranslatableString(getKey(@"footer.help.report"), @"Report an Issue");

        /// <summary>
        /// "Wiki"
        /// </summary>
        public static LocalisableString FooterHelpWiki => new TranslatableString(getKey(@"footer.help.wiki"), @"Wiki");

        /// <summary>
        /// "Legal &amp; Status"
        /// </summary>
        public static LocalisableString FooterLegalDefault => new TranslatableString(getKey(@"footer.legal._"), @"Legal & Status");

        /// <summary>
        /// "Copyright (DMCA)"
        /// </summary>
        public static LocalisableString FooterLegalCopyright => new TranslatableString(getKey(@"footer.legal.copyright"), @"Copyright (DMCA)");

        /// <summary>
        /// "Privacy"
        /// </summary>
        public static LocalisableString FooterLegalPrivacy => new TranslatableString(getKey(@"footer.legal.privacy"), @"Privacy");

        /// <summary>
        /// "Server Status"
        /// </summary>
        public static LocalisableString FooterLegalServerStatus => new TranslatableString(getKey(@"footer.legal.server_status"), @"Server Status");

        /// <summary>
        /// "Source Code"
        /// </summary>
        public static LocalisableString FooterLegalSourceCode => new TranslatableString(getKey(@"footer.legal.source_code"), @"Source Code");

        /// <summary>
        /// "Terms"
        /// </summary>
        public static LocalisableString FooterLegalTerms => new TranslatableString(getKey(@"footer.legal.terms"), @"Terms");

        /// <summary>
        /// "Invalid request parameter"
        /// </summary>
        public static LocalisableString Errors400Error => new TranslatableString(getKey(@"errors.400.error"), @"Invalid request parameter");

        /// <summary>
        /// ""
        /// </summary>
        public static LocalisableString Errors400Description => new TranslatableString(getKey(@"errors.400.description"), @"");

        /// <summary>
        /// "Page Missing"
        /// </summary>
        public static LocalisableString Errors404Error => new TranslatableString(getKey(@"errors.404.error"), @"Page Missing");

        /// <summary>
        /// "Sorry, but the page you requested isn&#39;t here!"
        /// </summary>
        public static LocalisableString Errors404Description => new TranslatableString(getKey(@"errors.404.description"), @"Sorry, but the page you requested isn't here!");

        /// <summary>
        /// "You shouldn&#39;t be here."
        /// </summary>
        public static LocalisableString Errors403Error => new TranslatableString(getKey(@"errors.403.error"), @"You shouldn't be here.");

        /// <summary>
        /// "You could try going back, though."
        /// </summary>
        public static LocalisableString Errors403Description => new TranslatableString(getKey(@"errors.403.description"), @"You could try going back, though.");

        /// <summary>
        /// "You shouldn&#39;t be here."
        /// </summary>
        public static LocalisableString Errors401Error => new TranslatableString(getKey(@"errors.401.error"), @"You shouldn't be here.");

        /// <summary>
        /// "You could try going back, though. Or maybe signing in."
        /// </summary>
        public static LocalisableString Errors401Description => new TranslatableString(getKey(@"errors.401.description"), @"You could try going back, though. Or maybe signing in.");

        /// <summary>
        /// "Page Missing"
        /// </summary>
        public static LocalisableString Errors405Error => new TranslatableString(getKey(@"errors.405.error"), @"Page Missing");

        /// <summary>
        /// "Sorry, but the page you requested isn&#39;t here!"
        /// </summary>
        public static LocalisableString Errors405Description => new TranslatableString(getKey(@"errors.405.description"), @"Sorry, but the page you requested isn't here!");

        /// <summary>
        /// "Invalid request parameter"
        /// </summary>
        public static LocalisableString Errors422Error => new TranslatableString(getKey(@"errors.422.error"), @"Invalid request parameter");

        /// <summary>
        /// ""
        /// </summary>
        public static LocalisableString Errors422Description => new TranslatableString(getKey(@"errors.422.description"), @"");

        /// <summary>
        /// "Rate limit exceeded"
        /// </summary>
        public static LocalisableString Errors429Error => new TranslatableString(getKey(@"errors.429.error"), @"Rate limit exceeded");

        /// <summary>
        /// ""
        /// </summary>
        public static LocalisableString Errors429Description => new TranslatableString(getKey(@"errors.429.description"), @"");

        /// <summary>
        /// "Oh no! Something broke! ;_;"
        /// </summary>
        public static LocalisableString Errors500Error => new TranslatableString(getKey(@"errors.500.error"), @"Oh no! Something broke! ;_;");

        /// <summary>
        /// "We&#39;re automatically notified of every error."
        /// </summary>
        public static LocalisableString Errors500Description => new TranslatableString(getKey(@"errors.500.description"), @"We're automatically notified of every error.");

        /// <summary>
        /// "Oh no! Something broke (badly)! ;_;"
        /// </summary>
        public static LocalisableString ErrorsFatalError => new TranslatableString(getKey(@"errors.fatal.error"), @"Oh no! Something broke (badly)! ;_;");

        /// <summary>
        /// "We&#39;re automatically notified of every error."
        /// </summary>
        public static LocalisableString ErrorsFatalDescription => new TranslatableString(getKey(@"errors.fatal.description"), @"We're automatically notified of every error.");

        /// <summary>
        /// "Down for maintenance!"
        /// </summary>
        public static LocalisableString Errors503Error => new TranslatableString(getKey(@"errors.503.error"), @"Down for maintenance!");

        /// <summary>
        /// "Maintenance usually takes anywhere from 5 seconds to 10 minutes. If we&#39;re down for longer, see {0} for more information."
        /// </summary>
        public static LocalisableString Errors503Description(string link) => new TranslatableString(getKey(@"errors.503.description"), @"Maintenance usually takes anywhere from 5 seconds to 10 minutes. If we're down for longer, see {0} for more information.", link);

        /// <summary>
        /// "@osustatus"
        /// </summary>
        public static LocalisableString Errors503LinkText => new TranslatableString(getKey(@"errors.503.link.text"), @"@osustatus");

        /// <summary>
        /// "https://twitter.com/osustatus"
        /// </summary>
        public static LocalisableString Errors503LinkHref => new TranslatableString(getKey(@"errors.503.link.href"), @"https://twitter.com/osustatus");

        /// <summary>
        /// "Just in case, here&#39;s a code you can give to support!"
        /// </summary>
        public static LocalisableString ErrorsReference => new TranslatableString(getKey(@"errors.reference"), @"Just in case, here's a code you can give to support!");

        /// <summary>
        /// "sign in / register"
        /// </summary>
        public static LocalisableString PopupLoginButton => new TranslatableString(getKey(@"popup_login.button"), @"sign in / register");

        /// <summary>
        /// "I&#39;ve forgotten my details"
        /// </summary>
        public static LocalisableString PopupLoginLoginForgot => new TranslatableString(getKey(@"popup_login.login.forgot"), @"I've forgotten my details");

        /// <summary>
        /// "password"
        /// </summary>
        public static LocalisableString PopupLoginLoginPassword => new TranslatableString(getKey(@"popup_login.login.password"), @"password");

        /// <summary>
        /// "Sign In To Proceed"
        /// </summary>
        public static LocalisableString PopupLoginLoginTitle => new TranslatableString(getKey(@"popup_login.login.title"), @"Sign In To Proceed");

        /// <summary>
        /// "username"
        /// </summary>
        public static LocalisableString PopupLoginLoginUsername => new TranslatableString(getKey(@"popup_login.login.username"), @"username");

        /// <summary>
        /// "Username or email address doesn&#39;t exist"
        /// </summary>
        public static LocalisableString PopupLoginLoginErrorEmail => new TranslatableString(getKey(@"popup_login.login.error.email"), @"Username or email address doesn't exist");

        /// <summary>
        /// "Incorrect password"
        /// </summary>
        public static LocalisableString PopupLoginLoginErrorPassword => new TranslatableString(getKey(@"popup_login.login.error.password"), @"Incorrect password");

        /// <summary>
        /// "Download"
        /// </summary>
        public static LocalisableString PopupLoginRegisterDownload => new TranslatableString(getKey(@"popup_login.register.download"), @"Download");

        /// <summary>
        /// "Download osu! to create your own account!"
        /// </summary>
        public static LocalisableString PopupLoginRegisterInfo => new TranslatableString(getKey(@"popup_login.register.info"), @"Download osu! to create your own account!");

        /// <summary>
        /// "Don&#39;t have an account?"
        /// </summary>
        public static LocalisableString PopupLoginRegisterTitle => new TranslatableString(getKey(@"popup_login.register.title"), @"Don't have an account?");

        /// <summary>
        /// "Settings"
        /// </summary>
        public static LocalisableString PopupUserLinksAccountEdit => new TranslatableString(getKey(@"popup_user.links.account-edit"), @"Settings");

        /// <summary>
        /// "Watchlists"
        /// </summary>
        public static LocalisableString PopupUserLinksFollows => new TranslatableString(getKey(@"popup_user.links.follows"), @"Watchlists");

        /// <summary>
        /// "Friends"
        /// </summary>
        public static LocalisableString PopupUserLinksFriends => new TranslatableString(getKey(@"popup_user.links.friends"), @"Friends");

        /// <summary>
        /// "Sign Out"
        /// </summary>
        public static LocalisableString PopupUserLinksLogout => new TranslatableString(getKey(@"popup_user.links.logout"), @"Sign Out");

        /// <summary>
        /// "My Profile"
        /// </summary>
        public static LocalisableString PopupUserLinksProfile => new TranslatableString(getKey(@"popup_user.links.profile"), @"My Profile");

        /// <summary>
        /// "Type to search!"
        /// </summary>
        public static LocalisableString PopupSearchInitial => new TranslatableString(getKey(@"popup_search.initial"), @"Type to search!");

        /// <summary>
        /// "Search failed. Click to retry."
        /// </summary>
        public static LocalisableString PopupSearchRetry => new TranslatableString(getKey(@"popup_search.retry"), @"Search failed. Click to retry.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}