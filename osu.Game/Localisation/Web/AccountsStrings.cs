// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class AccountsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Accounts";

        /// <summary>
        /// "account settings"
        /// </summary>
        public static LocalisableString EditTitleCompact => new TranslatableString(getKey(@"edit.title_compact"), @"account settings");

        /// <summary>
        /// "username"
        /// </summary>
        public static LocalisableString EditUsername => new TranslatableString(getKey(@"edit.username"), @"username");

        /// <summary>
        /// "Avatar"
        /// </summary>
        public static LocalisableString EditAvatarTitle => new TranslatableString(getKey(@"edit.avatar.title"), @"Avatar");

        /// <summary>
        /// "Please ensure your avatar adheres to {0}.&lt;br/&gt;This means it must be &lt;strong&gt;suitable for all ages&lt;/strong&gt;. i.e. no nudity, profanity or suggestive content."
        /// </summary>
        public static LocalisableString EditAvatarRules(string link) => new TranslatableString(getKey(@"edit.avatar.rules"), @"Please ensure your avatar adheres to {0}.<br/>This means it must be <strong>suitable for all ages</strong>. i.e. no nudity, profanity or suggestive content.", link);

        /// <summary>
        /// "the community rules"
        /// </summary>
        public static LocalisableString EditAvatarRulesLink => new TranslatableString(getKey(@"edit.avatar.rules_link"), @"the community rules");

        /// <summary>
        /// "current email"
        /// </summary>
        public static LocalisableString EditEmailCurrent => new TranslatableString(getKey(@"edit.email.current"), @"current email");

        /// <summary>
        /// "new email"
        /// </summary>
        public static LocalisableString EditEmailNew => new TranslatableString(getKey(@"edit.email.new"), @"new email");

        /// <summary>
        /// "email confirmation"
        /// </summary>
        public static LocalisableString EditEmailNewConfirmation => new TranslatableString(getKey(@"edit.email.new_confirmation"), @"email confirmation");

        /// <summary>
        /// "Email"
        /// </summary>
        public static LocalisableString EditEmailTitle => new TranslatableString(getKey(@"edit.email.title"), @"Email");

        /// <summary>
        /// "current password"
        /// </summary>
        public static LocalisableString EditPasswordCurrent => new TranslatableString(getKey(@"edit.password.current"), @"current password");

        /// <summary>
        /// "new password"
        /// </summary>
        public static LocalisableString EditPasswordNew => new TranslatableString(getKey(@"edit.password.new"), @"new password");

        /// <summary>
        /// "password confirmation"
        /// </summary>
        public static LocalisableString EditPasswordNewConfirmation => new TranslatableString(getKey(@"edit.password.new_confirmation"), @"password confirmation");

        /// <summary>
        /// "Password"
        /// </summary>
        public static LocalisableString EditPasswordTitle => new TranslatableString(getKey(@"edit.password.title"), @"Password");

        /// <summary>
        /// "Profile"
        /// </summary>
        public static LocalisableString EditProfileTitle => new TranslatableString(getKey(@"edit.profile.title"), @"Profile");

        /// <summary>
        /// "discord"
        /// </summary>
        public static LocalisableString EditProfileUserUserDiscord => new TranslatableString(getKey(@"edit.profile.user.user_discord"), @"discord");

        /// <summary>
        /// "current location"
        /// </summary>
        public static LocalisableString EditProfileUserUserFrom => new TranslatableString(getKey(@"edit.profile.user.user_from"), @"current location");

        /// <summary>
        /// "interests"
        /// </summary>
        public static LocalisableString EditProfileUserUserInterests => new TranslatableString(getKey(@"edit.profile.user.user_interests"), @"interests");

        /// <summary>
        /// "occupation"
        /// </summary>
        public static LocalisableString EditProfileUserUserOcc => new TranslatableString(getKey(@"edit.profile.user.user_occ"), @"occupation");

        /// <summary>
        /// "twitter"
        /// </summary>
        public static LocalisableString EditProfileUserUserTwitter => new TranslatableString(getKey(@"edit.profile.user.user_twitter"), @"twitter");

        /// <summary>
        /// "website"
        /// </summary>
        public static LocalisableString EditProfileUserUserWebsite => new TranslatableString(getKey(@"edit.profile.user.user_website"), @"website");

        /// <summary>
        /// "Signature"
        /// </summary>
        public static LocalisableString EditSignatureTitle => new TranslatableString(getKey(@"edit.signature.title"), @"Signature");

        /// <summary>
        /// "update"
        /// </summary>
        public static LocalisableString EditSignatureUpdate => new TranslatableString(getKey(@"edit.signature.update"), @"update");

        /// <summary>
        /// "receive notifications for new problem on qualified beatmaps of following modes"
        /// </summary>
        public static LocalisableString NotificationsBeatmapsetDiscussionQualifiedProblem => new TranslatableString(getKey(@"notifications.beatmapset_discussion_qualified_problem"), @"receive notifications for new problem on qualified beatmaps of following modes");

        /// <summary>
        /// "receive notifications for when beatmaps of following modes are disqualified"
        /// </summary>
        public static LocalisableString NotificationsBeatmapsetDisqualify => new TranslatableString(getKey(@"notifications.beatmapset_disqualify"), @"receive notifications for when beatmaps of following modes are disqualified");

        /// <summary>
        /// "receive notifications for replies to your comments"
        /// </summary>
        public static LocalisableString NotificationsCommentReply => new TranslatableString(getKey(@"notifications.comment_reply"), @"receive notifications for replies to your comments");

        /// <summary>
        /// "Notifications"
        /// </summary>
        public static LocalisableString NotificationsTitle => new TranslatableString(getKey(@"notifications.title"), @"Notifications");

        /// <summary>
        /// "automatically enable notifications on new forum topics that you create"
        /// </summary>
        public static LocalisableString NotificationsTopicAutoSubscribe => new TranslatableString(getKey(@"notifications.topic_auto_subscribe"), @"automatically enable notifications on new forum topics that you create");

        /// <summary>
        /// "delivery options"
        /// </summary>
        public static LocalisableString NotificationsOptionsDefault => new TranslatableString(getKey(@"notifications.options._"), @"delivery options");

        /// <summary>
        /// "guest difficulty"
        /// </summary>
        public static LocalisableString NotificationsOptionsBeatmapOwnerChange => new TranslatableString(getKey(@"notifications.options.beatmap_owner_change"), @"guest difficulty");

        /// <summary>
        /// "beatmap modding"
        /// </summary>
        public static LocalisableString NotificationsOptionsBeatmapsetModding => new TranslatableString(getKey(@"notifications.options.beatmapset:modding"), @"beatmap modding");

        /// <summary>
        /// "private chat messages"
        /// </summary>
        public static LocalisableString NotificationsOptionsChannelMessage => new TranslatableString(getKey(@"notifications.options.channel_message"), @"private chat messages");

        /// <summary>
        /// "new comments"
        /// </summary>
        public static LocalisableString NotificationsOptionsCommentNew => new TranslatableString(getKey(@"notifications.options.comment_new"), @"new comments");

        /// <summary>
        /// "topic reply"
        /// </summary>
        public static LocalisableString NotificationsOptionsForumTopicReply => new TranslatableString(getKey(@"notifications.options.forum_topic_reply"), @"topic reply");

        /// <summary>
        /// "mail"
        /// </summary>
        public static LocalisableString NotificationsOptionsMail => new TranslatableString(getKey(@"notifications.options.mail"), @"mail");

        /// <summary>
        /// "beatmap mapper"
        /// </summary>
        public static LocalisableString NotificationsOptionsMapping => new TranslatableString(getKey(@"notifications.options.mapping"), @"beatmap mapper");

        /// <summary>
        /// "push"
        /// </summary>
        public static LocalisableString NotificationsOptionsPush => new TranslatableString(getKey(@"notifications.options.push"), @"push");

        /// <summary>
        /// "user medal unlocked"
        /// </summary>
        public static LocalisableString NotificationsOptionsUserAchievementUnlock => new TranslatableString(getKey(@"notifications.options.user_achievement_unlock"), @"user medal unlocked");

        /// <summary>
        /// "authorized clients"
        /// </summary>
        public static LocalisableString OauthAuthorizedClients => new TranslatableString(getKey(@"oauth.authorized_clients"), @"authorized clients");

        /// <summary>
        /// "own clients"
        /// </summary>
        public static LocalisableString OauthOwnClients => new TranslatableString(getKey(@"oauth.own_clients"), @"own clients");

        /// <summary>
        /// "OAuth"
        /// </summary>
        public static LocalisableString OauthTitle => new TranslatableString(getKey(@"oauth.title"), @"OAuth");

        /// <summary>
        /// "hide warnings for explicit content in beatmaps"
        /// </summary>
        public static LocalisableString OptionsBeatmapsetShowNsfw => new TranslatableString(getKey(@"options.beatmapset_show_nsfw"), @"hide warnings for explicit content in beatmaps");

        /// <summary>
        /// "show beatmap metadata in original language"
        /// </summary>
        public static LocalisableString OptionsBeatmapsetTitleShowOriginal => new TranslatableString(getKey(@"options.beatmapset_title_show_original"), @"show beatmap metadata in original language");

        /// <summary>
        /// "Options"
        /// </summary>
        public static LocalisableString OptionsTitle => new TranslatableString(getKey(@"options.title"), @"Options");

        /// <summary>
        /// "default beatmap download type"
        /// </summary>
        public static LocalisableString OptionsBeatmapsetDownloadDefault => new TranslatableString(getKey(@"options.beatmapset_download._"), @"default beatmap download type");

        /// <summary>
        /// "with video if available"
        /// </summary>
        public static LocalisableString OptionsBeatmapsetDownloadAll => new TranslatableString(getKey(@"options.beatmapset_download.all"), @"with video if available");

        /// <summary>
        /// "open in osu!direct"
        /// </summary>
        public static LocalisableString OptionsBeatmapsetDownloadDirect => new TranslatableString(getKey(@"options.beatmapset_download.direct"), @"open in osu!direct");

        /// <summary>
        /// "without video"
        /// </summary>
        public static LocalisableString OptionsBeatmapsetDownloadNoVideo => new TranslatableString(getKey(@"options.beatmapset_download.no_video"), @"without video");

        /// <summary>
        /// "keyboard"
        /// </summary>
        public static LocalisableString PlaystylesKeyboard => new TranslatableString(getKey(@"playstyles.keyboard"), @"keyboard");

        /// <summary>
        /// "mouse"
        /// </summary>
        public static LocalisableString PlaystylesMouse => new TranslatableString(getKey(@"playstyles.mouse"), @"mouse");

        /// <summary>
        /// "tablet"
        /// </summary>
        public static LocalisableString PlaystylesTablet => new TranslatableString(getKey(@"playstyles.tablet"), @"tablet");

        /// <summary>
        /// "Playstyles"
        /// </summary>
        public static LocalisableString PlaystylesTitle => new TranslatableString(getKey(@"playstyles.title"), @"Playstyles");

        /// <summary>
        /// "touch"
        /// </summary>
        public static LocalisableString PlaystylesTouch => new TranslatableString(getKey(@"playstyles.touch"), @"touch");

        /// <summary>
        /// "block private messages from people not on your friends list"
        /// </summary>
        public static LocalisableString PrivacyFriendsOnly => new TranslatableString(getKey(@"privacy.friends_only"), @"block private messages from people not on your friends list");

        /// <summary>
        /// "hide your online presence"
        /// </summary>
        public static LocalisableString PrivacyHideOnline => new TranslatableString(getKey(@"privacy.hide_online"), @"hide your online presence");

        /// <summary>
        /// "Privacy"
        /// </summary>
        public static LocalisableString PrivacyTitle => new TranslatableString(getKey(@"privacy.title"), @"Privacy");

        /// <summary>
        /// "current"
        /// </summary>
        public static LocalisableString SecurityCurrentSession => new TranslatableString(getKey(@"security.current_session"), @"current");

        /// <summary>
        /// "End Session"
        /// </summary>
        public static LocalisableString SecurityEndSession => new TranslatableString(getKey(@"security.end_session"), @"End Session");

        /// <summary>
        /// "This will immediately end your session on that device. Are you sure?"
        /// </summary>
        public static LocalisableString SecurityEndSessionConfirmation => new TranslatableString(getKey(@"security.end_session_confirmation"), @"This will immediately end your session on that device. Are you sure?");

        /// <summary>
        /// "Last active:"
        /// </summary>
        public static LocalisableString SecurityLastActive => new TranslatableString(getKey(@"security.last_active"), @"Last active:");

        /// <summary>
        /// "Security"
        /// </summary>
        public static LocalisableString SecurityTitle => new TranslatableString(getKey(@"security.title"), @"Security");

        /// <summary>
        /// "web sessions"
        /// </summary>
        public static LocalisableString SecurityWebSessions => new TranslatableString(getKey(@"security.web_sessions"), @"web sessions");

        /// <summary>
        /// "update"
        /// </summary>
        public static LocalisableString UpdateEmailUpdate => new TranslatableString(getKey(@"update_email.update"), @"update");

        /// <summary>
        /// "update"
        /// </summary>
        public static LocalisableString UpdatePasswordUpdate => new TranslatableString(getKey(@"update_password.update"), @"update");

        /// <summary>
        /// "You can close this tab/window now"
        /// </summary>
        public static LocalisableString VerificationCompletedText => new TranslatableString(getKey(@"verification_completed.text"), @"You can close this tab/window now");

        /// <summary>
        /// "Verification has been completed"
        /// </summary>
        public static LocalisableString VerificationCompletedTitle => new TranslatableString(getKey(@"verification_completed.title"), @"Verification has been completed");

        /// <summary>
        /// "Invalid or expired verification link"
        /// </summary>
        public static LocalisableString VerificationInvalidTitle => new TranslatableString(getKey(@"verification_invalid.title"), @"Invalid or expired verification link");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}