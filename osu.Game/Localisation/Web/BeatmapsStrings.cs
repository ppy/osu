// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class BeatmapsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Beatmaps";

        /// <summary>
        /// "Failed updating vote"
        /// </summary>
        public static LocalisableString DiscussionVotesUpdateError => new TranslatableString(getKey(@"discussion-votes.update.error"), @"Failed updating vote");

        /// <summary>
        /// "allow kudosu"
        /// </summary>
        public static LocalisableString DiscussionsAllowKudosu => new TranslatableString(getKey(@"discussions.allow_kudosu"), @"allow kudosu");

        /// <summary>
        /// "Beatmap Page"
        /// </summary>
        public static LocalisableString DiscussionsBeatmapInformation => new TranslatableString(getKey(@"discussions.beatmap_information"), @"Beatmap Page");

        /// <summary>
        /// "delete"
        /// </summary>
        public static LocalisableString DiscussionsDelete => new TranslatableString(getKey(@"discussions.delete"), @"delete");

        /// <summary>
        /// "Deleted by {0} {1}."
        /// </summary>
        public static LocalisableString DiscussionsDeleted(string editor, string deleteTime) => new TranslatableString(getKey(@"discussions.deleted"), @"Deleted by {0} {1}.", editor, deleteTime);

        /// <summary>
        /// "deny kudosu"
        /// </summary>
        public static LocalisableString DiscussionsDenyKudosu => new TranslatableString(getKey(@"discussions.deny_kudosu"), @"deny kudosu");

        /// <summary>
        /// "edit"
        /// </summary>
        public static LocalisableString DiscussionsEdit => new TranslatableString(getKey(@"discussions.edit"), @"edit");

        /// <summary>
        /// "Last edited by {0} {1}."
        /// </summary>
        public static LocalisableString DiscussionsEdited(string editor, string updateTime) => new TranslatableString(getKey(@"discussions.edited"), @"Last edited by {0} {1}.", editor, updateTime);

        /// <summary>
        /// "Guest difficulty by {0}"
        /// </summary>
        public static LocalisableString DiscussionsGuest(string user) => new TranslatableString(getKey(@"discussions.guest"), @"Guest difficulty by {0}", user);

        /// <summary>
        /// "Denied from obtaining kudosu."
        /// </summary>
        public static LocalisableString DiscussionsKudosuDenied => new TranslatableString(getKey(@"discussions.kudosu_denied"), @"Denied from obtaining kudosu.");

        /// <summary>
        /// "This difficulty has been deleted so it may no longer be discussed."
        /// </summary>
        public static LocalisableString DiscussionsMessagePlaceholderDeletedBeatmap => new TranslatableString(getKey(@"discussions.message_placeholder_deleted_beatmap"), @"This difficulty has been deleted so it may no longer be discussed.");

        /// <summary>
        /// "Discussion for this beatmap has been disabled."
        /// </summary>
        public static LocalisableString DiscussionsMessagePlaceholderLocked => new TranslatableString(getKey(@"discussions.message_placeholder_locked"), @"Discussion for this beatmap has been disabled.");

        /// <summary>
        /// "Can&#39;t post discussion while silenced."
        /// </summary>
        public static LocalisableString DiscussionsMessagePlaceholderSilenced => new TranslatableString(getKey(@"discussions.message_placeholder_silenced"), @"Can't post discussion while silenced.");

        /// <summary>
        /// "Select Comment Type"
        /// </summary>
        public static LocalisableString DiscussionsMessageTypeSelect => new TranslatableString(getKey(@"discussions.message_type_select"), @"Select Comment Type");

        /// <summary>
        /// "Press enter to reply."
        /// </summary>
        public static LocalisableString DiscussionsReplyNotice => new TranslatableString(getKey(@"discussions.reply_notice"), @"Press enter to reply.");

        /// <summary>
        /// "Type your response here"
        /// </summary>
        public static LocalisableString DiscussionsReplyPlaceholder => new TranslatableString(getKey(@"discussions.reply_placeholder"), @"Type your response here");

        /// <summary>
        /// "Please sign in to post or reply"
        /// </summary>
        public static LocalisableString DiscussionsRequireLogin => new TranslatableString(getKey(@"discussions.require-login"), @"Please sign in to post or reply");

        /// <summary>
        /// "Resolved"
        /// </summary>
        public static LocalisableString DiscussionsResolved => new TranslatableString(getKey(@"discussions.resolved"), @"Resolved");

        /// <summary>
        /// "restore"
        /// </summary>
        public static LocalisableString DiscussionsRestore => new TranslatableString(getKey(@"discussions.restore"), @"restore");

        /// <summary>
        /// "Show deleted"
        /// </summary>
        public static LocalisableString DiscussionsShowDeleted => new TranslatableString(getKey(@"discussions.show_deleted"), @"Show deleted");

        /// <summary>
        /// "Discussions"
        /// </summary>
        public static LocalisableString DiscussionsTitle => new TranslatableString(getKey(@"discussions.title"), @"Discussions");

        /// <summary>
        /// "Collapse all"
        /// </summary>
        public static LocalisableString DiscussionsCollapseAllCollapse => new TranslatableString(getKey(@"discussions.collapse.all-collapse"), @"Collapse all");

        /// <summary>
        /// "Expand all"
        /// </summary>
        public static LocalisableString DiscussionsCollapseAllExpand => new TranslatableString(getKey(@"discussions.collapse.all-expand"), @"Expand all");

        /// <summary>
        /// "No discussions yet!"
        /// </summary>
        public static LocalisableString DiscussionsEmptyEmpty => new TranslatableString(getKey(@"discussions.empty.empty"), @"No discussions yet!");

        /// <summary>
        /// "No discussion matches selected filter."
        /// </summary>
        public static LocalisableString DiscussionsEmptyHidden => new TranslatableString(getKey(@"discussions.empty.hidden"), @"No discussion matches selected filter.");

        /// <summary>
        /// "Lock discussion"
        /// </summary>
        public static LocalisableString DiscussionsLockButtonLock => new TranslatableString(getKey(@"discussions.lock.button.lock"), @"Lock discussion");

        /// <summary>
        /// "Unlock discussion"
        /// </summary>
        public static LocalisableString DiscussionsLockButtonUnlock => new TranslatableString(getKey(@"discussions.lock.button.unlock"), @"Unlock discussion");

        /// <summary>
        /// "Reason for locking"
        /// </summary>
        public static LocalisableString DiscussionsLockPromptLock => new TranslatableString(getKey(@"discussions.lock.prompt.lock"), @"Reason for locking");

        /// <summary>
        /// "Are you sure to unlock?"
        /// </summary>
        public static LocalisableString DiscussionsLockPromptUnlock => new TranslatableString(getKey(@"discussions.lock.prompt.unlock"), @"Are you sure to unlock?");

        /// <summary>
        /// "This post will go to general beatmap discussion. To mod this difficulty, start message with timestamp (e.g. 00:12:345)."
        /// </summary>
        public static LocalisableString DiscussionsMessageHintInGeneral => new TranslatableString(getKey(@"discussions.message_hint.in_general"), @"This post will go to general beatmap discussion. To mod this difficulty, start message with timestamp (e.g. 00:12:345).");

        /// <summary>
        /// "To mod multiple timestamps, post multiple times (one post per timestamp)."
        /// </summary>
        public static LocalisableString DiscussionsMessageHintInTimeline => new TranslatableString(getKey(@"discussions.message_hint.in_timeline"), @"To mod multiple timestamps, post multiple times (one post per timestamp).");

        /// <summary>
        /// "Type here to post to General ({0})"
        /// </summary>
        public static LocalisableString DiscussionsMessagePlaceholderGeneral(string version) => new TranslatableString(getKey(@"discussions.message_placeholder.general"), @"Type here to post to General ({0})", version);

        /// <summary>
        /// "Type here to post to General (All difficulties)"
        /// </summary>
        public static LocalisableString DiscussionsMessagePlaceholderGeneralAll => new TranslatableString(getKey(@"discussions.message_placeholder.generalall"), @"Type here to post to General (All difficulties)");

        /// <summary>
        /// "Type here to post a review"
        /// </summary>
        public static LocalisableString DiscussionsMessagePlaceholderReview => new TranslatableString(getKey(@"discussions.message_placeholder.review"), @"Type here to post a review");

        /// <summary>
        /// "Type here to post to Timeline ({0})"
        /// </summary>
        public static LocalisableString DiscussionsMessagePlaceholderTimeline(string version) => new TranslatableString(getKey(@"discussions.message_placeholder.timeline"), @"Type here to post to Timeline ({0})", version);

        /// <summary>
        /// "Disqualify"
        /// </summary>
        public static LocalisableString DiscussionsMessageTypeDisqualify => new TranslatableString(getKey(@"discussions.message_type.disqualify"), @"Disqualify");

        /// <summary>
        /// "Hype!"
        /// </summary>
        public static LocalisableString DiscussionsMessageTypeHype => new TranslatableString(getKey(@"discussions.message_type.hype"), @"Hype!");

        /// <summary>
        /// "Note"
        /// </summary>
        public static LocalisableString DiscussionsMessageTypeMapperNote => new TranslatableString(getKey(@"discussions.message_type.mapper_note"), @"Note");

        /// <summary>
        /// "Reset Nomination"
        /// </summary>
        public static LocalisableString DiscussionsMessageTypeNominationReset => new TranslatableString(getKey(@"discussions.message_type.nomination_reset"), @"Reset Nomination");

        /// <summary>
        /// "Praise"
        /// </summary>
        public static LocalisableString DiscussionsMessageTypePraise => new TranslatableString(getKey(@"discussions.message_type.praise"), @"Praise");

        /// <summary>
        /// "Problem"
        /// </summary>
        public static LocalisableString DiscussionsMessageTypeProblem => new TranslatableString(getKey(@"discussions.message_type.problem"), @"Problem");

        /// <summary>
        /// "Review"
        /// </summary>
        public static LocalisableString DiscussionsMessageTypeReview => new TranslatableString(getKey(@"discussions.message_type.review"), @"Review");

        /// <summary>
        /// "Suggestion"
        /// </summary>
        public static LocalisableString DiscussionsMessageTypeSuggestion => new TranslatableString(getKey(@"discussions.message_type.suggestion"), @"Suggestion");

        /// <summary>
        /// "History"
        /// </summary>
        public static LocalisableString DiscussionsModeEvents => new TranslatableString(getKey(@"discussions.mode.events"), @"History");

        /// <summary>
        /// "General {0}"
        /// </summary>
        public static LocalisableString DiscussionsModeGeneral(string scope) => new TranslatableString(getKey(@"discussions.mode.general"), @"General {0}", scope);

        /// <summary>
        /// "Reviews"
        /// </summary>
        public static LocalisableString DiscussionsModeReviews => new TranslatableString(getKey(@"discussions.mode.reviews"), @"Reviews");

        /// <summary>
        /// "Timeline"
        /// </summary>
        public static LocalisableString DiscussionsModeTimeline => new TranslatableString(getKey(@"discussions.mode.timeline"), @"Timeline");

        /// <summary>
        /// "This difficulty"
        /// </summary>
        public static LocalisableString DiscussionsModeScopesGeneral => new TranslatableString(getKey(@"discussions.mode.scopes.general"), @"This difficulty");

        /// <summary>
        /// "All difficulties"
        /// </summary>
        public static LocalisableString DiscussionsModeScopesGeneralAll => new TranslatableString(getKey(@"discussions.mode.scopes.generalall"), @"All difficulties");

        /// <summary>
        /// "Pin"
        /// </summary>
        public static LocalisableString DiscussionsNewPin => new TranslatableString(getKey(@"discussions.new.pin"), @"Pin");

        /// <summary>
        /// "Timestamp"
        /// </summary>
        public static LocalisableString DiscussionsNewTimestamp => new TranslatableString(getKey(@"discussions.new.timestamp"), @"Timestamp");

        /// <summary>
        /// "ctrl-c in edit mode and paste in your message to add a timestamp!"
        /// </summary>
        public static LocalisableString DiscussionsNewTimestampMissing => new TranslatableString(getKey(@"discussions.new.timestamp_missing"), @"ctrl-c in edit mode and paste in your message to add a timestamp!");

        /// <summary>
        /// "New Discussion"
        /// </summary>
        public static LocalisableString DiscussionsNewTitle => new TranslatableString(getKey(@"discussions.new.title"), @"New Discussion");

        /// <summary>
        /// "Unpin"
        /// </summary>
        public static LocalisableString DiscussionsNewUnpin => new TranslatableString(getKey(@"discussions.new.unpin"), @"Unpin");

        /// <summary>
        /// "New Review"
        /// </summary>
        public static LocalisableString DiscussionsReviewNew => new TranslatableString(getKey(@"discussions.review.new"), @"New Review");

        /// <summary>
        /// "Delete"
        /// </summary>
        public static LocalisableString DiscussionsReviewEmbedDelete => new TranslatableString(getKey(@"discussions.review.embed.delete"), @"Delete");

        /// <summary>
        /// "[DISCUSSION DELETED]"
        /// </summary>
        public static LocalisableString DiscussionsReviewEmbedMissing => new TranslatableString(getKey(@"discussions.review.embed.missing"), @"[DISCUSSION DELETED]");

        /// <summary>
        /// "Unlink"
        /// </summary>
        public static LocalisableString DiscussionsReviewEmbedUnlink => new TranslatableString(getKey(@"discussions.review.embed.unlink"), @"Unlink");

        /// <summary>
        /// "Unsaved"
        /// </summary>
        public static LocalisableString DiscussionsReviewEmbedUnsaved => new TranslatableString(getKey(@"discussions.review.embed.unsaved"), @"Unsaved");

        /// <summary>
        /// "Posts on &quot;All difficulties&quot; can&#39;t be timestamped."
        /// </summary>
        public static LocalisableString DiscussionsReviewEmbedTimestampAllDiff => new TranslatableString(getKey(@"discussions.review.embed.timestamp.all-diff"), @"Posts on ""All difficulties"" can't be timestamped.");

        /// <summary>
        /// "If this {0} starts with a timestamp, it will be shown under Timeline."
        /// </summary>
        public static LocalisableString DiscussionsReviewEmbedTimestampDiff(string type) => new TranslatableString(getKey(@"discussions.review.embed.timestamp.diff"), @"If this {0} starts with a timestamp, it will be shown under Timeline.", type);

        /// <summary>
        /// "insert paragraph"
        /// </summary>
        public static LocalisableString DiscussionsReviewInsertBlockParagraph => new TranslatableString(getKey(@"discussions.review.insert-block.paragraph"), @"insert paragraph");

        /// <summary>
        /// "insert praise"
        /// </summary>
        public static LocalisableString DiscussionsReviewInsertBlockPraise => new TranslatableString(getKey(@"discussions.review.insert-block.praise"), @"insert praise");

        /// <summary>
        /// "insert problem"
        /// </summary>
        public static LocalisableString DiscussionsReviewInsertBlockProblem => new TranslatableString(getKey(@"discussions.review.insert-block.problem"), @"insert problem");

        /// <summary>
        /// "insert suggestion"
        /// </summary>
        public static LocalisableString DiscussionsReviewInsertBlockSuggestion => new TranslatableString(getKey(@"discussions.review.insert-block.suggestion"), @"insert suggestion");

        /// <summary>
        /// "{0} mapped by {1}"
        /// </summary>
        public static LocalisableString DiscussionsShowTitle(string title, string mapper) => new TranslatableString(getKey(@"discussions.show.title"), @"{0} mapped by {1}", title, mapper);

        /// <summary>
        /// "Creation time"
        /// </summary>
        public static LocalisableString DiscussionsSortCreatedAt => new TranslatableString(getKey(@"discussions.sort.created_at"), @"Creation time");

        /// <summary>
        /// "Timeline"
        /// </summary>
        public static LocalisableString DiscussionsSortTimeline => new TranslatableString(getKey(@"discussions.sort.timeline"), @"Timeline");

        /// <summary>
        /// "Last update"
        /// </summary>
        public static LocalisableString DiscussionsSortUpdatedAt => new TranslatableString(getKey(@"discussions.sort.updated_at"), @"Last update");

        /// <summary>
        /// "Deleted"
        /// </summary>
        public static LocalisableString DiscussionsStatsDeleted => new TranslatableString(getKey(@"discussions.stats.deleted"), @"Deleted");

        /// <summary>
        /// "Notes"
        /// </summary>
        public static LocalisableString DiscussionsStatsMapperNotes => new TranslatableString(getKey(@"discussions.stats.mapper_notes"), @"Notes");

        /// <summary>
        /// "Mine"
        /// </summary>
        public static LocalisableString DiscussionsStatsMine => new TranslatableString(getKey(@"discussions.stats.mine"), @"Mine");

        /// <summary>
        /// "Pending"
        /// </summary>
        public static LocalisableString DiscussionsStatsPending => new TranslatableString(getKey(@"discussions.stats.pending"), @"Pending");

        /// <summary>
        /// "Praises"
        /// </summary>
        public static LocalisableString DiscussionsStatsPraises => new TranslatableString(getKey(@"discussions.stats.praises"), @"Praises");

        /// <summary>
        /// "Resolved"
        /// </summary>
        public static LocalisableString DiscussionsStatsResolved => new TranslatableString(getKey(@"discussions.stats.resolved"), @"Resolved");

        /// <summary>
        /// "All"
        /// </summary>
        public static LocalisableString DiscussionsStatsTotal => new TranslatableString(getKey(@"discussions.stats.total"), @"All");

        /// <summary>
        /// "This beatmap was approved on {0}!"
        /// </summary>
        public static LocalisableString DiscussionsStatusMessagesApproved(string date) => new TranslatableString(getKey(@"discussions.status-messages.approved"), @"This beatmap was approved on {0}!", date);

        /// <summary>
        /// "This beatmap wasn&#39;t updated since {0} so it was graveyarded..."
        /// </summary>
        public static LocalisableString DiscussionsStatusMessagesGraveyard(string date) => new TranslatableString(getKey(@"discussions.status-messages.graveyard"), @"This beatmap wasn't updated since {0} so it was graveyarded...", date);

        /// <summary>
        /// "This beatmap was added to loved on {0}!"
        /// </summary>
        public static LocalisableString DiscussionsStatusMessagesLoved(string date) => new TranslatableString(getKey(@"discussions.status-messages.loved"), @"This beatmap was added to loved on {0}!", date);

        /// <summary>
        /// "This beatmap was ranked on {0}!"
        /// </summary>
        public static LocalisableString DiscussionsStatusMessagesRanked(string date) => new TranslatableString(getKey(@"discussions.status-messages.ranked"), @"This beatmap was ranked on {0}!", date);

        /// <summary>
        /// "Note: This beatmap is marked as a work-in-progress by the creator."
        /// </summary>
        public static LocalisableString DiscussionsStatusMessagesWip => new TranslatableString(getKey(@"discussions.status-messages.wip"), @"Note: This beatmap is marked as a work-in-progress by the creator.");

        /// <summary>
        /// "No downvotes yet"
        /// </summary>
        public static LocalisableString DiscussionsVotesNoneDown => new TranslatableString(getKey(@"discussions.votes.none.down"), @"No downvotes yet");

        /// <summary>
        /// "No upvotes yet"
        /// </summary>
        public static LocalisableString DiscussionsVotesNoneUp => new TranslatableString(getKey(@"discussions.votes.none.up"), @"No upvotes yet");

        /// <summary>
        /// "Latest downvotes"
        /// </summary>
        public static LocalisableString DiscussionsVotesLatestDown => new TranslatableString(getKey(@"discussions.votes.latest.down"), @"Latest downvotes");

        /// <summary>
        /// "Latest upvotes"
        /// </summary>
        public static LocalisableString DiscussionsVotesLatestUp => new TranslatableString(getKey(@"discussions.votes.latest.up"), @"Latest upvotes");

        /// <summary>
        /// "Hype Beatmap!"
        /// </summary>
        public static LocalisableString HypeButton => new TranslatableString(getKey(@"hype.button"), @"Hype Beatmap!");

        /// <summary>
        /// "Already Hyped!"
        /// </summary>
        public static LocalisableString HypeButtonDone => new TranslatableString(getKey(@"hype.button_done"), @"Already Hyped!");

        /// <summary>
        /// "Are you sure? This will use one out of your remaining {0} hype and can&#39;t be undone."
        /// </summary>
        public static LocalisableString HypeConfirm(string n) => new TranslatableString(getKey(@"hype.confirm"), @"Are you sure? This will use one out of your remaining {0} hype and can't be undone.", n);

        /// <summary>
        /// "Hype this beatmap to make it more visible for nomination and ranking!"
        /// </summary>
        public static LocalisableString HypeExplanation => new TranslatableString(getKey(@"hype.explanation"), @"Hype this beatmap to make it more visible for nomination and ranking!");

        /// <summary>
        /// "Sign in and hype this beatmap to make it more visible for nomination and ranking!"
        /// </summary>
        public static LocalisableString HypeExplanationGuest => new TranslatableString(getKey(@"hype.explanation_guest"), @"Sign in and hype this beatmap to make it more visible for nomination and ranking!");

        /// <summary>
        /// "You&#39;ll get another hype {0}."
        /// </summary>
        public static LocalisableString HypeNewTime(string newTime) => new TranslatableString(getKey(@"hype.new_time"), @"You'll get another hype {0}.", newTime);

        /// <summary>
        /// "You have {0} hype left."
        /// </summary>
        public static LocalisableString HypeRemaining(string remaining) => new TranslatableString(getKey(@"hype.remaining"), @"You have {0} hype left.", remaining);

        /// <summary>
        /// "Hype: {0}/{1}"
        /// </summary>
        public static LocalisableString HypeRequiredText(string current, string required) => new TranslatableString(getKey(@"hype.required_text"), @"Hype: {0}/{1}", current, required);

        /// <summary>
        /// "Hype Train"
        /// </summary>
        public static LocalisableString HypeSectionTitle => new TranslatableString(getKey(@"hype.section_title"), @"Hype Train");

        /// <summary>
        /// "Hype"
        /// </summary>
        public static LocalisableString HypeTitle => new TranslatableString(getKey(@"hype.title"), @"Hype");

        /// <summary>
        /// "Leave Feedback"
        /// </summary>
        public static LocalisableString FeedbackButton => new TranslatableString(getKey(@"feedback.button"), @"Leave Feedback");

        /// <summary>
        /// "Delete"
        /// </summary>
        public static LocalisableString NominationsDelete => new TranslatableString(getKey(@"nominations.delete"), @"Delete");

        /// <summary>
        /// "Are you sure? The beatmap will be deleted and you will be redirected back to your profile."
        /// </summary>
        public static LocalisableString NominationsDeleteOwnConfirm => new TranslatableString(getKey(@"nominations.delete_own_confirm"), @"Are you sure? The beatmap will be deleted and you will be redirected back to your profile.");

        /// <summary>
        /// "Are you sure? The beatmap will be deleted and you will be redirected back to the user&#39;s profile."
        /// </summary>
        public static LocalisableString NominationsDeleteOtherConfirm => new TranslatableString(getKey(@"nominations.delete_other_confirm"), @"Are you sure? The beatmap will be deleted and you will be redirected back to the user's profile.");

        /// <summary>
        /// "Reason for disqualification?"
        /// </summary>
        public static LocalisableString NominationsDisqualificationPrompt => new TranslatableString(getKey(@"nominations.disqualification_prompt"), @"Reason for disqualification?");

        /// <summary>
        /// "Disqualified {0} ({1})."
        /// </summary>
        public static LocalisableString NominationsDisqualifiedAt(string timeAgo, string reason) => new TranslatableString(getKey(@"nominations.disqualified_at"), @"Disqualified {0} ({1}).", timeAgo, reason);

        /// <summary>
        /// "no reason specified"
        /// </summary>
        public static LocalisableString NominationsDisqualifiedNoReason => new TranslatableString(getKey(@"nominations.disqualified_no_reason"), @"no reason specified");

        /// <summary>
        /// "Disqualify"
        /// </summary>
        public static LocalisableString NominationsDisqualify => new TranslatableString(getKey(@"nominations.disqualify"), @"Disqualify");

        /// <summary>
        /// "Error performing that action, try refreshing the page."
        /// </summary>
        public static LocalisableString NominationsIncorrectState => new TranslatableString(getKey(@"nominations.incorrect_state"), @"Error performing that action, try refreshing the page.");

        /// <summary>
        /// "Love"
        /// </summary>
        public static LocalisableString NominationsLove => new TranslatableString(getKey(@"nominations.love"), @"Love");

        /// <summary>
        /// "Love this beatmap?"
        /// </summary>
        public static LocalisableString NominationsLoveConfirm => new TranslatableString(getKey(@"nominations.love_confirm"), @"Love this beatmap?");

        /// <summary>
        /// "Nominate"
        /// </summary>
        public static LocalisableString NominationsNominate => new TranslatableString(getKey(@"nominations.nominate"), @"Nominate");

        /// <summary>
        /// "Nominate this beatmap?"
        /// </summary>
        public static LocalisableString NominationsNominateConfirm => new TranslatableString(getKey(@"nominations.nominate_confirm"), @"Nominate this beatmap?");

        /// <summary>
        /// "nominated by {0}"
        /// </summary>
        public static LocalisableString NominationsNominatedBy(string users) => new TranslatableString(getKey(@"nominations.nominated_by"), @"nominated by {0}", users);

        /// <summary>
        /// "There isn&#39;t enough hype."
        /// </summary>
        public static LocalisableString NominationsNotEnoughHype => new TranslatableString(getKey(@"nominations.not_enough_hype"), @"There isn't enough hype.");

        /// <summary>
        /// "Remove from Loved"
        /// </summary>
        public static LocalisableString NominationsRemoveFromLoved => new TranslatableString(getKey(@"nominations.remove_from_loved"), @"Remove from Loved");

        /// <summary>
        /// "Reason for removing from Loved:"
        /// </summary>
        public static LocalisableString NominationsRemoveFromLovedPrompt => new TranslatableString(getKey(@"nominations.remove_from_loved_prompt"), @"Reason for removing from Loved:");

        /// <summary>
        /// "Nominations: {0}/{1}"
        /// </summary>
        public static LocalisableString NominationsRequiredText(string current, string required) => new TranslatableString(getKey(@"nominations.required_text"), @"Nominations: {0}/{1}", current, required);

        /// <summary>
        /// "deleted"
        /// </summary>
        public static LocalisableString NominationsResetMessageDeleted => new TranslatableString(getKey(@"nominations.reset_message_deleted"), @"deleted");

        /// <summary>
        /// "Nomination Status"
        /// </summary>
        public static LocalisableString NominationsTitle => new TranslatableString(getKey(@"nominations.title"), @"Nomination Status");

        /// <summary>
        /// "There are still unresolved issues that must be addressed first."
        /// </summary>
        public static LocalisableString NominationsUnresolvedIssues => new TranslatableString(getKey(@"nominations.unresolved_issues"), @"There are still unresolved issues that must be addressed first.");

        /// <summary>
        /// "This map is estimated to be ranked {0} if no issues are found. It is #{1} in the {2}."
        /// </summary>
        public static LocalisableString NominationsRankEstimateDefault(string date, string position, string queue) => new TranslatableString(getKey(@"nominations.rank_estimate._"), @"This map is estimated to be ranked {0} if no issues are found. It is #{1} in the {2}.", date, position, queue);

        /// <summary>
        /// "ranking queue"
        /// </summary>
        public static LocalisableString NominationsRankEstimateQueue => new TranslatableString(getKey(@"nominations.rank_estimate.queue"), @"ranking queue");

        /// <summary>
        /// "soon"
        /// </summary>
        public static LocalisableString NominationsRankEstimateSoon => new TranslatableString(getKey(@"nominations.rank_estimate.soon"), @"soon");

        /// <summary>
        /// "Nomination process reset {0} by {1} with new problem {2} ({3})."
        /// </summary>
        public static LocalisableString NominationsResetAtNominationReset(string timeAgo, string user, string discussion, string message) => new TranslatableString(getKey(@"nominations.reset_at.nomination_reset"), @"Nomination process reset {0} by {1} with new problem {2} ({3}).", timeAgo, user, discussion, message);

        /// <summary>
        /// "Disqualified {0} by {1} with new problem {2} ({3})."
        /// </summary>
        public static LocalisableString NominationsResetAtDisqualify(string timeAgo, string user, string discussion, string message) => new TranslatableString(getKey(@"nominations.reset_at.disqualify"), @"Disqualified {0} by {1} with new problem {2} ({3}).", timeAgo, user, discussion, message);

        /// <summary>
        /// "Are you sure? Posting a new problem will reset the nomination process."
        /// </summary>
        public static LocalisableString NominationsResetConfirmNominationReset => new TranslatableString(getKey(@"nominations.reset_confirm.nomination_reset"), @"Are you sure? Posting a new problem will reset the nomination process.");

        /// <summary>
        /// "Are you sure? This will remove the beatmap from qualifying and reset the nomination process."
        /// </summary>
        public static LocalisableString NominationsResetConfirmDisqualify => new TranslatableString(getKey(@"nominations.reset_confirm.disqualify"), @"Are you sure? This will remove the beatmap from qualifying and reset the nomination process.");

        /// <summary>
        /// "type in keywords..."
        /// </summary>
        public static LocalisableString ListingSearchPrompt => new TranslatableString(getKey(@"listing.search.prompt"), @"type in keywords...");

        /// <summary>
        /// "Sign in to search."
        /// </summary>
        public static LocalisableString ListingSearchLoginRequired => new TranslatableString(getKey(@"listing.search.login_required"), @"Sign in to search.");

        /// <summary>
        /// "More Search Options"
        /// </summary>
        public static LocalisableString ListingSearchOptions => new TranslatableString(getKey(@"listing.search.options"), @"More Search Options");

        /// <summary>
        /// "Filtering by {0} requires an active osu!supporter tag"
        /// </summary>
        public static LocalisableString ListingSearchSupporterFilter(string filters) => new TranslatableString(getKey(@"listing.search.supporter_filter"), @"Filtering by {0} requires an active osu!supporter tag", filters);

        /// <summary>
        /// "no results"
        /// </summary>
        public static LocalisableString ListingSearchNotFound => new TranslatableString(getKey(@"listing.search.not-found"), @"no results");

        /// <summary>
        /// "... nope, nothing found."
        /// </summary>
        public static LocalisableString ListingSearchNotFoundQuote => new TranslatableString(getKey(@"listing.search.not-found-quote"), @"... nope, nothing found.");

        /// <summary>
        /// "Extra"
        /// </summary>
        public static LocalisableString ListingSearchFiltersExtra => new TranslatableString(getKey(@"listing.search.filters.extra"), @"Extra");

        /// <summary>
        /// "General"
        /// </summary>
        public static LocalisableString ListingSearchFiltersGeneral => new TranslatableString(getKey(@"listing.search.filters.general"), @"General");

        /// <summary>
        /// "Genre"
        /// </summary>
        public static LocalisableString ListingSearchFiltersGenre => new TranslatableString(getKey(@"listing.search.filters.genre"), @"Genre");

        /// <summary>
        /// "Language"
        /// </summary>
        public static LocalisableString ListingSearchFiltersLanguage => new TranslatableString(getKey(@"listing.search.filters.language"), @"Language");

        /// <summary>
        /// "Mode"
        /// </summary>
        public static LocalisableString ListingSearchFiltersMode => new TranslatableString(getKey(@"listing.search.filters.mode"), @"Mode");

        /// <summary>
        /// "Explicit Content"
        /// </summary>
        public static LocalisableString ListingSearchFiltersNsfw => new TranslatableString(getKey(@"listing.search.filters.nsfw"), @"Explicit Content");

        /// <summary>
        /// "Played"
        /// </summary>
        public static LocalisableString ListingSearchFiltersPlayed => new TranslatableString(getKey(@"listing.search.filters.played"), @"Played");

        /// <summary>
        /// "Rank Achieved"
        /// </summary>
        public static LocalisableString ListingSearchFiltersRank => new TranslatableString(getKey(@"listing.search.filters.rank"), @"Rank Achieved");

        /// <summary>
        /// "Categories"
        /// </summary>
        public static LocalisableString ListingSearchFiltersStatus => new TranslatableString(getKey(@"listing.search.filters.status"), @"Categories");

        /// <summary>
        /// "Title"
        /// </summary>
        public static LocalisableString ListingSearchSortingTitle => new TranslatableString(getKey(@"listing.search.sorting.title"), @"Title");

        /// <summary>
        /// "Artist"
        /// </summary>
        public static LocalisableString ListingSearchSortingArtist => new TranslatableString(getKey(@"listing.search.sorting.artist"), @"Artist");

        /// <summary>
        /// "Difficulty"
        /// </summary>
        public static LocalisableString ListingSearchSortingDifficulty => new TranslatableString(getKey(@"listing.search.sorting.difficulty"), @"Difficulty");

        /// <summary>
        /// "Favourites"
        /// </summary>
        public static LocalisableString ListingSearchSortingFavourites => new TranslatableString(getKey(@"listing.search.sorting.favourites"), @"Favourites");

        /// <summary>
        /// "Updated"
        /// </summary>
        public static LocalisableString ListingSearchSortingUpdated => new TranslatableString(getKey(@"listing.search.sorting.updated"), @"Updated");

        /// <summary>
        /// "Ranked"
        /// </summary>
        public static LocalisableString ListingSearchSortingRanked => new TranslatableString(getKey(@"listing.search.sorting.ranked"), @"Ranked");

        /// <summary>
        /// "Rating"
        /// </summary>
        public static LocalisableString ListingSearchSortingRating => new TranslatableString(getKey(@"listing.search.sorting.rating"), @"Rating");

        /// <summary>
        /// "Plays"
        /// </summary>
        public static LocalisableString ListingSearchSortingPlays => new TranslatableString(getKey(@"listing.search.sorting.plays"), @"Plays");

        /// <summary>
        /// "Relevance"
        /// </summary>
        public static LocalisableString ListingSearchSortingRelevance => new TranslatableString(getKey(@"listing.search.sorting.relevance"), @"Relevance");

        /// <summary>
        /// "Nominations"
        /// </summary>
        public static LocalisableString ListingSearchSortingNominations => new TranslatableString(getKey(@"listing.search.sorting.nominations"), @"Nominations");

        /// <summary>
        /// "Filtering by {0} requires an active {1}"
        /// </summary>
        public static LocalisableString ListingSearchSupporterFilterQuoteDefault(string filters, string link) => new TranslatableString(getKey(@"listing.search.supporter_filter_quote._"), @"Filtering by {0} requires an active {1}", filters, link);

        /// <summary>
        /// "osu!supporter tag"
        /// </summary>
        public static LocalisableString ListingSearchSupporterFilterQuoteLinkText => new TranslatableString(getKey(@"listing.search.supporter_filter_quote.link_text"), @"osu!supporter tag");

        /// <summary>
        /// "Include converted beatmaps"
        /// </summary>
        public static LocalisableString GeneralConverts => new TranslatableString(getKey(@"general.converts"), @"Include converted beatmaps");

        /// <summary>
        /// "Subscribed mappers"
        /// </summary>
        public static LocalisableString GeneralFollows => new TranslatableString(getKey(@"general.follows"), @"Subscribed mappers");

        /// <summary>
        /// "Recommended difficulty"
        /// </summary>
        public static LocalisableString GeneralRecommended => new TranslatableString(getKey(@"general.recommended"), @"Recommended difficulty");

        /// <summary>
        /// "All"
        /// </summary>
        public static LocalisableString ModeAll => new TranslatableString(getKey(@"mode.all"), @"All");

        /// <summary>
        /// "Any"
        /// </summary>
        public static LocalisableString ModeAny => new TranslatableString(getKey(@"mode.any"), @"Any");

        /// <summary>
        /// "osu!"
        /// </summary>
        public static LocalisableString ModeOsu => new TranslatableString(getKey(@"mode.osu"), @"osu!");

        /// <summary>
        /// "osu!taiko"
        /// </summary>
        public static LocalisableString ModeTaiko => new TranslatableString(getKey(@"mode.taiko"), @"osu!taiko");

        /// <summary>
        /// "osu!catch"
        /// </summary>
        public static LocalisableString ModeFruits => new TranslatableString(getKey(@"mode.fruits"), @"osu!catch");

        /// <summary>
        /// "osu!mania"
        /// </summary>
        public static LocalisableString ModeMania => new TranslatableString(getKey(@"mode.mania"), @"osu!mania");

        /// <summary>
        /// "Any"
        /// </summary>
        public static LocalisableString StatusAny => new TranslatableString(getKey(@"status.any"), @"Any");

        /// <summary>
        /// "Approved"
        /// </summary>
        public static LocalisableString StatusApproved => new TranslatableString(getKey(@"status.approved"), @"Approved");

        /// <summary>
        /// "Favourites"
        /// </summary>
        public static LocalisableString StatusFavourites => new TranslatableString(getKey(@"status.favourites"), @"Favourites");

        /// <summary>
        /// "Graveyard"
        /// </summary>
        public static LocalisableString StatusGraveyard => new TranslatableString(getKey(@"status.graveyard"), @"Graveyard");

        /// <summary>
        /// "Has Leaderboard"
        /// </summary>
        public static LocalisableString StatusLeaderboard => new TranslatableString(getKey(@"status.leaderboard"), @"Has Leaderboard");

        /// <summary>
        /// "Loved"
        /// </summary>
        public static LocalisableString StatusLoved => new TranslatableString(getKey(@"status.loved"), @"Loved");

        /// <summary>
        /// "My Maps"
        /// </summary>
        public static LocalisableString StatusMine => new TranslatableString(getKey(@"status.mine"), @"My Maps");

        /// <summary>
        /// "Pending"
        /// </summary>
        public static LocalisableString StatusPending => new TranslatableString(getKey(@"status.pending"), @"Pending");

        /// <summary>
        /// "Qualified"
        /// </summary>
        public static LocalisableString StatusQualified => new TranslatableString(getKey(@"status.qualified"), @"Qualified");

        /// <summary>
        /// "Ranked"
        /// </summary>
        public static LocalisableString StatusRanked => new TranslatableString(getKey(@"status.ranked"), @"Ranked");

        /// <summary>
        /// "Any"
        /// </summary>
        public static LocalisableString GenreAny => new TranslatableString(getKey(@"genre.any"), @"Any");

        /// <summary>
        /// "Unspecified"
        /// </summary>
        public static LocalisableString GenreUnspecified => new TranslatableString(getKey(@"genre.unspecified"), @"Unspecified");

        /// <summary>
        /// "Video Game"
        /// </summary>
        public static LocalisableString GenreVideoGame => new TranslatableString(getKey(@"genre.video-game"), @"Video Game");

        /// <summary>
        /// "Anime"
        /// </summary>
        public static LocalisableString GenreAnime => new TranslatableString(getKey(@"genre.anime"), @"Anime");

        /// <summary>
        /// "Rock"
        /// </summary>
        public static LocalisableString GenreRock => new TranslatableString(getKey(@"genre.rock"), @"Rock");

        /// <summary>
        /// "Pop"
        /// </summary>
        public static LocalisableString GenrePop => new TranslatableString(getKey(@"genre.pop"), @"Pop");

        /// <summary>
        /// "Other"
        /// </summary>
        public static LocalisableString GenreOther => new TranslatableString(getKey(@"genre.other"), @"Other");

        /// <summary>
        /// "Novelty"
        /// </summary>
        public static LocalisableString GenreNovelty => new TranslatableString(getKey(@"genre.novelty"), @"Novelty");

        /// <summary>
        /// "Hip Hop"
        /// </summary>
        public static LocalisableString GenreHipHop => new TranslatableString(getKey(@"genre.hip-hop"), @"Hip Hop");

        /// <summary>
        /// "Electronic"
        /// </summary>
        public static LocalisableString GenreElectronic => new TranslatableString(getKey(@"genre.electronic"), @"Electronic");

        /// <summary>
        /// "Metal"
        /// </summary>
        public static LocalisableString GenreMetal => new TranslatableString(getKey(@"genre.metal"), @"Metal");

        /// <summary>
        /// "Classical"
        /// </summary>
        public static LocalisableString GenreClassical => new TranslatableString(getKey(@"genre.classical"), @"Classical");

        /// <summary>
        /// "Folk"
        /// </summary>
        public static LocalisableString GenreFolk => new TranslatableString(getKey(@"genre.folk"), @"Folk");

        /// <summary>
        /// "Jazz"
        /// </summary>
        public static LocalisableString GenreJazz => new TranslatableString(getKey(@"genre.jazz"), @"Jazz");

        /// <summary>
        /// "4K"
        /// </summary>
        public static LocalisableString Mods4K => new TranslatableString(getKey(@"mods.4k"), @"4K");

        /// <summary>
        /// "5K"
        /// </summary>
        public static LocalisableString Mods5K => new TranslatableString(getKey(@"mods.5k"), @"5K");

        /// <summary>
        /// "6K"
        /// </summary>
        public static LocalisableString Mods6K => new TranslatableString(getKey(@"mods.6k"), @"6K");

        /// <summary>
        /// "7K"
        /// </summary>
        public static LocalisableString Mods7K => new TranslatableString(getKey(@"mods.7k"), @"7K");

        /// <summary>
        /// "8K"
        /// </summary>
        public static LocalisableString Mods8K => new TranslatableString(getKey(@"mods.8k"), @"8K");

        /// <summary>
        /// "9K"
        /// </summary>
        public static LocalisableString Mods9K => new TranslatableString(getKey(@"mods.9k"), @"9K");

        /// <summary>
        /// "Auto Pilot"
        /// </summary>
        public static LocalisableString ModsAP => new TranslatableString(getKey(@"mods.ap"), @"Auto Pilot");

        /// <summary>
        /// "Double Time"
        /// </summary>
        public static LocalisableString ModsDT => new TranslatableString(getKey(@"mods.dt"), @"Double Time");

        /// <summary>
        /// "Easy Mode"
        /// </summary>
        public static LocalisableString ModsEZ => new TranslatableString(getKey(@"mods.ez"), @"Easy Mode");

        /// <summary>
        /// "Fade In"
        /// </summary>
        public static LocalisableString ModsFI => new TranslatableString(getKey(@"mods.fi"), @"Fade In");

        /// <summary>
        /// "Flashlight"
        /// </summary>
        public static LocalisableString ModsFL => new TranslatableString(getKey(@"mods.fl"), @"Flashlight");

        /// <summary>
        /// "Hidden"
        /// </summary>
        public static LocalisableString ModsHD => new TranslatableString(getKey(@"mods.hd"), @"Hidden");

        /// <summary>
        /// "Hard Rock"
        /// </summary>
        public static LocalisableString ModsHR => new TranslatableString(getKey(@"mods.hr"), @"Hard Rock");

        /// <summary>
        /// "Half Time"
        /// </summary>
        public static LocalisableString ModsHT => new TranslatableString(getKey(@"mods.ht"), @"Half Time");

        /// <summary>
        /// "Mirror"
        /// </summary>
        public static LocalisableString ModsMR => new TranslatableString(getKey(@"mods.mr"), @"Mirror");

        /// <summary>
        /// "Nightcore"
        /// </summary>
        public static LocalisableString ModsNC => new TranslatableString(getKey(@"mods.nc"), @"Nightcore");

        /// <summary>
        /// "No Fail"
        /// </summary>
        public static LocalisableString ModsNF => new TranslatableString(getKey(@"mods.nf"), @"No Fail");

        /// <summary>
        /// "No mods"
        /// </summary>
        public static LocalisableString ModsNM => new TranslatableString(getKey(@"mods.nm"), @"No mods");

        /// <summary>
        /// "Perfect"
        /// </summary>
        public static LocalisableString ModsPF => new TranslatableString(getKey(@"mods.pf"), @"Perfect");

        /// <summary>
        /// "Relax"
        /// </summary>
        public static LocalisableString ModsRX => new TranslatableString(getKey(@"mods.rx"), @"Relax");

        /// <summary>
        /// "Sudden Death"
        /// </summary>
        public static LocalisableString ModsSD => new TranslatableString(getKey(@"mods.sd"), @"Sudden Death");

        /// <summary>
        /// "Spun Out"
        /// </summary>
        public static LocalisableString ModsSO => new TranslatableString(getKey(@"mods.so"), @"Spun Out");

        /// <summary>
        /// "Touch Device"
        /// </summary>
        public static LocalisableString ModsTD => new TranslatableString(getKey(@"mods.td"), @"Touch Device");

        /// <summary>
        /// "Score V2"
        /// </summary>
        public static LocalisableString ModsV2 => new TranslatableString(getKey(@"mods.v2"), @"Score V2");

        /// <summary>
        /// "Any"
        /// </summary>
        public static LocalisableString LanguageAny => new TranslatableString(getKey(@"language.any"), @"Any");

        /// <summary>
        /// "English"
        /// </summary>
        public static LocalisableString LanguageEnglish => new TranslatableString(getKey(@"language.english"), @"English");

        /// <summary>
        /// "Chinese"
        /// </summary>
        public static LocalisableString LanguageChinese => new TranslatableString(getKey(@"language.chinese"), @"Chinese");

        /// <summary>
        /// "French"
        /// </summary>
        public static LocalisableString LanguageFrench => new TranslatableString(getKey(@"language.french"), @"French");

        /// <summary>
        /// "German"
        /// </summary>
        public static LocalisableString LanguageGerman => new TranslatableString(getKey(@"language.german"), @"German");

        /// <summary>
        /// "Italian"
        /// </summary>
        public static LocalisableString LanguageItalian => new TranslatableString(getKey(@"language.italian"), @"Italian");

        /// <summary>
        /// "Japanese"
        /// </summary>
        public static LocalisableString LanguageJapanese => new TranslatableString(getKey(@"language.japanese"), @"Japanese");

        /// <summary>
        /// "Korean"
        /// </summary>
        public static LocalisableString LanguageKorean => new TranslatableString(getKey(@"language.korean"), @"Korean");

        /// <summary>
        /// "Spanish"
        /// </summary>
        public static LocalisableString LanguageSpanish => new TranslatableString(getKey(@"language.spanish"), @"Spanish");

        /// <summary>
        /// "Swedish"
        /// </summary>
        public static LocalisableString LanguageSwedish => new TranslatableString(getKey(@"language.swedish"), @"Swedish");

        /// <summary>
        /// "Russian"
        /// </summary>
        public static LocalisableString LanguageRussian => new TranslatableString(getKey(@"language.russian"), @"Russian");

        /// <summary>
        /// "Polish"
        /// </summary>
        public static LocalisableString LanguagePolish => new TranslatableString(getKey(@"language.polish"), @"Polish");

        /// <summary>
        /// "Instrumental"
        /// </summary>
        public static LocalisableString LanguageInstrumental => new TranslatableString(getKey(@"language.instrumental"), @"Instrumental");

        /// <summary>
        /// "Other"
        /// </summary>
        public static LocalisableString LanguageOther => new TranslatableString(getKey(@"language.other"), @"Other");

        /// <summary>
        /// "Unspecified"
        /// </summary>
        public static LocalisableString LanguageUnspecified => new TranslatableString(getKey(@"language.unspecified"), @"Unspecified");

        /// <summary>
        /// "Hide"
        /// </summary>
        public static LocalisableString NsfwExclude => new TranslatableString(getKey(@"nsfw.exclude"), @"Hide");

        /// <summary>
        /// "Show"
        /// </summary>
        public static LocalisableString NsfwInclude => new TranslatableString(getKey(@"nsfw.include"), @"Show");

        /// <summary>
        /// "Any"
        /// </summary>
        public static LocalisableString PlayedAny => new TranslatableString(getKey(@"played.any"), @"Any");

        /// <summary>
        /// "Played"
        /// </summary>
        public static LocalisableString PlayedPlayed => new TranslatableString(getKey(@"played.played"), @"Played");

        /// <summary>
        /// "Unplayed"
        /// </summary>
        public static LocalisableString PlayedUnplayed => new TranslatableString(getKey(@"played.unplayed"), @"Unplayed");

        /// <summary>
        /// "Has Video"
        /// </summary>
        public static LocalisableString ExtraVideo => new TranslatableString(getKey(@"extra.video"), @"Has Video");

        /// <summary>
        /// "Has Storyboard"
        /// </summary>
        public static LocalisableString ExtraStoryboard => new TranslatableString(getKey(@"extra.storyboard"), @"Has Storyboard");

        /// <summary>
        /// "Any"
        /// </summary>
        public static LocalisableString RankAny => new TranslatableString(getKey(@"rank.any"), @"Any");

        /// <summary>
        /// "Silver SS"
        /// </summary>
        public static LocalisableString RankXH => new TranslatableString(getKey(@"rank.xh"), @"Silver SS");

        /// <summary>
        /// "SS"
        /// </summary>
        public static LocalisableString RankX => new TranslatableString(getKey(@"rank.x"), @"SS");

        /// <summary>
        /// "Silver S"
        /// </summary>
        public static LocalisableString RankSH => new TranslatableString(getKey(@"rank.sh"), @"Silver S");

        /// <summary>
        /// "S"
        /// </summary>
        public static LocalisableString RankS => new TranslatableString(getKey(@"rank.s"), @"S");

        /// <summary>
        /// "A"
        /// </summary>
        public static LocalisableString RankA => new TranslatableString(getKey(@"rank.a"), @"A");

        /// <summary>
        /// "B"
        /// </summary>
        public static LocalisableString RankB => new TranslatableString(getKey(@"rank.b"), @"B");

        /// <summary>
        /// "C"
        /// </summary>
        public static LocalisableString RankC => new TranslatableString(getKey(@"rank.c"), @"C");

        /// <summary>
        /// "D"
        /// </summary>
        public static LocalisableString RankD => new TranslatableString(getKey(@"rank.d"), @"D");

        /// <summary>
        /// "Playcount: {0}"
        /// </summary>
        public static LocalisableString PanelPlaycount(string count) => new TranslatableString(getKey(@"panel.playcount"), @"Playcount: {0}", count);

        /// <summary>
        /// "Favourites: {0}"
        /// </summary>
        public static LocalisableString PanelFavourites(string count) => new TranslatableString(getKey(@"panel.favourites"), @"Favourites: {0}", count);

        /// <summary>
        /// "4K"
        /// </summary>
        public static LocalisableString VariantMania4k => new TranslatableString(getKey(@"variant.mania.4k"), @"4K");

        /// <summary>
        /// "7K"
        /// </summary>
        public static LocalisableString VariantMania7k => new TranslatableString(getKey(@"variant.mania.7k"), @"7K");

        /// <summary>
        /// "All"
        /// </summary>
        public static LocalisableString VariantManiaAll => new TranslatableString(getKey(@"variant.mania.all"), @"All");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}