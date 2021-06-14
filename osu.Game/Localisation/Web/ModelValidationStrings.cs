// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class ModelValidationStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.ModelValidation";

        /// <summary>
        /// "Invalid {0} specified."
        /// </summary>
        public static LocalisableString Invalid(string attribute) => new TranslatableString(getKey(@"invalid"), @"Invalid {0} specified.", attribute);

        /// <summary>
        /// "{0} cannot be negative."
        /// </summary>
        public static LocalisableString NotNegative(string attribute) => new TranslatableString(getKey(@"not_negative"), @"{0} cannot be negative.", attribute);

        /// <summary>
        /// "{0} is required."
        /// </summary>
        public static LocalisableString Required(string attribute) => new TranslatableString(getKey(@"required"), @"{0} is required.", attribute);

        /// <summary>
        /// "{0} exceeded maximum length - can only be up to {1} characters."
        /// </summary>
        public static LocalisableString TooLong(string attribute, string limit) => new TranslatableString(getKey(@"too_long"), @"{0} exceeded maximum length - can only be up to {1} characters.", attribute, limit);

        /// <summary>
        /// "Confirmation does not match."
        /// </summary>
        public static LocalisableString WrongConfirmation => new TranslatableString(getKey(@"wrong_confirmation"), @"Confirmation does not match.");

        /// <summary>
        /// "Timestamp is specified but beatmap difficulty is missing."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionBeatmapMissing => new TranslatableString(getKey(@"beatmapset_discussion.beatmap_missing"), @"Timestamp is specified but beatmap difficulty is missing.");

        /// <summary>
        /// "Beatmap can&#39;t be hyped."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionBeatmapsetNoHype => new TranslatableString(getKey(@"beatmapset_discussion.beatmapset_no_hype"), @"Beatmap can't be hyped.");

        /// <summary>
        /// "Hype must be done in the General (all difficulties) section."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionHypeRequiresNullBeatmap => new TranslatableString(getKey(@"beatmapset_discussion.hype_requires_null_beatmap"), @"Hype must be done in the General (all difficulties) section.");

        /// <summary>
        /// "Invalid difficulty specified."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionInvalidBeatmapId => new TranslatableString(getKey(@"beatmapset_discussion.invalid_beatmap_id"), @"Invalid difficulty specified.");

        /// <summary>
        /// "Invalid beatmap specified."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionInvalidBeatmapsetId => new TranslatableString(getKey(@"beatmapset_discussion.invalid_beatmapset_id"), @"Invalid beatmap specified.");

        /// <summary>
        /// "Discussion is locked."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionLocked => new TranslatableString(getKey(@"beatmapset_discussion.locked"), @"Discussion is locked.");

        /// <summary>
        /// "Message type"
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionAttributesMessageType => new TranslatableString(getKey(@"beatmapset_discussion.attributes.message_type"), @"Message type");

        /// <summary>
        /// "Timestamp"
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionAttributesTimestamp => new TranslatableString(getKey(@"beatmapset_discussion.attributes.timestamp"), @"Timestamp");

        /// <summary>
        /// "This beatmap is currently locked for discussion and can&#39;t be hyped"
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionHypeDiscussionLocked => new TranslatableString(getKey(@"beatmapset_discussion.hype.discussion_locked"), @"This beatmap is currently locked for discussion and can't be hyped");

        /// <summary>
        /// "Must be signed in to hype."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionHypeGuest => new TranslatableString(getKey(@"beatmapset_discussion.hype.guest"), @"Must be signed in to hype.");

        /// <summary>
        /// "You have already hyped this beatmap."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionHypeHyped => new TranslatableString(getKey(@"beatmapset_discussion.hype.hyped"), @"You have already hyped this beatmap.");

        /// <summary>
        /// "You have used all your hype."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionHypeLimitExceeded => new TranslatableString(getKey(@"beatmapset_discussion.hype.limit_exceeded"), @"You have used all your hype.");

        /// <summary>
        /// "This beatmap can not be hyped"
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionHypeNotHypeable => new TranslatableString(getKey(@"beatmapset_discussion.hype.not_hypeable"), @"This beatmap can not be hyped");

        /// <summary>
        /// "No hyping your own beatmap."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionHypeOwner => new TranslatableString(getKey(@"beatmapset_discussion.hype.owner"), @"No hyping your own beatmap.");

        /// <summary>
        /// "Specified timestamp is beyond the length of the beatmap."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionTimestampExceedsBeatmapsetLength => new TranslatableString(getKey(@"beatmapset_discussion.timestamp.exceeds_beatmapset_length"), @"Specified timestamp is beyond the length of the beatmap.");

        /// <summary>
        /// "Timestamp can&#39;t be negative."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionTimestampNegative => new TranslatableString(getKey(@"beatmapset_discussion.timestamp.negative"), @"Timestamp can't be negative.");

        /// <summary>
        /// "Discussion is locked."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionPostDiscussionLocked => new TranslatableString(getKey(@"beatmapset_discussion_post.discussion_locked"), @"Discussion is locked.");

        /// <summary>
        /// "Can not delete starting post."
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionPostFirstPost => new TranslatableString(getKey(@"beatmapset_discussion_post.first_post"), @"Can not delete starting post.");

        /// <summary>
        /// "The message"
        /// </summary>
        public static LocalisableString BeatmapsetDiscussionPostAttributesMessage => new TranslatableString(getKey(@"beatmapset_discussion_post.attributes.message"), @"The message");

        /// <summary>
        /// "Replying to deleted comment is not allowed."
        /// </summary>
        public static LocalisableString CommentDeletedParent => new TranslatableString(getKey(@"comment.deleted_parent"), @"Replying to deleted comment is not allowed.");

        /// <summary>
        /// "Pinning comment reply is not allowed."
        /// </summary>
        public static LocalisableString CommentTopOnly => new TranslatableString(getKey(@"comment.top_only"), @"Pinning comment reply is not allowed.");

        /// <summary>
        /// "The message"
        /// </summary>
        public static LocalisableString CommentAttributesMessage => new TranslatableString(getKey(@"comment.attributes.message"), @"The message");

        /// <summary>
        /// "Invalid {0} specified."
        /// </summary>
        public static LocalisableString FollowInvalid(string attribute) => new TranslatableString(getKey(@"follow.invalid"), @"Invalid {0} specified.", attribute);

        /// <summary>
        /// "Can only vote a feature request."
        /// </summary>
        public static LocalisableString ForumFeatureVoteNotFeatureTopic => new TranslatableString(getKey(@"forum.feature_vote.not_feature_topic"), @"Can only vote a feature request.");

        /// <summary>
        /// "Not enough votes."
        /// </summary>
        public static LocalisableString ForumFeatureVoteNotEnoughFeatureVotes => new TranslatableString(getKey(@"forum.feature_vote.not_enough_feature_votes"), @"Not enough votes.");

        /// <summary>
        /// "Invalid option specified."
        /// </summary>
        public static LocalisableString ForumPollVoteInvalid => new TranslatableString(getKey(@"forum.poll_vote.invalid"), @"Invalid option specified.");

        /// <summary>
        /// "Deleting beatmap metadata post is not allowed."
        /// </summary>
        public static LocalisableString ForumPostBeatmapsetPostNoDelete => new TranslatableString(getKey(@"forum.post.beatmapset_post_no_delete"), @"Deleting beatmap metadata post is not allowed.");

        /// <summary>
        /// "Editing beatmap metadata post is not allowed."
        /// </summary>
        public static LocalisableString ForumPostBeatmapsetPostNoEdit => new TranslatableString(getKey(@"forum.post.beatmapset_post_no_edit"), @"Editing beatmap metadata post is not allowed.");

        /// <summary>
        /// "Can&#39;t delete starting post"
        /// </summary>
        public static LocalisableString ForumPostFirstPostNoDelete => new TranslatableString(getKey(@"forum.post.first_post_no_delete"), @"Can't delete starting post");

        /// <summary>
        /// "Post is missing topic"
        /// </summary>
        public static LocalisableString ForumPostMissingTopic => new TranslatableString(getKey(@"forum.post.missing_topic"), @"Post is missing topic");

        /// <summary>
        /// "Your reply contains only a quote."
        /// </summary>
        public static LocalisableString ForumPostOnlyQuote => new TranslatableString(getKey(@"forum.post.only_quote"), @"Your reply contains only a quote.");

        /// <summary>
        /// "Post body"
        /// </summary>
        public static LocalisableString ForumPostAttributesPostText => new TranslatableString(getKey(@"forum.post.attributes.post_text"), @"Post body");

        /// <summary>
        /// "Topic title"
        /// </summary>
        public static LocalisableString ForumTopicAttributesTopicTitle => new TranslatableString(getKey(@"forum.topic.attributes.topic_title"), @"Topic title");

        /// <summary>
        /// "Duplicated option is not allowed."
        /// </summary>
        public static LocalisableString ForumTopicPollDuplicateOptions => new TranslatableString(getKey(@"forum.topic_poll.duplicate_options"), @"Duplicated option is not allowed.");

        /// <summary>
        /// "Can&#39;t edit a poll after more than {0} hours."
        /// </summary>
        public static LocalisableString ForumTopicPollGracePeriodExpired(string limit) => new TranslatableString(getKey(@"forum.topic_poll.grace_period_expired"), @"Can't edit a poll after more than {0} hours.", limit);

        /// <summary>
        /// "Can&#39;t hide results of a poll that never ends."
        /// </summary>
        public static LocalisableString ForumTopicPollHidingResultsForever => new TranslatableString(getKey(@"forum.topic_poll.hiding_results_forever"), @"Can't hide results of a poll that never ends.");

        /// <summary>
        /// "Option per user may not exceed the number of available options."
        /// </summary>
        public static LocalisableString ForumTopicPollInvalidMaxOptions => new TranslatableString(getKey(@"forum.topic_poll.invalid_max_options"), @"Option per user may not exceed the number of available options.");

        /// <summary>
        /// "A minimum of one option per user is required."
        /// </summary>
        public static LocalisableString ForumTopicPollMinimumOneSelection => new TranslatableString(getKey(@"forum.topic_poll.minimum_one_selection"), @"A minimum of one option per user is required.");

        /// <summary>
        /// "Need at least two options."
        /// </summary>
        public static LocalisableString ForumTopicPollMinimumTwoOptions => new TranslatableString(getKey(@"forum.topic_poll.minimum_two_options"), @"Need at least two options.");

        /// <summary>
        /// "Exceeded maximum number of allowed options."
        /// </summary>
        public static LocalisableString ForumTopicPollTooManyOptions => new TranslatableString(getKey(@"forum.topic_poll.too_many_options"), @"Exceeded maximum number of allowed options.");

        /// <summary>
        /// "Poll title"
        /// </summary>
        public static LocalisableString ForumTopicPollAttributesTitle => new TranslatableString(getKey(@"forum.topic_poll.attributes.title"), @"Poll title");

        /// <summary>
        /// "Select an option when voting."
        /// </summary>
        public static LocalisableString ForumTopicVoteRequired => new TranslatableString(getKey(@"forum.topic_vote.required"), @"Select an option when voting.");

        /// <summary>
        /// "Selected more options than allowed."
        /// </summary>
        public static LocalisableString ForumTopicVoteTooMany => new TranslatableString(getKey(@"forum.topic_vote.too_many"), @"Selected more options than allowed.");

        /// <summary>
        /// "Exceeded maximum number of allowed OAuth applications."
        /// </summary>
        public static LocalisableString OauthClientTooMany => new TranslatableString(getKey(@"oauth.client.too_many"), @"Exceeded maximum number of allowed OAuth applications.");

        /// <summary>
        /// "Please enter a valid URL."
        /// </summary>
        public static LocalisableString OauthClientUrl => new TranslatableString(getKey(@"oauth.client.url"), @"Please enter a valid URL.");

        /// <summary>
        /// "Application Name"
        /// </summary>
        public static LocalisableString OauthClientAttributesName => new TranslatableString(getKey(@"oauth.client.attributes.name"), @"Application Name");

        /// <summary>
        /// "Application Callback URL"
        /// </summary>
        public static LocalisableString OauthClientAttributesRedirect => new TranslatableString(getKey(@"oauth.client.attributes.redirect"), @"Application Callback URL");

        /// <summary>
        /// "Password may not contain username."
        /// </summary>
        public static LocalisableString UserContainsUsername => new TranslatableString(getKey(@"user.contains_username"), @"Password may not contain username.");

        /// <summary>
        /// "Email address already used."
        /// </summary>
        public static LocalisableString UserEmailAlreadyUsed => new TranslatableString(getKey(@"user.email_already_used"), @"Email address already used.");

        /// <summary>
        /// "Email address not allowed."
        /// </summary>
        public static LocalisableString UserEmailNotAllowed => new TranslatableString(getKey(@"user.email_not_allowed"), @"Email address not allowed.");

        /// <summary>
        /// "Country not in database."
        /// </summary>
        public static LocalisableString UserInvalidCountry => new TranslatableString(getKey(@"user.invalid_country"), @"Country not in database.");

        /// <summary>
        /// "Discord username invalid."
        /// </summary>
        public static LocalisableString UserInvalidDiscord => new TranslatableString(getKey(@"user.invalid_discord"), @"Discord username invalid.");

        /// <summary>
        /// "Doesn&#39;t seem to be a valid email address."
        /// </summary>
        public static LocalisableString UserInvalidEmail => new TranslatableString(getKey(@"user.invalid_email"), @"Doesn't seem to be a valid email address.");

        /// <summary>
        /// "Twitter username invalid."
        /// </summary>
        public static LocalisableString UserInvalidTwitter => new TranslatableString(getKey(@"user.invalid_twitter"), @"Twitter username invalid.");

        /// <summary>
        /// "New password is too short."
        /// </summary>
        public static LocalisableString UserTooShort => new TranslatableString(getKey(@"user.too_short"), @"New password is too short.");

        /// <summary>
        /// "Username or email address already used."
        /// </summary>
        public static LocalisableString UserUnknownDuplicate => new TranslatableString(getKey(@"user.unknown_duplicate"), @"Username or email address already used.");

        /// <summary>
        /// "This username will be available for use in {0}."
        /// </summary>
        public static LocalisableString UserUsernameAvailableIn(string duration) => new TranslatableString(getKey(@"user.username_available_in"), @"This username will be available for use in {0}.", duration);

        /// <summary>
        /// "This username will be available for use any minute now!"
        /// </summary>
        public static LocalisableString UserUsernameAvailableSoon => new TranslatableString(getKey(@"user.username_available_soon"), @"This username will be available for use any minute now!");

        /// <summary>
        /// "The requested username contains invalid characters."
        /// </summary>
        public static LocalisableString UserUsernameInvalidCharacters => new TranslatableString(getKey(@"user.username_invalid_characters"), @"The requested username contains invalid characters.");

        /// <summary>
        /// "Username is already in use!"
        /// </summary>
        public static LocalisableString UserUsernameInUse => new TranslatableString(getKey(@"user.username_in_use"), @"Username is already in use!");

        /// <summary>
        /// "Username is already in use!"
        /// </summary>
        public static LocalisableString UserUsernameLocked => new TranslatableString(getKey(@"user.username_locked"), @"Username is already in use!");

        /// <summary>
        /// "Please use either underscores or spaces, not both!"
        /// </summary>
        public static LocalisableString UserUsernameNoSpaceUserscoreMix => new TranslatableString(getKey(@"user.username_no_space_userscore_mix"), @"Please use either underscores or spaces, not both!");

        /// <summary>
        /// "Username can&#39;t start or end with spaces!"
        /// </summary>
        public static LocalisableString UserUsernameNoSpaces => new TranslatableString(getKey(@"user.username_no_spaces"), @"Username can't start or end with spaces!");

        /// <summary>
        /// "This username choice is not allowed."
        /// </summary>
        public static LocalisableString UserUsernameNotAllowed => new TranslatableString(getKey(@"user.username_not_allowed"), @"This username choice is not allowed.");

        /// <summary>
        /// "The requested username is too short."
        /// </summary>
        public static LocalisableString UserUsernameTooShort => new TranslatableString(getKey(@"user.username_too_short"), @"The requested username is too short.");

        /// <summary>
        /// "The requested username is too long."
        /// </summary>
        public static LocalisableString UserUsernameTooLong => new TranslatableString(getKey(@"user.username_too_long"), @"The requested username is too long.");

        /// <summary>
        /// "Blacklisted password."
        /// </summary>
        public static LocalisableString UserWeak => new TranslatableString(getKey(@"user.weak"), @"Blacklisted password.");

        /// <summary>
        /// "Current password is incorrect."
        /// </summary>
        public static LocalisableString UserWrongCurrentPassword => new TranslatableString(getKey(@"user.wrong_current_password"), @"Current password is incorrect.");

        /// <summary>
        /// "Email confirmation does not match."
        /// </summary>
        public static LocalisableString UserWrongEmailConfirmation => new TranslatableString(getKey(@"user.wrong_email_confirmation"), @"Email confirmation does not match.");

        /// <summary>
        /// "Password confirmation does not match."
        /// </summary>
        public static LocalisableString UserWrongPasswordConfirmation => new TranslatableString(getKey(@"user.wrong_password_confirmation"), @"Password confirmation does not match.");

        /// <summary>
        /// "Exceeded maximum length - can only be up to {0} characters."
        /// </summary>
        public static LocalisableString UserTooLong(string limit) => new TranslatableString(getKey(@"user.too_long"), @"Exceeded maximum length - can only be up to {0} characters.", limit);

        /// <summary>
        /// "Username"
        /// </summary>
        public static LocalisableString UserAttributesUsername => new TranslatableString(getKey(@"user.attributes.username"), @"Username");

        /// <summary>
        /// "Email address"
        /// </summary>
        public static LocalisableString UserAttributesUserEmail => new TranslatableString(getKey(@"user.attributes.user_email"), @"Email address");

        /// <summary>
        /// "Password"
        /// </summary>
        public static LocalisableString UserAttributesPassword => new TranslatableString(getKey(@"user.attributes.password"), @"Password");

        /// <summary>
        /// "You cannot change your username while restricted."
        /// </summary>
        public static LocalisableString UserChangeUsernameRestricted => new TranslatableString(getKey(@"user.change_username.restricted"), @"You cannot change your username while restricted.");

        /// <summary>
        /// "You must have {0} to change your name!"
        /// </summary>
        public static LocalisableString UserChangeUsernameSupporterRequiredDefault(string link) => new TranslatableString(getKey(@"user.change_username.supporter_required._"), @"You must have {0} to change your name!", link);

        /// <summary>
        /// "supported osu!"
        /// </summary>
        public static LocalisableString UserChangeUsernameSupporterRequiredLinkText => new TranslatableString(getKey(@"user.change_username.supporter_required.link_text"), @"supported osu!");

        /// <summary>
        /// "This is already your username, silly!"
        /// </summary>
        public static LocalisableString UserChangeUsernameUsernameIsSame => new TranslatableString(getKey(@"user.change_username.username_is_same"), @"This is already your username, silly!");

        /// <summary>
        /// "{0} is not valid for this report type."
        /// </summary>
        public static LocalisableString UserReportReasonNotValid(string reason) => new TranslatableString(getKey(@"user_report.reason_not_valid"), @"{0} is not valid for this report type.", reason);

        /// <summary>
        /// "You can&#39;t report yourself!"
        /// </summary>
        public static LocalisableString UserReportSelf => new TranslatableString(getKey(@"user_report.self"), @"You can't report yourself!");

        /// <summary>
        /// "Quantity"
        /// </summary>
        public static LocalisableString StoreOrderItemAttributesQuantity => new TranslatableString(getKey(@"store.order_item.attributes.quantity"), @"Quantity");

        /// <summary>
        /// "Cost"
        /// </summary>
        public static LocalisableString StoreOrderItemAttributesCost => new TranslatableString(getKey(@"store.order_item.attributes.cost"), @"Cost");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}