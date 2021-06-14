// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class ChatStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Chat";

        /// <summary>
        /// "talking in {0}"
        /// </summary>
        public static LocalisableString TalkingIn(string channel) => new TranslatableString(getKey(@"talking_in"), @"talking in {0}", channel);

        /// <summary>
        /// "talking with {0}"
        /// </summary>
        public static LocalisableString TalkingWith(string name) => new TranslatableString(getKey(@"talking_with"), @"talking with {0}", name);

        /// <summary>
        /// "chat"
        /// </summary>
        public static LocalisableString TitleCompact => new TranslatableString(getKey(@"title_compact"), @"chat");

        /// <summary>
        /// "You cannot message this channel at this time. This may be due to any of the following reasons:"
        /// </summary>
        public static LocalisableString CannotSendChannel => new TranslatableString(getKey(@"cannot_send.channel"), @"You cannot message this channel at this time. This may be due to any of the following reasons:");

        /// <summary>
        /// "You cannot message this user at this time. This may be due to any of the following reasons:"
        /// </summary>
        public static LocalisableString CannotSendUser => new TranslatableString(getKey(@"cannot_send.user"), @"You cannot message this user at this time. This may be due to any of the following reasons:");

        /// <summary>
        /// "You were blocked by the recipient"
        /// </summary>
        public static LocalisableString CannotSendReasonsBlocked => new TranslatableString(getKey(@"cannot_send.reasons.blocked"), @"You were blocked by the recipient");

        /// <summary>
        /// "The channel has been moderated"
        /// </summary>
        public static LocalisableString CannotSendReasonsChannelModerated => new TranslatableString(getKey(@"cannot_send.reasons.channel_moderated"), @"The channel has been moderated");

        /// <summary>
        /// "The recipient only accepts messages from people on their friends list"
        /// </summary>
        public static LocalisableString CannotSendReasonsFriendsOnly => new TranslatableString(getKey(@"cannot_send.reasons.friends_only"), @"The recipient only accepts messages from people on their friends list");

        /// <summary>
        /// "You have not played the game enough"
        /// </summary>
        public static LocalisableString CannotSendReasonsNotEnoughPlays => new TranslatableString(getKey(@"cannot_send.reasons.not_enough_plays"), @"You have not played the game enough");

        /// <summary>
        /// "Your session has not been verified"
        /// </summary>
        public static LocalisableString CannotSendReasonsNotVerified => new TranslatableString(getKey(@"cannot_send.reasons.not_verified"), @"Your session has not been verified");

        /// <summary>
        /// "You are currently restricted"
        /// </summary>
        public static LocalisableString CannotSendReasonsRestricted => new TranslatableString(getKey(@"cannot_send.reasons.restricted"), @"You are currently restricted");

        /// <summary>
        /// "You are currently silenced"
        /// </summary>
        public static LocalisableString CannotSendReasonsSilenced => new TranslatableString(getKey(@"cannot_send.reasons.silenced"), @"You are currently silenced");

        /// <summary>
        /// "The recipient is currently restricted"
        /// </summary>
        public static LocalisableString CannotSendReasonsTargetRestricted => new TranslatableString(getKey(@"cannot_send.reasons.target_restricted"), @"The recipient is currently restricted");

        /// <summary>
        /// "unable to send message..."
        /// </summary>
        public static LocalisableString InputDisabled => new TranslatableString(getKey(@"input.disabled"), @"unable to send message...");

        /// <summary>
        /// "type message..."
        /// </summary>
        public static LocalisableString InputPlaceholder => new TranslatableString(getKey(@"input.placeholder"), @"type message...");

        /// <summary>
        /// "Send"
        /// </summary>
        public static LocalisableString InputSend => new TranslatableString(getKey(@"input.send"), @"Send");

        /// <summary>
        /// "Start conversations from a user&#39;s profile or a usercard popup."
        /// </summary>
        public static LocalisableString NoConversationsHowto => new TranslatableString(getKey(@"no-conversations.howto"), @"Start conversations from a user's profile or a usercard popup.");

        /// <summary>
        /// "Public channels you join via &lt;a href=&quot;{0}&quot;&gt;osu!lazer&lt;/a&gt; will also be visible here."
        /// </summary>
        public static LocalisableString NoConversationsLazer(string link) => new TranslatableString(getKey(@"no-conversations.lazer"), @"Public channels you join via <a href=""{0}"">osu!lazer</a> will also be visible here.", link);

        /// <summary>
        /// "no conversations yet"
        /// </summary>
        public static LocalisableString NoConversationsTitle => new TranslatableString(getKey(@"no-conversations.title"), @"no conversations yet");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}