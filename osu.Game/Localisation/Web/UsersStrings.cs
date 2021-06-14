// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class UsersStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Users";

        /// <summary>
        /// "[deleted user]"
        /// </summary>
        public static LocalisableString Deleted => new TranslatableString(getKey(@"deleted"), @"[deleted user]");

        /// <summary>
        /// "{0}&#39;s Modding History"
        /// </summary>
        public static LocalisableString BeatmapsetActivitiesTitle(string user) => new TranslatableString(getKey(@"beatmapset_activities.title"), @"{0}'s Modding History", user);

        /// <summary>
        /// "Modding"
        /// </summary>
        public static LocalisableString BeatmapsetActivitiesTitleCompact => new TranslatableString(getKey(@"beatmapset_activities.title_compact"), @"Modding");

        /// <summary>
        /// "Recently started discussions"
        /// </summary>
        public static LocalisableString BeatmapsetActivitiesDiscussionsTitleRecent => new TranslatableString(getKey(@"beatmapset_activities.discussions.title_recent"), @"Recently started discussions");

        /// <summary>
        /// "Recent events"
        /// </summary>
        public static LocalisableString BeatmapsetActivitiesEventsTitleRecent => new TranslatableString(getKey(@"beatmapset_activities.events.title_recent"), @"Recent events");

        /// <summary>
        /// "Recent posts"
        /// </summary>
        public static LocalisableString BeatmapsetActivitiesPostsTitleRecent => new TranslatableString(getKey(@"beatmapset_activities.posts.title_recent"), @"Recent posts");

        /// <summary>
        /// "Most upvoted by (last 3 months)"
        /// </summary>
        public static LocalisableString BeatmapsetActivitiesVotesReceivedTitleMost => new TranslatableString(getKey(@"beatmapset_activities.votes_received.title_most"), @"Most upvoted by (last 3 months)");

        /// <summary>
        /// "Most upvoted (last 3 months)"
        /// </summary>
        public static LocalisableString BeatmapsetActivitiesVotesMadeTitleMost => new TranslatableString(getKey(@"beatmapset_activities.votes_made.title_most"), @"Most upvoted (last 3 months)");

        /// <summary>
        /// "You have blocked this user."
        /// </summary>
        public static LocalisableString BlocksBannerText => new TranslatableString(getKey(@"blocks.banner_text"), @"You have blocked this user.");

        /// <summary>
        /// "blocked users ({0})"
        /// </summary>
        public static LocalisableString BlocksBlockedCount(string count) => new TranslatableString(getKey(@"blocks.blocked_count"), @"blocked users ({0})", count);

        /// <summary>
        /// "Hide profile"
        /// </summary>
        public static LocalisableString BlocksHideProfile => new TranslatableString(getKey(@"blocks.hide_profile"), @"Hide profile");

        /// <summary>
        /// "That user is not blocked."
        /// </summary>
        public static LocalisableString BlocksNotBlocked => new TranslatableString(getKey(@"blocks.not_blocked"), @"That user is not blocked.");

        /// <summary>
        /// "Show profile"
        /// </summary>
        public static LocalisableString BlocksShowProfile => new TranslatableString(getKey(@"blocks.show_profile"), @"Show profile");

        /// <summary>
        /// "Block limit reached."
        /// </summary>
        public static LocalisableString BlocksTooMany => new TranslatableString(getKey(@"blocks.too_many"), @"Block limit reached.");

        /// <summary>
        /// "Block"
        /// </summary>
        public static LocalisableString BlocksButtonBlock => new TranslatableString(getKey(@"blocks.button.block"), @"Block");

        /// <summary>
        /// "Unblock"
        /// </summary>
        public static LocalisableString BlocksButtonUnblock => new TranslatableString(getKey(@"blocks.button.unblock"), @"Unblock");

        /// <summary>
        /// "Loading..."
        /// </summary>
        public static LocalisableString CardLoading => new TranslatableString(getKey(@"card.loading"), @"Loading...");

        /// <summary>
        /// "Send message"
        /// </summary>
        public static LocalisableString CardSendMessage => new TranslatableString(getKey(@"card.send_message"), @"Send message");

        /// <summary>
        /// "Uh-oh! It looks like your account has been disabled."
        /// </summary>
        public static LocalisableString DisabledTitle => new TranslatableString(getKey(@"disabled.title"), @"Uh-oh! It looks like your account has been disabled.");

        /// <summary>
        /// "In the case you have broken a rule, please note that there is generally a cool-down period of one month during which we will not consider any amnesty requests. After this period, you are free to contact us should you deem it necessary. Please note that creating new accounts after you have had one disabled will result in an &lt;strong&gt;extension of this one month cool-down&lt;/strong&gt;. Please also note that for &lt;strong&gt;every account you create, you are further breaking rules&lt;/strong&gt;. We highly suggest you don&#39;t go down this path!"
        /// </summary>
        public static LocalisableString DisabledWarning => new TranslatableString(getKey(@"disabled.warning"), @"In the case you have broken a rule, please note that there is generally a cool-down period of one month during which we will not consider any amnesty requests. After this period, you are free to contact us should you deem it necessary. Please note that creating new accounts after you have had one disabled will result in an <strong>extension of this one month cool-down</strong>. Please also note that for <strong>every account you create, you are further breaking rules</strong>. We highly suggest you don't go down this path!");

        /// <summary>
        /// "If you feel this is a mistake, you are welcome to contact us (via {0} or by clicking the &quot;?&quot; in the bottom-right-hand corner of this page). Please note that we are always fully confident with our actions, as they are based on very solid data. We reserve the right to disregard your request should we feel you are being intentionally dishonest."
        /// </summary>
        public static LocalisableString DisabledIfMistakeDefault(string email) => new TranslatableString(getKey(@"disabled.if_mistake._"), @"If you feel this is a mistake, you are welcome to contact us (via {0} or by clicking the ""?"" in the bottom-right-hand corner of this page). Please note that we are always fully confident with our actions, as they are based on very solid data. We reserve the right to disregard your request should we feel you are being intentionally dishonest.", email);

        /// <summary>
        /// "email"
        /// </summary>
        public static LocalisableString DisabledIfMistakeEmail => new TranslatableString(getKey(@"disabled.if_mistake.email"), @"email");

        /// <summary>
        /// "Your account has deemed to be compromised. It may be disabled temporarily while its identity is confirmed."
        /// </summary>
        public static LocalisableString DisabledReasonsCompromised => new TranslatableString(getKey(@"disabled.reasons.compromised"), @"Your account has deemed to be compromised. It may be disabled temporarily while its identity is confirmed.");

        /// <summary>
        /// "There are a number of reasons that can result in your account being disabled:"
        /// </summary>
        public static LocalisableString DisabledReasonsOpening => new TranslatableString(getKey(@"disabled.reasons.opening"), @"There are a number of reasons that can result in your account being disabled:");

        /// <summary>
        /// "You have broken one or more of our {0} or {1}."
        /// </summary>
        public static LocalisableString DisabledReasonsTosDefault(string communityRules, string tos) => new TranslatableString(getKey(@"disabled.reasons.tos._"), @"You have broken one or more of our {0} or {1}.", communityRules, tos);

        /// <summary>
        /// "community rules"
        /// </summary>
        public static LocalisableString DisabledReasonsTosCommunityRules => new TranslatableString(getKey(@"disabled.reasons.tos.community_rules"), @"community rules");

        /// <summary>
        /// "terms of service"
        /// </summary>
        public static LocalisableString DisabledReasonsTosTos => new TranslatableString(getKey(@"disabled.reasons.tos.tos"), @"terms of service");

        /// <summary>
        /// "Members by game mode"
        /// </summary>
        public static LocalisableString FilteringByGameMode => new TranslatableString(getKey(@"filtering.by_game_mode"), @"Members by game mode");

        /// <summary>
        /// "Your account hasn&#39;t been used in a long time."
        /// </summary>
        public static LocalisableString ForceReactivationReasonInactiveDifferentCountry => new TranslatableString(getKey(@"force_reactivation.reason.inactive_different_country"), @"Your account hasn't been used in a long time.");

        /// <summary>
        /// "Sign in"
        /// </summary>
        public static LocalisableString LoginDefault => new TranslatableString(getKey(@"login._"), @"Sign in");

        /// <summary>
        /// "Sign in"
        /// </summary>
        public static LocalisableString LoginButton => new TranslatableString(getKey(@"login.button"), @"Sign in");

        /// <summary>
        /// "Signing in..."
        /// </summary>
        public static LocalisableString LoginButtonPosting => new TranslatableString(getKey(@"login.button_posting"), @"Signing in...");

        /// <summary>
        /// "Signing in with email is currently disabled. Please use username instead."
        /// </summary>
        public static LocalisableString LoginEmailLoginDisabled => new TranslatableString(getKey(@"login.email_login_disabled"), @"Signing in with email is currently disabled. Please use username instead.");

        /// <summary>
        /// "Incorrect sign in"
        /// </summary>
        public static LocalisableString LoginFailed => new TranslatableString(getKey(@"login.failed"), @"Incorrect sign in");

        /// <summary>
        /// "Forgotten your password?"
        /// </summary>
        public static LocalisableString LoginForgot => new TranslatableString(getKey(@"login.forgot"), @"Forgotten your password?");

        /// <summary>
        /// "Please sign in to continue"
        /// </summary>
        public static LocalisableString LoginInfo => new TranslatableString(getKey(@"login.info"), @"Please sign in to continue");

        /// <summary>
        /// "Too many failed login attempts, please complete the captcha and try again. (Refresh page if captcha is not visible)"
        /// </summary>
        public static LocalisableString LoginInvalidCaptcha => new TranslatableString(getKey(@"login.invalid_captcha"), @"Too many failed login attempts, please complete the captcha and try again. (Refresh page if captcha is not visible)");

        /// <summary>
        /// "your IP address is locked. Please wait a few minutes."
        /// </summary>
        public static LocalisableString LoginLockedIp => new TranslatableString(getKey(@"login.locked_ip"), @"your IP address is locked. Please wait a few minutes.");

        /// <summary>
        /// "Password"
        /// </summary>
        public static LocalisableString LoginPassword => new TranslatableString(getKey(@"login.password"), @"Password");

        /// <summary>
        /// "Don&#39;t have an osu! account? Make a new one"
        /// </summary>
        public static LocalisableString LoginRegister => new TranslatableString(getKey(@"login.register"), @"Don't have an osu! account? Make a new one");

        /// <summary>
        /// "Remember this computer"
        /// </summary>
        public static LocalisableString LoginRemember => new TranslatableString(getKey(@"login.remember"), @"Remember this computer");

        /// <summary>
        /// "Please sign in to proceed"
        /// </summary>
        public static LocalisableString LoginTitle => new TranslatableString(getKey(@"login.title"), @"Please sign in to proceed");

        /// <summary>
        /// "Username"
        /// </summary>
        public static LocalisableString LoginUsername => new TranslatableString(getKey(@"login.username"), @"Username");

        /// <summary>
        /// "Beta access is currently restricted to privileged users."
        /// </summary>
        public static LocalisableString LoginBetaMain => new TranslatableString(getKey(@"login.beta.main"), @"Beta access is currently restricted to privileged users.");

        /// <summary>
        /// "(osu!supporters will get in soon)"
        /// </summary>
        public static LocalisableString LoginBetaSmall => new TranslatableString(getKey(@"login.beta.small"), @"(osu!supporters will get in soon)");

        /// <summary>
        /// "{0}&#39;s posts"
        /// </summary>
        public static LocalisableString PostsTitle(string username) => new TranslatableString(getKey(@"posts.title"), @"{0}'s posts", username);

        /// <summary>
        /// "click to sign in"
        /// </summary>
        public static LocalisableString AnonymousLoginLink => new TranslatableString(getKey(@"anonymous.login_link"), @"click to sign in");

        /// <summary>
        /// "sign in"
        /// </summary>
        public static LocalisableString AnonymousLoginText => new TranslatableString(getKey(@"anonymous.login_text"), @"sign in");

        /// <summary>
        /// "Guest"
        /// </summary>
        public static LocalisableString AnonymousUsername => new TranslatableString(getKey(@"anonymous.username"), @"Guest");

        /// <summary>
        /// "You need to be signed in to do this."
        /// </summary>
        public static LocalisableString AnonymousError => new TranslatableString(getKey(@"anonymous.error"), @"You need to be signed in to do this.");

        /// <summary>
        /// "Are you sure you want to sign out? :("
        /// </summary>
        public static LocalisableString LogoutConfirm => new TranslatableString(getKey(@"logout_confirm"), @"Are you sure you want to sign out? :(");

        /// <summary>
        /// "Report"
        /// </summary>
        public static LocalisableString ReportButtonText => new TranslatableString(getKey(@"report.button_text"), @"Report");

        /// <summary>
        /// "Additional Comments"
        /// </summary>
        public static LocalisableString ReportComments => new TranslatableString(getKey(@"report.comments"), @"Additional Comments");

        /// <summary>
        /// "Please provide any information you believe could be useful."
        /// </summary>
        public static LocalisableString ReportPlaceholder => new TranslatableString(getKey(@"report.placeholder"), @"Please provide any information you believe could be useful.");

        /// <summary>
        /// "Reason"
        /// </summary>
        public static LocalisableString ReportReason => new TranslatableString(getKey(@"report.reason"), @"Reason");

        /// <summary>
        /// "Thanks for your report!"
        /// </summary>
        public static LocalisableString ReportThanks => new TranslatableString(getKey(@"report.thanks"), @"Thanks for your report!");

        /// <summary>
        /// "Report {0}?"
        /// </summary>
        public static LocalisableString ReportTitle(string username) => new TranslatableString(getKey(@"report.title"), @"Report {0}?", username);

        /// <summary>
        /// "Send Report"
        /// </summary>
        public static LocalisableString ReportActionsSend => new TranslatableString(getKey(@"report.actions.send"), @"Send Report");

        /// <summary>
        /// "Cancel"
        /// </summary>
        public static LocalisableString ReportActionsCancel => new TranslatableString(getKey(@"report.actions.cancel"), @"Cancel");

        /// <summary>
        /// "Foul play / Cheating"
        /// </summary>
        public static LocalisableString ReportOptionsCheating => new TranslatableString(getKey(@"report.options.cheating"), @"Foul play / Cheating");

        /// <summary>
        /// "Insulting me / others"
        /// </summary>
        public static LocalisableString ReportOptionsInsults => new TranslatableString(getKey(@"report.options.insults"), @"Insulting me / others");

        /// <summary>
        /// "Spamming"
        /// </summary>
        public static LocalisableString ReportOptionsSpam => new TranslatableString(getKey(@"report.options.spam"), @"Spamming");

        /// <summary>
        /// "Linking inappropriate content"
        /// </summary>
        public static LocalisableString ReportOptionsUnwantedContent => new TranslatableString(getKey(@"report.options.unwanted_content"), @"Linking inappropriate content");

        /// <summary>
        /// "Nonsense"
        /// </summary>
        public static LocalisableString ReportOptionsNonsense => new TranslatableString(getKey(@"report.options.nonsense"), @"Nonsense");

        /// <summary>
        /// "Other (type below)"
        /// </summary>
        public static LocalisableString ReportOptionsOther => new TranslatableString(getKey(@"report.options.other"), @"Other (type below)");

        /// <summary>
        /// "Your account has been restricted!"
        /// </summary>
        public static LocalisableString RestrictedBannerTitle => new TranslatableString(getKey(@"restricted_banner.title"), @"Your account has been restricted!");

        /// <summary>
        /// "While restricted, you will be unable to interact with other players and your scores will only be visible to you. This is usually the result of an automated process and will usually be lifted within 24 hours. If you wish to appeal your restriction, please &lt;a href=&quot;mailto{0}@ppy.sh&quot;&gt;contact support&lt;/a&gt;."
        /// </summary>
        public static LocalisableString RestrictedBannerMessage(string accounts) => new TranslatableString(getKey(@"restricted_banner.message"), @"While restricted, you will be unable to interact with other players and your scores will only be visible to you. This is usually the result of an automated process and will usually be lifted within 24 hours. If you wish to appeal your restriction, please <a href=""mailto{0}@ppy.sh"">contact support</a>.", accounts);

        /// <summary>
        /// "{0} years old"
        /// </summary>
        public static LocalisableString ShowAge(string age) => new TranslatableString(getKey(@"show.age"), @"{0} years old", age);

        /// <summary>
        /// "change your avatar!"
        /// </summary>
        public static LocalisableString ShowChangeAvatar => new TranslatableString(getKey(@"show.change_avatar"), @"change your avatar!");

        /// <summary>
        /// "Here since the beginning"
        /// </summary>
        public static LocalisableString ShowFirstMembers => new TranslatableString(getKey(@"show.first_members"), @"Here since the beginning");

        /// <summary>
        /// "osu!developer"
        /// </summary>
        public static LocalisableString ShowIsDeveloper => new TranslatableString(getKey(@"show.is_developer"), @"osu!developer");

        /// <summary>
        /// "osu!supporter"
        /// </summary>
        public static LocalisableString ShowIsSupporter => new TranslatableString(getKey(@"show.is_supporter"), @"osu!supporter");

        /// <summary>
        /// "Joined {0}"
        /// </summary>
        public static LocalisableString ShowJoinedAt(string date) => new TranslatableString(getKey(@"show.joined_at"), @"Joined {0}", date);

        /// <summary>
        /// "Last seen {0}"
        /// </summary>
        public static LocalisableString ShowLastvisit(string date) => new TranslatableString(getKey(@"show.lastvisit"), @"Last seen {0}", date);

        /// <summary>
        /// "Currently online"
        /// </summary>
        public static LocalisableString ShowLastvisitOnline => new TranslatableString(getKey(@"show.lastvisit_online"), @"Currently online");

        /// <summary>
        /// "You might have made a typo! (or the user may have been banned)"
        /// </summary>
        public static LocalisableString ShowMissingtext => new TranslatableString(getKey(@"show.missingtext"), @"You might have made a typo! (or the user may have been banned)");

        /// <summary>
        /// "From {0}"
        /// </summary>
        public static LocalisableString ShowOriginCountry(string country) => new TranslatableString(getKey(@"show.origin_country"), @"From {0}", country);

        /// <summary>
        /// "formerly known as"
        /// </summary>
        public static LocalisableString ShowPreviousUsernames => new TranslatableString(getKey(@"show.previous_usernames"), @"formerly known as");

        /// <summary>
        /// "Plays with {0}"
        /// </summary>
        public static LocalisableString ShowPlaysWith(string devices) => new TranslatableString(getKey(@"show.plays_with"), @"Plays with {0}", devices);

        /// <summary>
        /// "{0}&#39;s profile"
        /// </summary>
        public static LocalisableString ShowTitle(string username) => new TranslatableString(getKey(@"show.title"), @"{0}'s profile", username);

        /// <summary>
        /// "Posted {0}"
        /// </summary>
        public static LocalisableString ShowCommentsCountDefault(string link) => new TranslatableString(getKey(@"show.comments_count._"), @"Posted {0}", link);

        /// <summary>
        /// "{0} comment|{0} comments"
        /// </summary>
        public static LocalisableString ShowCommentsCountCount(string countDelimited) => new TranslatableString(getKey(@"show.comments_count.count"), @"{0} comment|{0} comments", countDelimited);

        /// <summary>
        /// "Change Profile Cover"
        /// </summary>
        public static LocalisableString ShowEditCoverButton => new TranslatableString(getKey(@"show.edit.cover.button"), @"Change Profile Cover");

        /// <summary>
        /// "More cover options will be available in the future"
        /// </summary>
        public static LocalisableString ShowEditCoverDefaultsInfo => new TranslatableString(getKey(@"show.edit.cover.defaults_info"), @"More cover options will be available in the future");

        /// <summary>
        /// "Failed processing image. Verify uploaded image and try again."
        /// </summary>
        public static LocalisableString ShowEditCoverUploadBrokenFile => new TranslatableString(getKey(@"show.edit.cover.upload.broken_file"), @"Failed processing image. Verify uploaded image and try again.");

        /// <summary>
        /// "Upload image"
        /// </summary>
        public static LocalisableString ShowEditCoverUploadButton => new TranslatableString(getKey(@"show.edit.cover.upload.button"), @"Upload image");

        /// <summary>
        /// "Drop here to upload"
        /// </summary>
        public static LocalisableString ShowEditCoverUploadDropzone => new TranslatableString(getKey(@"show.edit.cover.upload.dropzone"), @"Drop here to upload");

        /// <summary>
        /// "You can also drop your image here to upload"
        /// </summary>
        public static LocalisableString ShowEditCoverUploadDropzoneInfo => new TranslatableString(getKey(@"show.edit.cover.upload.dropzone_info"), @"You can also drop your image here to upload");

        /// <summary>
        /// "Cover size should be 2400x640"
        /// </summary>
        public static LocalisableString ShowEditCoverUploadSizeInfo => new TranslatableString(getKey(@"show.edit.cover.upload.size_info"), @"Cover size should be 2400x640");

        /// <summary>
        /// "Uploaded file is too large."
        /// </summary>
        public static LocalisableString ShowEditCoverUploadTooLarge => new TranslatableString(getKey(@"show.edit.cover.upload.too_large"), @"Uploaded file is too large.");

        /// <summary>
        /// "Unsupported format."
        /// </summary>
        public static LocalisableString ShowEditCoverUploadUnsupportedFormat => new TranslatableString(getKey(@"show.edit.cover.upload.unsupported_format"), @"Unsupported format.");

        /// <summary>
        /// "Upload available for {0} only"
        /// </summary>
        public static LocalisableString ShowEditCoverUploadRestrictionInfoDefault(string link) => new TranslatableString(getKey(@"show.edit.cover.upload.restriction_info._"), @"Upload available for {0} only", link);

        /// <summary>
        /// "osu!supporters"
        /// </summary>
        public static LocalisableString ShowEditCoverUploadRestrictionInfoLink => new TranslatableString(getKey(@"show.edit.cover.upload.restriction_info.link"), @"osu!supporters");

        /// <summary>
        /// "default game mode"
        /// </summary>
        public static LocalisableString ShowEditDefaultPlaymodeIsDefaultTooltip => new TranslatableString(getKey(@"show.edit.default_playmode.is_default_tooltip"), @"default game mode");

        /// <summary>
        /// "set {0} as profile default game mode"
        /// </summary>
        public static LocalisableString ShowEditDefaultPlaymodeSet(string mode) => new TranslatableString(getKey(@"show.edit.default_playmode.set"), @"set {0} as profile default game mode", mode);

        /// <summary>
        /// "none"
        /// </summary>
        public static LocalisableString ShowExtraNone => new TranslatableString(getKey(@"show.extra.none"), @"none");

        /// <summary>
        /// "No recent plays"
        /// </summary>
        public static LocalisableString ShowExtraUnranked => new TranslatableString(getKey(@"show.extra.unranked"), @"No recent plays");

        /// <summary>
        /// "Achieved on {0}"
        /// </summary>
        public static LocalisableString ShowExtraAchievementsAchievedOn(string date) => new TranslatableString(getKey(@"show.extra.achievements.achieved-on"), @"Achieved on {0}", date);

        /// <summary>
        /// "Locked"
        /// </summary>
        public static LocalisableString ShowExtraAchievementsLocked => new TranslatableString(getKey(@"show.extra.achievements.locked"), @"Locked");

        /// <summary>
        /// "Achievements"
        /// </summary>
        public static LocalisableString ShowExtraAchievementsTitle => new TranslatableString(getKey(@"show.extra.achievements.title"), @"Achievements");

        /// <summary>
        /// "by {0}"
        /// </summary>
        public static LocalisableString ShowExtraBeatmapsByArtist(string artist) => new TranslatableString(getKey(@"show.extra.beatmaps.by_artist"), @"by {0}", artist);

        /// <summary>
        /// "Beatmaps"
        /// </summary>
        public static LocalisableString ShowExtraBeatmapsTitle => new TranslatableString(getKey(@"show.extra.beatmaps.title"), @"Beatmaps");

        /// <summary>
        /// "Favourite Beatmaps"
        /// </summary>
        public static LocalisableString ShowExtraBeatmapsFavouriteTitle => new TranslatableString(getKey(@"show.extra.beatmaps.favourite.title"), @"Favourite Beatmaps");

        /// <summary>
        /// "Graveyarded Beatmaps"
        /// </summary>
        public static LocalisableString ShowExtraBeatmapsGraveyardTitle => new TranslatableString(getKey(@"show.extra.beatmaps.graveyard.title"), @"Graveyarded Beatmaps");

        /// <summary>
        /// "Loved Beatmaps"
        /// </summary>
        public static LocalisableString ShowExtraBeatmapsLovedTitle => new TranslatableString(getKey(@"show.extra.beatmaps.loved.title"), @"Loved Beatmaps");

        /// <summary>
        /// "Pending Beatmaps"
        /// </summary>
        public static LocalisableString ShowExtraBeatmapsPendingTitle => new TranslatableString(getKey(@"show.extra.beatmaps.pending.title"), @"Pending Beatmaps");

        /// <summary>
        /// "Ranked Beatmaps"
        /// </summary>
        public static LocalisableString ShowExtraBeatmapsRankedTitle => new TranslatableString(getKey(@"show.extra.beatmaps.ranked.title"), @"Ranked Beatmaps");

        /// <summary>
        /// "Discussions"
        /// </summary>
        public static LocalisableString ShowExtraDiscussionsTitle => new TranslatableString(getKey(@"show.extra.discussions.title"), @"Discussions");

        /// <summary>
        /// "Recent Discussions"
        /// </summary>
        public static LocalisableString ShowExtraDiscussionsTitleLonger => new TranslatableString(getKey(@"show.extra.discussions.title_longer"), @"Recent Discussions");

        /// <summary>
        /// "see more discussions"
        /// </summary>
        public static LocalisableString ShowExtraDiscussionsShowMore => new TranslatableString(getKey(@"show.extra.discussions.show_more"), @"see more discussions");

        /// <summary>
        /// "Events"
        /// </summary>
        public static LocalisableString ShowExtraEventsTitle => new TranslatableString(getKey(@"show.extra.events.title"), @"Events");

        /// <summary>
        /// "Recent Events"
        /// </summary>
        public static LocalisableString ShowExtraEventsTitleLonger => new TranslatableString(getKey(@"show.extra.events.title_longer"), @"Recent Events");

        /// <summary>
        /// "see more events"
        /// </summary>
        public static LocalisableString ShowExtraEventsShowMore => new TranslatableString(getKey(@"show.extra.events.show_more"), @"see more events");

        /// <summary>
        /// "Historical"
        /// </summary>
        public static LocalisableString ShowExtraHistoricalTitle => new TranslatableString(getKey(@"show.extra.historical.title"), @"Historical");

        /// <summary>
        /// "Play History"
        /// </summary>
        public static LocalisableString ShowExtraHistoricalMonthlyPlaycountsTitle => new TranslatableString(getKey(@"show.extra.historical.monthly_playcounts.title"), @"Play History");

        /// <summary>
        /// "Plays"
        /// </summary>
        public static LocalisableString ShowExtraHistoricalMonthlyPlaycountsCountLabel => new TranslatableString(getKey(@"show.extra.historical.monthly_playcounts.count_label"), @"Plays");

        /// <summary>
        /// "times played"
        /// </summary>
        public static LocalisableString ShowExtraHistoricalMostPlayedCount => new TranslatableString(getKey(@"show.extra.historical.most_played.count"), @"times played");

        /// <summary>
        /// "Most Played Beatmaps"
        /// </summary>
        public static LocalisableString ShowExtraHistoricalMostPlayedTitle => new TranslatableString(getKey(@"show.extra.historical.most_played.title"), @"Most Played Beatmaps");

        /// <summary>
        /// "accuracy: {0}"
        /// </summary>
        public static LocalisableString ShowExtraHistoricalRecentPlaysAccuracy(string percentage) => new TranslatableString(getKey(@"show.extra.historical.recent_plays.accuracy"), @"accuracy: {0}", percentage);

        /// <summary>
        /// "Recent Plays (24h)"
        /// </summary>
        public static LocalisableString ShowExtraHistoricalRecentPlaysTitle => new TranslatableString(getKey(@"show.extra.historical.recent_plays.title"), @"Recent Plays (24h)");

        /// <summary>
        /// "Replays Watched History"
        /// </summary>
        public static LocalisableString ShowExtraHistoricalReplaysWatchedCountsTitle => new TranslatableString(getKey(@"show.extra.historical.replays_watched_counts.title"), @"Replays Watched History");

        /// <summary>
        /// "Replays Watched"
        /// </summary>
        public static LocalisableString ShowExtraHistoricalReplaysWatchedCountsCountLabel => new TranslatableString(getKey(@"show.extra.historical.replays_watched_counts.count_label"), @"Replays Watched");

        /// <summary>
        /// "Recent Kudosu History"
        /// </summary>
        public static LocalisableString ShowExtraKudosuRecentEntries => new TranslatableString(getKey(@"show.extra.kudosu.recent_entries"), @"Recent Kudosu History");

        /// <summary>
        /// "Kudosu!"
        /// </summary>
        public static LocalisableString ShowExtraKudosuTitle => new TranslatableString(getKey(@"show.extra.kudosu.title"), @"Kudosu!");

        /// <summary>
        /// "Total Kudosu Earned"
        /// </summary>
        public static LocalisableString ShowExtraKudosuTotal => new TranslatableString(getKey(@"show.extra.kudosu.total"), @"Total Kudosu Earned");

        /// <summary>
        /// "{0} kudosu"
        /// </summary>
        public static LocalisableString ShowExtraKudosuEntryAmount(string amount) => new TranslatableString(getKey(@"show.extra.kudosu.entry.amount"), @"{0} kudosu", amount);

        /// <summary>
        /// "This user hasn&#39;t received any kudosu!"
        /// </summary>
        public static LocalisableString ShowExtraKudosuEntryEmpty => new TranslatableString(getKey(@"show.extra.kudosu.entry.empty"), @"This user hasn't received any kudosu!");

        /// <summary>
        /// "Received {0} from kudosu deny repeal of modding post {1}"
        /// </summary>
        public static LocalisableString ShowExtraKudosuEntryBeatmapDiscussionAllowKudosuGive(string amount, string post) => new TranslatableString(getKey(@"show.extra.kudosu.entry.beatmap_discussion.allow_kudosu.give"), @"Received {0} from kudosu deny repeal of modding post {1}", amount, post);

        /// <summary>
        /// "Denied {0} from modding post {1}"
        /// </summary>
        public static LocalisableString ShowExtraKudosuEntryBeatmapDiscussionDenyKudosuReset(string amount, string post) => new TranslatableString(getKey(@"show.extra.kudosu.entry.beatmap_discussion.deny_kudosu.reset"), @"Denied {0} from modding post {1}", amount, post);

        /// <summary>
        /// "Lost {0} from modding post deletion of {1}"
        /// </summary>
        public static LocalisableString ShowExtraKudosuEntryBeatmapDiscussionDeleteReset(string amount, string post) => new TranslatableString(getKey(@"show.extra.kudosu.entry.beatmap_discussion.delete.reset"), @"Lost {0} from modding post deletion of {1}", amount, post);

        /// <summary>
        /// "Received {0} from modding post restoration of {1}"
        /// </summary>
        public static LocalisableString ShowExtraKudosuEntryBeatmapDiscussionRestoreGive(string amount, string post) => new TranslatableString(getKey(@"show.extra.kudosu.entry.beatmap_discussion.restore.give"), @"Received {0} from modding post restoration of {1}", amount, post);

        /// <summary>
        /// "Received {0} from obtaining votes in modding post of {1}"
        /// </summary>
        public static LocalisableString ShowExtraKudosuEntryBeatmapDiscussionVoteGive(string amount, string post) => new TranslatableString(getKey(@"show.extra.kudosu.entry.beatmap_discussion.vote.give"), @"Received {0} from obtaining votes in modding post of {1}", amount, post);

        /// <summary>
        /// "Lost {0} from losing votes in modding post of {1}"
        /// </summary>
        public static LocalisableString ShowExtraKudosuEntryBeatmapDiscussionVoteReset(string amount, string post) => new TranslatableString(getKey(@"show.extra.kudosu.entry.beatmap_discussion.vote.reset"), @"Lost {0} from losing votes in modding post of {1}", amount, post);

        /// <summary>
        /// "Received {0} from votes recalculation in modding post of {1}"
        /// </summary>
        public static LocalisableString ShowExtraKudosuEntryBeatmapDiscussionRecalculateGive(string amount, string post) => new TranslatableString(getKey(@"show.extra.kudosu.entry.beatmap_discussion.recalculate.give"), @"Received {0} from votes recalculation in modding post of {1}", amount, post);

        /// <summary>
        /// "Lost {0} from votes recalculation in modding post of {1}"
        /// </summary>
        public static LocalisableString ShowExtraKudosuEntryBeatmapDiscussionRecalculateReset(string amount, string post) => new TranslatableString(getKey(@"show.extra.kudosu.entry.beatmap_discussion.recalculate.reset"), @"Lost {0} from votes recalculation in modding post of {1}", amount, post);

        /// <summary>
        /// "Received {0} from {1} for a post at {2}"
        /// </summary>
        public static LocalisableString ShowExtraKudosuEntryForumPostGive(string amount, string giver, string post) => new TranslatableString(getKey(@"show.extra.kudosu.entry.forum_post.give"), @"Received {0} from {1} for a post at {2}", amount, giver, post);

        /// <summary>
        /// "Kudosu reset by {0} for the post {1}"
        /// </summary>
        public static LocalisableString ShowExtraKudosuEntryForumPostReset(string giver, string post) => new TranslatableString(getKey(@"show.extra.kudosu.entry.forum_post.reset"), @"Kudosu reset by {0} for the post {1}", giver, post);

        /// <summary>
        /// "Denied kudosu by {0} for the post {1}"
        /// </summary>
        public static LocalisableString ShowExtraKudosuEntryForumPostRevoke(string giver, string post) => new TranslatableString(getKey(@"show.extra.kudosu.entry.forum_post.revoke"), @"Denied kudosu by {0} for the post {1}", giver, post);

        /// <summary>
        /// "Based on how much of a contribution the user has made to beatmap moderation. See {0} for more information."
        /// </summary>
        public static LocalisableString ShowExtraKudosuTotalInfoDefault(string link) => new TranslatableString(getKey(@"show.extra.kudosu.total_info._"), @"Based on how much of a contribution the user has made to beatmap moderation. See {0} for more information.", link);

        /// <summary>
        /// "this page"
        /// </summary>
        public static LocalisableString ShowExtraKudosuTotalInfoLink => new TranslatableString(getKey(@"show.extra.kudosu.total_info.link"), @"this page");

        /// <summary>
        /// "me!"
        /// </summary>
        public static LocalisableString ShowExtraMeTitle => new TranslatableString(getKey(@"show.extra.me.title"), @"me!");

        /// <summary>
        /// "This user hasn&#39;t gotten any yet. ;_;"
        /// </summary>
        public static LocalisableString ShowExtraMedalsEmpty => new TranslatableString(getKey(@"show.extra.medals.empty"), @"This user hasn't gotten any yet. ;_;");

        /// <summary>
        /// "Latest"
        /// </summary>
        public static LocalisableString ShowExtraMedalsRecent => new TranslatableString(getKey(@"show.extra.medals.recent"), @"Latest");

        /// <summary>
        /// "Medals"
        /// </summary>
        public static LocalisableString ShowExtraMedalsTitle => new TranslatableString(getKey(@"show.extra.medals.title"), @"Medals");

        /// <summary>
        /// "Posts"
        /// </summary>
        public static LocalisableString ShowExtraPostsTitle => new TranslatableString(getKey(@"show.extra.posts.title"), @"Posts");

        /// <summary>
        /// "Recent Posts"
        /// </summary>
        public static LocalisableString ShowExtraPostsTitleLonger => new TranslatableString(getKey(@"show.extra.posts.title_longer"), @"Recent Posts");

        /// <summary>
        /// "see more posts"
        /// </summary>
        public static LocalisableString ShowExtraPostsShowMore => new TranslatableString(getKey(@"show.extra.posts.show_more"), @"see more posts");

        /// <summary>
        /// "Recent"
        /// </summary>
        public static LocalisableString ShowExtraRecentActivityTitle => new TranslatableString(getKey(@"show.extra.recent_activity.title"), @"Recent");

        /// <summary>
        /// "Download Replay"
        /// </summary>
        public static LocalisableString ShowExtraTopRanksDownloadReplay => new TranslatableString(getKey(@"show.extra.top_ranks.download_replay"), @"Download Replay");

        /// <summary>
        /// "Only ranked beatmaps award pp"
        /// </summary>
        public static LocalisableString ShowExtraTopRanksNotRanked => new TranslatableString(getKey(@"show.extra.top_ranks.not_ranked"), @"Only ranked beatmaps award pp");

        /// <summary>
        /// "weighted {0}"
        /// </summary>
        public static LocalisableString ShowExtraTopRanksPpWeight(string percentage) => new TranslatableString(getKey(@"show.extra.top_ranks.pp_weight"), @"weighted {0}", percentage);

        /// <summary>
        /// "View Details"
        /// </summary>
        public static LocalisableString ShowExtraTopRanksViewDetails => new TranslatableString(getKey(@"show.extra.top_ranks.view_details"), @"View Details");

        /// <summary>
        /// "Ranks"
        /// </summary>
        public static LocalisableString ShowExtraTopRanksTitle => new TranslatableString(getKey(@"show.extra.top_ranks.title"), @"Ranks");

        /// <summary>
        /// "Best Performance"
        /// </summary>
        public static LocalisableString ShowExtraTopRanksBestTitle => new TranslatableString(getKey(@"show.extra.top_ranks.best.title"), @"Best Performance");

        /// <summary>
        /// "First Place Ranks"
        /// </summary>
        public static LocalisableString ShowExtraTopRanksFirstTitle => new TranslatableString(getKey(@"show.extra.top_ranks.first.title"), @"First Place Ranks");

        /// <summary>
        /// "Votes Given (last 3 months)"
        /// </summary>
        public static LocalisableString ShowExtraVotesGiven => new TranslatableString(getKey(@"show.extra.votes.given"), @"Votes Given (last 3 months)");

        /// <summary>
        /// "Votes Received (last 3 months)"
        /// </summary>
        public static LocalisableString ShowExtraVotesReceived => new TranslatableString(getKey(@"show.extra.votes.received"), @"Votes Received (last 3 months)");

        /// <summary>
        /// "Votes"
        /// </summary>
        public static LocalisableString ShowExtraVotesTitle => new TranslatableString(getKey(@"show.extra.votes.title"), @"Votes");

        /// <summary>
        /// "Recent Votes"
        /// </summary>
        public static LocalisableString ShowExtraVotesTitleLonger => new TranslatableString(getKey(@"show.extra.votes.title_longer"), @"Recent Votes");

        /// <summary>
        /// "{0} vote|{0} votes"
        /// </summary>
        public static LocalisableString ShowExtraVotesVoteCount(string countDelimited) => new TranslatableString(getKey(@"show.extra.votes.vote_count"), @"{0} vote|{0} votes", countDelimited);

        /// <summary>
        /// "Account Standing"
        /// </summary>
        public static LocalisableString ShowExtraAccountStandingTitle => new TranslatableString(getKey(@"show.extra.account_standing.title"), @"Account Standing");

        /// <summary>
        /// "&lt;strong&gt;{0}&#39;s&lt;/strong&gt; account is not in a good standing :("
        /// </summary>
        public static LocalisableString ShowExtraAccountStandingBadStanding(string username) => new TranslatableString(getKey(@"show.extra.account_standing.bad_standing"), @"<strong>{0}'s</strong> account is not in a good standing :(", username);

        /// <summary>
        /// "&lt;strong&gt;{0}&lt;/strong&gt; will be able to speak again {1}."
        /// </summary>
        public static LocalisableString ShowExtraAccountStandingRemainingSilence(string username, string duration) => new TranslatableString(getKey(@"show.extra.account_standing.remaining_silence"), @"<strong>{0}</strong> will be able to speak again {1}.", username, duration);

        /// <summary>
        /// "Recent Infringements"
        /// </summary>
        public static LocalisableString ShowExtraAccountStandingRecentInfringementsTitle => new TranslatableString(getKey(@"show.extra.account_standing.recent_infringements.title"), @"Recent Infringements");

        /// <summary>
        /// "date"
        /// </summary>
        public static LocalisableString ShowExtraAccountStandingRecentInfringementsDate => new TranslatableString(getKey(@"show.extra.account_standing.recent_infringements.date"), @"date");

        /// <summary>
        /// "action"
        /// </summary>
        public static LocalisableString ShowExtraAccountStandingRecentInfringementsAction => new TranslatableString(getKey(@"show.extra.account_standing.recent_infringements.action"), @"action");

        /// <summary>
        /// "length"
        /// </summary>
        public static LocalisableString ShowExtraAccountStandingRecentInfringementsLength => new TranslatableString(getKey(@"show.extra.account_standing.recent_infringements.length"), @"length");

        /// <summary>
        /// "Permanent"
        /// </summary>
        public static LocalisableString ShowExtraAccountStandingRecentInfringementsLengthPermanent => new TranslatableString(getKey(@"show.extra.account_standing.recent_infringements.length_permanent"), @"Permanent");

        /// <summary>
        /// "description"
        /// </summary>
        public static LocalisableString ShowExtraAccountStandingRecentInfringementsDescription => new TranslatableString(getKey(@"show.extra.account_standing.recent_infringements.description"), @"description");

        /// <summary>
        /// "by {0}"
        /// </summary>
        public static LocalisableString ShowExtraAccountStandingRecentInfringementsActor(string username) => new TranslatableString(getKey(@"show.extra.account_standing.recent_infringements.actor"), @"by {0}", username);

        /// <summary>
        /// "Ban"
        /// </summary>
        public static LocalisableString ShowExtraAccountStandingRecentInfringementsActionsRestriction => new TranslatableString(getKey(@"show.extra.account_standing.recent_infringements.actions.restriction"), @"Ban");

        /// <summary>
        /// "Silence"
        /// </summary>
        public static LocalisableString ShowExtraAccountStandingRecentInfringementsActionsSilence => new TranslatableString(getKey(@"show.extra.account_standing.recent_infringements.actions.silence"), @"Silence");

        /// <summary>
        /// "Note"
        /// </summary>
        public static LocalisableString ShowExtraAccountStandingRecentInfringementsActionsNote => new TranslatableString(getKey(@"show.extra.account_standing.recent_infringements.actions.note"), @"Note");

        /// <summary>
        /// "Discord"
        /// </summary>
        public static LocalisableString ShowInfoDiscord => new TranslatableString(getKey(@"show.info.discord"), @"Discord");

        /// <summary>
        /// "Interests"
        /// </summary>
        public static LocalisableString ShowInfoInterests => new TranslatableString(getKey(@"show.info.interests"), @"Interests");

        /// <summary>
        /// "Current Location"
        /// </summary>
        public static LocalisableString ShowInfoLocation => new TranslatableString(getKey(@"show.info.location"), @"Current Location");

        /// <summary>
        /// "Occupation"
        /// </summary>
        public static LocalisableString ShowInfoOccupation => new TranslatableString(getKey(@"show.info.occupation"), @"Occupation");

        /// <summary>
        /// "Twitter"
        /// </summary>
        public static LocalisableString ShowInfoTwitter => new TranslatableString(getKey(@"show.info.twitter"), @"Twitter");

        /// <summary>
        /// "Website"
        /// </summary>
        public static LocalisableString ShowInfoWebsite => new TranslatableString(getKey(@"show.info.website"), @"Website");

        /// <summary>
        /// "They may have changed their username."
        /// </summary>
        public static LocalisableString ShowNotFoundReason1 => new TranslatableString(getKey(@"show.not_found.reason_1"), @"They may have changed their username.");

        /// <summary>
        /// "The account may be temporarily unavailable due to security or abuse issues."
        /// </summary>
        public static LocalisableString ShowNotFoundReason2 => new TranslatableString(getKey(@"show.not_found.reason_2"), @"The account may be temporarily unavailable due to security or abuse issues.");

        /// <summary>
        /// "You may have made a typo!"
        /// </summary>
        public static LocalisableString ShowNotFoundReason3 => new TranslatableString(getKey(@"show.not_found.reason_3"), @"You may have made a typo!");

        /// <summary>
        /// "There are a few possible reasons for this:"
        /// </summary>
        public static LocalisableString ShowNotFoundReasonHeader => new TranslatableString(getKey(@"show.not_found.reason_header"), @"There are a few possible reasons for this:");

        /// <summary>
        /// "User not found! ;_;"
        /// </summary>
        public static LocalisableString ShowNotFoundTitle => new TranslatableString(getKey(@"show.not_found.title"), @"User not found! ;_;");

        /// <summary>
        /// "Edit profile page"
        /// </summary>
        public static LocalisableString ShowPageButton => new TranslatableString(getKey(@"show.page.button"), @"Edit profile page");

        /// <summary>
        /// "&lt;strong&gt;me!&lt;/strong&gt; is a personal customisable area in your profile page."
        /// </summary>
        public static LocalisableString ShowPageDescription => new TranslatableString(getKey(@"show.page.description"), @"<strong>me!</strong> is a personal customisable area in your profile page.");

        /// <summary>
        /// "Edit me!"
        /// </summary>
        public static LocalisableString ShowPageEditBig => new TranslatableString(getKey(@"show.page.edit_big"), @"Edit me!");

        /// <summary>
        /// "Type page content here"
        /// </summary>
        public static LocalisableString ShowPagePlaceholder => new TranslatableString(getKey(@"show.page.placeholder"), @"Type page content here");

        /// <summary>
        /// "You need to be an {0} to unlock this feature."
        /// </summary>
        public static LocalisableString ShowPageRestrictionInfoDefault(string link) => new TranslatableString(getKey(@"show.page.restriction_info._"), @"You need to be an {0} to unlock this feature.", link);

        /// <summary>
        /// "osu!supporter"
        /// </summary>
        public static LocalisableString ShowPageRestrictionInfoLink => new TranslatableString(getKey(@"show.page.restriction_info.link"), @"osu!supporter");

        /// <summary>
        /// "Contributed {0}"
        /// </summary>
        public static LocalisableString ShowPostCountDefault(string link) => new TranslatableString(getKey(@"show.post_count._"), @"Contributed {0}", link);

        /// <summary>
        /// "{0} forum post|{0} forum posts"
        /// </summary>
        public static LocalisableString ShowPostCountCount(string countDelimited) => new TranslatableString(getKey(@"show.post_count.count"), @"{0} forum post|{0} forum posts", countDelimited);

        /// <summary>
        /// "Country rank for {0}"
        /// </summary>
        public static LocalisableString ShowRankCountry(string mode) => new TranslatableString(getKey(@"show.rank.country"), @"Country rank for {0}", mode);

        /// <summary>
        /// "Country Ranking"
        /// </summary>
        public static LocalisableString ShowRankCountrySimple => new TranslatableString(getKey(@"show.rank.country_simple"), @"Country Ranking");

        /// <summary>
        /// "Global rank for {0}"
        /// </summary>
        public static LocalisableString ShowRankGlobal(string mode) => new TranslatableString(getKey(@"show.rank.global"), @"Global rank for {0}", mode);

        /// <summary>
        /// "Global Ranking"
        /// </summary>
        public static LocalisableString ShowRankGlobalSimple => new TranslatableString(getKey(@"show.rank.global_simple"), @"Global Ranking");

        /// <summary>
        /// "Hit Accuracy"
        /// </summary>
        public static LocalisableString ShowStatsHitAccuracy => new TranslatableString(getKey(@"show.stats.hit_accuracy"), @"Hit Accuracy");

        /// <summary>
        /// "Level {0}"
        /// </summary>
        public static LocalisableString ShowStatsLevel(string level) => new TranslatableString(getKey(@"show.stats.level"), @"Level {0}", level);

        /// <summary>
        /// "Progress to next level"
        /// </summary>
        public static LocalisableString ShowStatsLevelProgress => new TranslatableString(getKey(@"show.stats.level_progress"), @"Progress to next level");

        /// <summary>
        /// "Maximum Combo"
        /// </summary>
        public static LocalisableString ShowStatsMaximumCombo => new TranslatableString(getKey(@"show.stats.maximum_combo"), @"Maximum Combo");

        /// <summary>
        /// "Medals"
        /// </summary>
        public static LocalisableString ShowStatsMedals => new TranslatableString(getKey(@"show.stats.medals"), @"Medals");

        /// <summary>
        /// "Play Count"
        /// </summary>
        public static LocalisableString ShowStatsPlayCount => new TranslatableString(getKey(@"show.stats.play_count"), @"Play Count");

        /// <summary>
        /// "Total Play Time"
        /// </summary>
        public static LocalisableString ShowStatsPlayTime => new TranslatableString(getKey(@"show.stats.play_time"), @"Total Play Time");

        /// <summary>
        /// "Ranked Score"
        /// </summary>
        public static LocalisableString ShowStatsRankedScore => new TranslatableString(getKey(@"show.stats.ranked_score"), @"Ranked Score");

        /// <summary>
        /// "Replays Watched by Others"
        /// </summary>
        public static LocalisableString ShowStatsReplaysWatchedByOthers => new TranslatableString(getKey(@"show.stats.replays_watched_by_others"), @"Replays Watched by Others");

        /// <summary>
        /// "Score Ranks"
        /// </summary>
        public static LocalisableString ShowStatsScoreRanks => new TranslatableString(getKey(@"show.stats.score_ranks"), @"Score Ranks");

        /// <summary>
        /// "Total Hits"
        /// </summary>
        public static LocalisableString ShowStatsTotalHits => new TranslatableString(getKey(@"show.stats.total_hits"), @"Total Hits");

        /// <summary>
        /// "Total Score"
        /// </summary>
        public static LocalisableString ShowStatsTotalScore => new TranslatableString(getKey(@"show.stats.total_score"), @"Total Score");

        /// <summary>
        /// "Graveyarded Beatmaps"
        /// </summary>
        public static LocalisableString ShowStatsGraveyardBeatmapsetCount => new TranslatableString(getKey(@"show.stats.graveyard_beatmapset_count"), @"Graveyarded Beatmaps");

        /// <summary>
        /// "Loved Beatmaps"
        /// </summary>
        public static LocalisableString ShowStatsLovedBeatmapsetCount => new TranslatableString(getKey(@"show.stats.loved_beatmapset_count"), @"Loved Beatmaps");

        /// <summary>
        /// "Pending Beatmaps"
        /// </summary>
        public static LocalisableString ShowStatsPendingBeatmapsetCount => new TranslatableString(getKey(@"show.stats.pending_beatmapset_count"), @"Pending Beatmaps");

        /// <summary>
        /// "Ranked Beatmaps"
        /// </summary>
        public static LocalisableString ShowStatsRankedBeatmapsetCount => new TranslatableString(getKey(@"show.stats.ranked_beatmapset_count"), @"Ranked Beatmaps");

        /// <summary>
        /// "You are currently silenced."
        /// </summary>
        public static LocalisableString SilencedBannerTitle => new TranslatableString(getKey(@"silenced_banner.title"), @"You are currently silenced.");

        /// <summary>
        /// "Some actions may be unavailable."
        /// </summary>
        public static LocalisableString SilencedBannerMessage => new TranslatableString(getKey(@"silenced_banner.message"), @"Some actions may be unavailable.");

        /// <summary>
        /// "All"
        /// </summary>
        public static LocalisableString StatusAll => new TranslatableString(getKey(@"status.all"), @"All");

        /// <summary>
        /// "Online"
        /// </summary>
        public static LocalisableString StatusOnline => new TranslatableString(getKey(@"status.online"), @"Online");

        /// <summary>
        /// "Offline"
        /// </summary>
        public static LocalisableString StatusOffline => new TranslatableString(getKey(@"status.offline"), @"Offline");

        /// <summary>
        /// "User created"
        /// </summary>
        public static LocalisableString StoreSaved => new TranslatableString(getKey(@"store.saved"), @"User created");

        /// <summary>
        /// "Account Verification"
        /// </summary>
        public static LocalisableString VerifyTitle => new TranslatableString(getKey(@"verify.title"), @"Account Verification");

        /// <summary>
        /// "Brick view"
        /// </summary>
        public static LocalisableString ViewModeBrick => new TranslatableString(getKey(@"view_mode.brick"), @"Brick view");

        /// <summary>
        /// "Card view"
        /// </summary>
        public static LocalisableString ViewModeCard => new TranslatableString(getKey(@"view_mode.card"), @"Card view");

        /// <summary>
        /// "List view"
        /// </summary>
        public static LocalisableString ViewModeList => new TranslatableString(getKey(@"view_mode.list"), @"List view");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}