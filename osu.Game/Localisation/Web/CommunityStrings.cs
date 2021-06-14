// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class CommunityStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Community";

        /// <summary>
        /// "I&#39;m convinced! {0}"
        /// </summary>
        public static LocalisableString SupportConvincedTitle(string d) => new TranslatableString(getKey(@"support.convinced.title"), @"I'm convinced! {0}", d);

        /// <summary>
        /// "support osu!"
        /// </summary>
        public static LocalisableString SupportConvincedSupport => new TranslatableString(getKey(@"support.convinced.support"), @"support osu!");

        /// <summary>
        /// "or gift supporter to other players"
        /// </summary>
        public static LocalisableString SupportConvincedGift => new TranslatableString(getKey(@"support.convinced.gift"), @"or gift supporter to other players");

        /// <summary>
        /// "click the heart button to proceed to the osu!store"
        /// </summary>
        public static LocalisableString SupportConvincedInstructions => new TranslatableString(getKey(@"support.convinced.instructions"), @"click the heart button to proceed to the osu!store");

        /// <summary>
        /// "Why should I support osu!? Where does the money go?"
        /// </summary>
        public static LocalisableString SupportWhySupportTitle => new TranslatableString(getKey(@"support.why-support.title"), @"Why should I support osu!? Where does the money go?");

        /// <summary>
        /// "Support the Team"
        /// </summary>
        public static LocalisableString SupportWhySupportTeamTitle => new TranslatableString(getKey(@"support.why-support.team.title"), @"Support the Team");

        /// <summary>
        /// "A small team develops and runs osu!. Your support helps them to, you know... live."
        /// </summary>
        public static LocalisableString SupportWhySupportTeamDescription => new TranslatableString(getKey(@"support.why-support.team.description"), @"A small team develops and runs osu!. Your support helps them to, you know... live.");

        /// <summary>
        /// "Server Infrastructure"
        /// </summary>
        public static LocalisableString SupportWhySupportInfraTitle => new TranslatableString(getKey(@"support.why-support.infra.title"), @"Server Infrastructure");

        /// <summary>
        /// "Contributions go towards the servers for running the website, multiplayer services, online leaderboards, etc."
        /// </summary>
        public static LocalisableString SupportWhySupportInfraDescription => new TranslatableString(getKey(@"support.why-support.infra.description"), @"Contributions go towards the servers for running the website, multiplayer services, online leaderboards, etc.");

        /// <summary>
        /// "Featured Artists"
        /// </summary>
        public static LocalisableString SupportWhySupportFeaturedArtistsTitle => new TranslatableString(getKey(@"support.why-support.featured-artists.title"), @"Featured Artists");

        /// <summary>
        /// "With your support, we can approach even more awesome artists and license more great music for use in osu!"
        /// </summary>
        public static LocalisableString SupportWhySupportFeaturedArtistsDescription => new TranslatableString(getKey(@"support.why-support.featured-artists.description"), @"With your support, we can approach even more awesome artists and license more great music for use in osu!");

        /// <summary>
        /// "View the current roster &amp;raquo;"
        /// </summary>
        public static LocalisableString SupportWhySupportFeaturedArtistsLinkText => new TranslatableString(getKey(@"support.why-support.featured-artists.link_text"), @"View the current roster &raquo;");

        /// <summary>
        /// "Keep osu! self-sustaining"
        /// </summary>
        public static LocalisableString SupportWhySupportAdsTitle => new TranslatableString(getKey(@"support.why-support.ads.title"), @"Keep osu! self-sustaining");

        /// <summary>
        /// "Your contributions help keep the game independent and completely free from ads and outside sponsors."
        /// </summary>
        public static LocalisableString SupportWhySupportAdsDescription => new TranslatableString(getKey(@"support.why-support.ads.description"), @"Your contributions help keep the game independent and completely free from ads and outside sponsors.");

        /// <summary>
        /// "Official Tournaments"
        /// </summary>
        public static LocalisableString SupportWhySupportTournamentsTitle => new TranslatableString(getKey(@"support.why-support.tournaments.title"), @"Official Tournaments");

        /// <summary>
        /// "Help fund the running of (and the prizes for) the official osu! World Cup tournaments."
        /// </summary>
        public static LocalisableString SupportWhySupportTournamentsDescription => new TranslatableString(getKey(@"support.why-support.tournaments.description"), @"Help fund the running of (and the prizes for) the official osu! World Cup tournaments.");

        /// <summary>
        /// "Explore tournaments &amp;raquo;"
        /// </summary>
        public static LocalisableString SupportWhySupportTournamentsLinkText => new TranslatableString(getKey(@"support.why-support.tournaments.link_text"), @"Explore tournaments &raquo;");

        /// <summary>
        /// "Open Source Bounty Program"
        /// </summary>
        public static LocalisableString SupportWhySupportBountyProgramTitle => new TranslatableString(getKey(@"support.why-support.bounty-program.title"), @"Open Source Bounty Program");

        /// <summary>
        /// "Support the community contributors that have given their time and effort to help make osu! better."
        /// </summary>
        public static LocalisableString SupportWhySupportBountyProgramDescription => new TranslatableString(getKey(@"support.why-support.bounty-program.description"), @"Support the community contributors that have given their time and effort to help make osu! better.");

        /// <summary>
        /// "Find out more &amp;raquo;"
        /// </summary>
        public static LocalisableString SupportWhySupportBountyProgramLinkText => new TranslatableString(getKey(@"support.why-support.bounty-program.link_text"), @"Find out more &raquo;");

        /// <summary>
        /// "Cool! What perks do I get?"
        /// </summary>
        public static LocalisableString SupportPerksTitle => new TranslatableString(getKey(@"support.perks.title"), @"Cool! What perks do I get?");

        /// <summary>
        /// "osu!direct"
        /// </summary>
        public static LocalisableString SupportPerksOsuDirectTitle => new TranslatableString(getKey(@"support.perks.osu_direct.title"), @"osu!direct");

        /// <summary>
        /// "Gain quick and easy access to search for and download beatmaps without having to leave the game."
        /// </summary>
        public static LocalisableString SupportPerksOsuDirectDescription => new TranslatableString(getKey(@"support.perks.osu_direct.description"), @"Gain quick and easy access to search for and download beatmaps without having to leave the game.");

        /// <summary>
        /// "Friend Ranking"
        /// </summary>
        public static LocalisableString SupportPerksFriendRankingTitle => new TranslatableString(getKey(@"support.perks.friend_ranking.title"), @"Friend Ranking");

        /// <summary>
        /// "See how you stack up against your friends on a beatmap&#39;s leaderboard, both in-game and on the website."
        /// </summary>
        public static LocalisableString SupportPerksFriendRankingDescription => new TranslatableString(getKey(@"support.perks.friend_ranking.description"), @"See how you stack up against your friends on a beatmap's leaderboard, both in-game and on the website.");

        /// <summary>
        /// "Country Ranking"
        /// </summary>
        public static LocalisableString SupportPerksCountryRankingTitle => new TranslatableString(getKey(@"support.perks.country_ranking.title"), @"Country Ranking");

        /// <summary>
        /// "Conquer your country before you conquer the world."
        /// </summary>
        public static LocalisableString SupportPerksCountryRankingDescription => new TranslatableString(getKey(@"support.perks.country_ranking.description"), @"Conquer your country before you conquer the world.");

        /// <summary>
        /// "Filter by Mods"
        /// </summary>
        public static LocalisableString SupportPerksModFilteringTitle => new TranslatableString(getKey(@"support.perks.mod_filtering.title"), @"Filter by Mods");

        /// <summary>
        /// "Associate only with people who play HDHR? No problem!"
        /// </summary>
        public static LocalisableString SupportPerksModFilteringDescription => new TranslatableString(getKey(@"support.perks.mod_filtering.description"), @"Associate only with people who play HDHR? No problem!");

        /// <summary>
        /// "Automatic Downloads"
        /// </summary>
        public static LocalisableString SupportPerksAutoDownloadsTitle => new TranslatableString(getKey(@"support.perks.auto_downloads.title"), @"Automatic Downloads");

        /// <summary>
        /// "Beatmaps will automatically download in multiplayer games, while spectating others, or when clicking relevant links in chat!"
        /// </summary>
        public static LocalisableString SupportPerksAutoDownloadsDescription => new TranslatableString(getKey(@"support.perks.auto_downloads.description"), @"Beatmaps will automatically download in multiplayer games, while spectating others, or when clicking relevant links in chat!");

        /// <summary>
        /// "Upload More"
        /// </summary>
        public static LocalisableString SupportPerksUploadMoreTitle => new TranslatableString(getKey(@"support.perks.upload_more.title"), @"Upload More");

        /// <summary>
        /// "Additional pending beatmap slots (per ranked beatmap) up to a max of 10."
        /// </summary>
        public static LocalisableString SupportPerksUploadMoreDescription => new TranslatableString(getKey(@"support.perks.upload_more.description"), @"Additional pending beatmap slots (per ranked beatmap) up to a max of 10.");

        /// <summary>
        /// "Early Access"
        /// </summary>
        public static LocalisableString SupportPerksEarlyAccessTitle => new TranslatableString(getKey(@"support.perks.early_access.title"), @"Early Access");

        /// <summary>
        /// "Gain early access to new releases with new features before they go public!&lt;br/&gt;&lt;br/&gt;This includes early access to new features on the website too!"
        /// </summary>
        public static LocalisableString SupportPerksEarlyAccessDescription => new TranslatableString(getKey(@"support.perks.early_access.description"), @"Gain early access to new releases with new features before they go public!<br/><br/>This includes early access to new features on the website too!");

        /// <summary>
        /// "Customisation"
        /// </summary>
        public static LocalisableString SupportPerksCustomisationTitle => new TranslatableString(getKey(@"support.perks.customisation.title"), @"Customisation");

        /// <summary>
        /// "Stand out by uploading a custom cover image or by creating a fully customizable &#39;me!&#39; section within your user profile."
        /// </summary>
        public static LocalisableString SupportPerksCustomisationDescription => new TranslatableString(getKey(@"support.perks.customisation.description"), @"Stand out by uploading a custom cover image or by creating a fully customizable 'me!' section within your user profile.");

        /// <summary>
        /// "Beatmap Filters"
        /// </summary>
        public static LocalisableString SupportPerksBeatmapFiltersTitle => new TranslatableString(getKey(@"support.perks.beatmap_filters.title"), @"Beatmap Filters");

        /// <summary>
        /// "Filter beatmap searches by played and unplayed maps, or by rank achieved."
        /// </summary>
        public static LocalisableString SupportPerksBeatmapFiltersDescription => new TranslatableString(getKey(@"support.perks.beatmap_filters.description"), @"Filter beatmap searches by played and unplayed maps, or by rank achieved.");

        /// <summary>
        /// "Yellow Fellow"
        /// </summary>
        public static LocalisableString SupportPerksYellowFellowTitle => new TranslatableString(getKey(@"support.perks.yellow_fellow.title"), @"Yellow Fellow");

        /// <summary>
        /// "Be recognised in-game with your new bright yellow chat username colour."
        /// </summary>
        public static LocalisableString SupportPerksYellowFellowDescription => new TranslatableString(getKey(@"support.perks.yellow_fellow.description"), @"Be recognised in-game with your new bright yellow chat username colour.");

        /// <summary>
        /// "Speedy Downloads"
        /// </summary>
        public static LocalisableString SupportPerksSpeedyDownloadsTitle => new TranslatableString(getKey(@"support.perks.speedy_downloads.title"), @"Speedy Downloads");

        /// <summary>
        /// "More lenient download restrictions, especially when using osu!direct."
        /// </summary>
        public static LocalisableString SupportPerksSpeedyDownloadsDescription => new TranslatableString(getKey(@"support.perks.speedy_downloads.description"), @"More lenient download restrictions, especially when using osu!direct.");

        /// <summary>
        /// "Change Username"
        /// </summary>
        public static LocalisableString SupportPerksChangeUsernameTitle => new TranslatableString(getKey(@"support.perks.change_username.title"), @"Change Username");

        /// <summary>
        /// "One free name change is included with your first supporter purchase."
        /// </summary>
        public static LocalisableString SupportPerksChangeUsernameDescription => new TranslatableString(getKey(@"support.perks.change_username.description"), @"One free name change is included with your first supporter purchase.");

        /// <summary>
        /// "Skinnables"
        /// </summary>
        public static LocalisableString SupportPerksSkinnablesTitle => new TranslatableString(getKey(@"support.perks.skinnables.title"), @"Skinnables");

        /// <summary>
        /// "Extra in-game skinnables, like the main menu background."
        /// </summary>
        public static LocalisableString SupportPerksSkinnablesDescription => new TranslatableString(getKey(@"support.perks.skinnables.description"), @"Extra in-game skinnables, like the main menu background.");

        /// <summary>
        /// "Feature Votes"
        /// </summary>
        public static LocalisableString SupportPerksFeatureVotesTitle => new TranslatableString(getKey(@"support.perks.feature_votes.title"), @"Feature Votes");

        /// <summary>
        /// "Votes for feature requests. (2 per month)"
        /// </summary>
        public static LocalisableString SupportPerksFeatureVotesDescription => new TranslatableString(getKey(@"support.perks.feature_votes.description"), @"Votes for feature requests. (2 per month)");

        /// <summary>
        /// "Sort Options"
        /// </summary>
        public static LocalisableString SupportPerksSortOptionsTitle => new TranslatableString(getKey(@"support.perks.sort_options.title"), @"Sort Options");

        /// <summary>
        /// "The ability to view beatmap country / friend / mod-specific rankings in-game."
        /// </summary>
        public static LocalisableString SupportPerksSortOptionsDescription => new TranslatableString(getKey(@"support.perks.sort_options.description"), @"The ability to view beatmap country / friend / mod-specific rankings in-game.");

        /// <summary>
        /// "More Favourites"
        /// </summary>
        public static LocalisableString SupportPerksMoreFavouritesTitle => new TranslatableString(getKey(@"support.perks.more_favourites.title"), @"More Favourites");

        /// <summary>
        /// "The maximum number of beatmaps you can favourite is increased from {0} &amp;rarr; {1}"
        /// </summary>
        public static LocalisableString SupportPerksMoreFavouritesDescription(string normally, string supporter) => new TranslatableString(getKey(@"support.perks.more_favourites.description"), @"The maximum number of beatmaps you can favourite is increased from {0} &rarr; {1}", normally, supporter);

        /// <summary>
        /// "More Friends"
        /// </summary>
        public static LocalisableString SupportPerksMoreFriendsTitle => new TranslatableString(getKey(@"support.perks.more_friends.title"), @"More Friends");

        /// <summary>
        /// "The maximum number of friends you can have is increased from {0} &amp;rarr; {1}"
        /// </summary>
        public static LocalisableString SupportPerksMoreFriendsDescription(string normally, string supporter) => new TranslatableString(getKey(@"support.perks.more_friends.description"), @"The maximum number of friends you can have is increased from {0} &rarr; {1}", normally, supporter);

        /// <summary>
        /// "Upload More Beatmaps"
        /// </summary>
        public static LocalisableString SupportPerksMoreBeatmapsTitle => new TranslatableString(getKey(@"support.perks.more_beatmaps.title"), @"Upload More Beatmaps");

        /// <summary>
        /// "How many pending beatmaps you can have at once is calculated from a base value plus an additional bonus for each ranked beatmap you currently have (up to a limit).&lt;br/&gt;&lt;br/&gt;Normally this is {0} plus {1} per ranked beatmap (up to {2}). With supporter, this increases to {3} plus {4} per ranked beatmap (up to {5})."
        /// </summary>
        public static LocalisableString SupportPerksMoreBeatmapsDescription(string @base, string bonus, string bonusMax, string supporterBase, string supporterBonus, string supporterBonusMax) => new TranslatableString(getKey(@"support.perks.more_beatmaps.description"), @"How many pending beatmaps you can have at once is calculated from a base value plus an additional bonus for each ranked beatmap you currently have (up to a limit).<br/><br/>Normally this is {0} plus {1} per ranked beatmap (up to {2}). With supporter, this increases to {3} plus {4} per ranked beatmap (up to {5}).", @base, bonus, bonusMax, supporterBase, supporterBonus, supporterBonusMax);

        /// <summary>
        /// "Friend Leaderboards"
        /// </summary>
        public static LocalisableString SupportPerksFriendFilteringTitle => new TranslatableString(getKey(@"support.perks.friend_filtering.title"), @"Friend Leaderboards");

        /// <summary>
        /// "Compete with your friends and see how you rank up against them!"
        /// </summary>
        public static LocalisableString SupportPerksFriendFilteringDescription => new TranslatableString(getKey(@"support.perks.friend_filtering.description"), @"Compete with your friends and see how you rank up against them!");

        /// <summary>
        /// "Thanks for your support so far! You have contributed {0} over {1} tag purchases!"
        /// </summary>
        public static LocalisableString SupportSupporterStatusContribution(string dollars, string tags) => new TranslatableString(getKey(@"support.supporter_status.contribution"), @"Thanks for your support so far! You have contributed {0} over {1} tag purchases!", dollars, tags);

        /// <summary>
        /// "You have given away {0} of your purchases as gifts (that&#39;s {1} worth), how generous!"
        /// </summary>
        public static LocalisableString SupportSupporterStatusGifted(string giftedTags, string giftedDollars) => new TranslatableString(getKey(@"support.supporter_status.gifted"), @"You have given away {0} of your purchases as gifts (that's {1} worth), how generous!", giftedTags, giftedDollars);

        /// <summary>
        /// "You haven&#39;t ever had an osu!supporter tag :("
        /// </summary>
        public static LocalisableString SupportSupporterStatusNotYet => new TranslatableString(getKey(@"support.supporter_status.not_yet"), @"You haven't ever had an osu!supporter tag :(");

        /// <summary>
        /// "Your current osu!supporter tag is valid until {0}!"
        /// </summary>
        public static LocalisableString SupportSupporterStatusValidUntil(string date) => new TranslatableString(getKey(@"support.supporter_status.valid_until"), @"Your current osu!supporter tag is valid until {0}!", date);

        /// <summary>
        /// "Your osu!supporter tag was valid until {0}."
        /// </summary>
        public static LocalisableString SupportSupporterStatusWasValidUntil(string date) => new TranslatableString(getKey(@"support.supporter_status.was_valid_until"), @"Your osu!supporter tag was valid until {0}.", date);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}