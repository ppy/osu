// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class AuthorizationStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Authorization";

        /// <summary>
        /// "How about playing some osu! instead?"
        /// </summary>
        public static LocalisableString PlayMore => new TranslatableString(getKey(@"play_more"), @"How about playing some osu! instead?");

        /// <summary>
        /// "Please sign in to proceed."
        /// </summary>
        public static LocalisableString RequireLogin => new TranslatableString(getKey(@"require_login"), @"Please sign in to proceed.");

        /// <summary>
        /// "Please verify to proceed."
        /// </summary>
        public static LocalisableString RequireVerification => new TranslatableString(getKey(@"require_verification"), @"Please verify to proceed.");

        /// <summary>
        /// "Can&#39;t do that while restricted."
        /// </summary>
        public static LocalisableString Restricted => new TranslatableString(getKey(@"restricted"), @"Can't do that while restricted.");

        /// <summary>
        /// "Can&#39;t do that while silenced."
        /// </summary>
        public static LocalisableString Silenced => new TranslatableString(getKey(@"silenced"), @"Can't do that while silenced.");

        /// <summary>
        /// "Access denied."
        /// </summary>
        public static LocalisableString Unauthorized => new TranslatableString(getKey(@"unauthorized"), @"Access denied.");

        /// <summary>
        /// "Can not undo hyping."
        /// </summary>
        public static LocalisableString BeatmapDiscussionDestroyIsHype => new TranslatableString(getKey(@"beatmap_discussion.destroy.is_hype"), @"Can not undo hyping.");

        /// <summary>
        /// "Can not delete discussion with replies"
        /// </summary>
        public static LocalisableString BeatmapDiscussionDestroyHasReply => new TranslatableString(getKey(@"beatmap_discussion.destroy.has_reply"), @"Can not delete discussion with replies");

        /// <summary>
        /// "You have reached your nomination limit for the day, please try again tomorrow."
        /// </summary>
        public static LocalisableString BeatmapDiscussionNominateExhausted => new TranslatableString(getKey(@"beatmap_discussion.nominate.exhausted"), @"You have reached your nomination limit for the day, please try again tomorrow.");

        /// <summary>
        /// "Error performing that action, try refreshing the page."
        /// </summary>
        public static LocalisableString BeatmapDiscussionNominateIncorrectState => new TranslatableString(getKey(@"beatmap_discussion.nominate.incorrect_state"), @"Error performing that action, try refreshing the page.");

        /// <summary>
        /// "Can&#39;t nominate own beatmap."
        /// </summary>
        public static LocalisableString BeatmapDiscussionNominateOwner => new TranslatableString(getKey(@"beatmap_discussion.nominate.owner"), @"Can't nominate own beatmap.");

        /// <summary>
        /// "You must set the genre and language before nominating."
        /// </summary>
        public static LocalisableString BeatmapDiscussionNominateSetMetadata => new TranslatableString(getKey(@"beatmap_discussion.nominate.set_metadata"), @"You must set the genre and language before nominating.");

        /// <summary>
        /// "Only thread starter and beatmap owner can resolve a discussion."
        /// </summary>
        public static LocalisableString BeatmapDiscussionResolveNotOwner => new TranslatableString(getKey(@"beatmap_discussion.resolve.not_owner"), @"Only thread starter and beatmap owner can resolve a discussion.");

        /// <summary>
        /// "Only beatmap owner or nominator/NAT group member can post mapper notes."
        /// </summary>
        public static LocalisableString BeatmapDiscussionStoreMapperNoteWrongUser => new TranslatableString(getKey(@"beatmap_discussion.store.mapper_note_wrong_user"), @"Only beatmap owner or nominator/NAT group member can post mapper notes.");

        /// <summary>
        /// "Can&#39;t vote on discussion made by bot"
        /// </summary>
        public static LocalisableString BeatmapDiscussionVoteBot => new TranslatableString(getKey(@"beatmap_discussion.vote.bot"), @"Can't vote on discussion made by bot");

        /// <summary>
        /// "Please wait a while before casting more votes"
        /// </summary>
        public static LocalisableString BeatmapDiscussionVoteLimitExceeded => new TranslatableString(getKey(@"beatmap_discussion.vote.limit_exceeded"), @"Please wait a while before casting more votes");

        /// <summary>
        /// "Can&#39;t vote on own discussion."
        /// </summary>
        public static LocalisableString BeatmapDiscussionVoteOwner => new TranslatableString(getKey(@"beatmap_discussion.vote.owner"), @"Can't vote on own discussion.");

        /// <summary>
        /// "Can only vote on discussions of pending beatmaps."
        /// </summary>
        public static LocalisableString BeatmapDiscussionVoteWrongBeatmapsetState => new TranslatableString(getKey(@"beatmap_discussion.vote.wrong_beatmapset_state"), @"Can only vote on discussions of pending beatmaps.");

        /// <summary>
        /// "You can only delete your own posts."
        /// </summary>
        public static LocalisableString BeatmapDiscussionPostDestroyNotOwner => new TranslatableString(getKey(@"beatmap_discussion_post.destroy.not_owner"), @"You can only delete your own posts.");

        /// <summary>
        /// "You can not delete a post of a resolved discussion."
        /// </summary>
        public static LocalisableString BeatmapDiscussionPostDestroyResolved => new TranslatableString(getKey(@"beatmap_discussion_post.destroy.resolved"), @"You can not delete a post of a resolved discussion.");

        /// <summary>
        /// "Automatically generated post can not be deleted."
        /// </summary>
        public static LocalisableString BeatmapDiscussionPostDestroySystemGenerated => new TranslatableString(getKey(@"beatmap_discussion_post.destroy.system_generated"), @"Automatically generated post can not be deleted.");

        /// <summary>
        /// "Only the poster can edit post."
        /// </summary>
        public static LocalisableString BeatmapDiscussionPostEditNotOwner => new TranslatableString(getKey(@"beatmap_discussion_post.edit.not_owner"), @"Only the poster can edit post.");

        /// <summary>
        /// "You can not edit a post of a resolved discussion."
        /// </summary>
        public static LocalisableString BeatmapDiscussionPostEditResolved => new TranslatableString(getKey(@"beatmap_discussion_post.edit.resolved"), @"You can not edit a post of a resolved discussion.");

        /// <summary>
        /// "Automatically generated post can not be edited."
        /// </summary>
        public static LocalisableString BeatmapDiscussionPostEditSystemGenerated => new TranslatableString(getKey(@"beatmap_discussion_post.edit.system_generated"), @"Automatically generated post can not be edited.");

        /// <summary>
        /// "This beatmap is locked for discussion."
        /// </summary>
        public static LocalisableString BeatmapDiscussionPostStoreBeatmapsetLocked => new TranslatableString(getKey(@"beatmap_discussion_post.store.beatmapset_locked"), @"This beatmap is locked for discussion.");

        /// <summary>
        /// "You cannot change metadata of a nominated map. Contact a BN or NAT member if you think it is set incorrectly."
        /// </summary>
        public static LocalisableString BeatmapsetMetadataNominated => new TranslatableString(getKey(@"beatmapset.metadata.nominated"), @"You cannot change metadata of a nominated map. Contact a BN or NAT member if you think it is set incorrectly.");

        /// <summary>
        /// "Cannot message a user that is blocking you or that you have blocked."
        /// </summary>
        public static LocalisableString ChatBlocked => new TranslatableString(getKey(@"chat.blocked"), @"Cannot message a user that is blocking you or that you have blocked.");

        /// <summary>
        /// "User is blocking messages from people not on their friends list."
        /// </summary>
        public static LocalisableString ChatFriendsOnly => new TranslatableString(getKey(@"chat.friends_only"), @"User is blocking messages from people not on their friends list.");

        /// <summary>
        /// "That channel is currently moderated."
        /// </summary>
        public static LocalisableString ChatModerated => new TranslatableString(getKey(@"chat.moderated"), @"That channel is currently moderated.");

        /// <summary>
        /// "You do not have access to that channel."
        /// </summary>
        public static LocalisableString ChatNoAccess => new TranslatableString(getKey(@"chat.no_access"), @"You do not have access to that channel.");

        /// <summary>
        /// "You cannot send messages while silenced, restricted or banned."
        /// </summary>
        public static LocalisableString ChatRestricted => new TranslatableString(getKey(@"chat.restricted"), @"You cannot send messages while silenced, restricted or banned.");

        /// <summary>
        /// "You cannot send messages while silenced, restricted or banned."
        /// </summary>
        public static LocalisableString ChatSilenced => new TranslatableString(getKey(@"chat.silenced"), @"You cannot send messages while silenced, restricted or banned.");

        /// <summary>
        /// "Can&#39;t edit deleted post."
        /// </summary>
        public static LocalisableString CommentUpdateDeleted => new TranslatableString(getKey(@"comment.update.deleted"), @"Can't edit deleted post.");

        /// <summary>
        /// "You cannot change your vote after the voting period for this contest has ended."
        /// </summary>
        public static LocalisableString ContestVotingOver => new TranslatableString(getKey(@"contest.voting_over"), @"You cannot change your vote after the voting period for this contest has ended.");

        /// <summary>
        /// "You have reached the entry limit for this contest"
        /// </summary>
        public static LocalisableString ContestEntryLimitReached => new TranslatableString(getKey(@"contest.entry.limit_reached"), @"You have reached the entry limit for this contest");

        /// <summary>
        /// "Thank you for your entries! Submissions have closed for this contest and voting will open soon."
        /// </summary>
        public static LocalisableString ContestEntryOver => new TranslatableString(getKey(@"contest.entry.over"), @"Thank you for your entries! Submissions have closed for this contest and voting will open soon.");

        /// <summary>
        /// "No permission to moderate this forum."
        /// </summary>
        public static LocalisableString ForumModerateNoPermission => new TranslatableString(getKey(@"forum.moderate.no_permission"), @"No permission to moderate this forum.");

        /// <summary>
        /// "Only last post can be deleted."
        /// </summary>
        public static LocalisableString ForumPostDeleteOnlyLastPost => new TranslatableString(getKey(@"forum.post.delete.only_last_post"), @"Only last post can be deleted.");

        /// <summary>
        /// "Can not delete post of a locked topic."
        /// </summary>
        public static LocalisableString ForumPostDeleteLocked => new TranslatableString(getKey(@"forum.post.delete.locked"), @"Can not delete post of a locked topic.");

        /// <summary>
        /// "Access to requested forum is required."
        /// </summary>
        public static LocalisableString ForumPostDeleteNoForumAccess => new TranslatableString(getKey(@"forum.post.delete.no_forum_access"), @"Access to requested forum is required.");

        /// <summary>
        /// "Only poster can delete the post."
        /// </summary>
        public static LocalisableString ForumPostDeleteNotOwner => new TranslatableString(getKey(@"forum.post.delete.not_owner"), @"Only poster can delete the post.");

        /// <summary>
        /// "Can not edit deleted post."
        /// </summary>
        public static LocalisableString ForumPostEditDeleted => new TranslatableString(getKey(@"forum.post.edit.deleted"), @"Can not edit deleted post.");

        /// <summary>
        /// "The post is locked from editing."
        /// </summary>
        public static LocalisableString ForumPostEditLocked => new TranslatableString(getKey(@"forum.post.edit.locked"), @"The post is locked from editing.");

        /// <summary>
        /// "Access to requested forum is required."
        /// </summary>
        public static LocalisableString ForumPostEditNoForumAccess => new TranslatableString(getKey(@"forum.post.edit.no_forum_access"), @"Access to requested forum is required.");

        /// <summary>
        /// "Only poster can edit the post."
        /// </summary>
        public static LocalisableString ForumPostEditNotOwner => new TranslatableString(getKey(@"forum.post.edit.not_owner"), @"Only poster can edit the post.");

        /// <summary>
        /// "Can not edit post of a locked topic."
        /// </summary>
        public static LocalisableString ForumPostEditTopicLocked => new TranslatableString(getKey(@"forum.post.edit.topic_locked"), @"Can not edit post of a locked topic.");

        /// <summary>
        /// "Try playing the game before posting on the forums, please! If you have a problem with playing, please post to the Help and Support forum."
        /// </summary>
        public static LocalisableString ForumPostStorePlayMore => new TranslatableString(getKey(@"forum.post.store.play_more"), @"Try playing the game before posting on the forums, please! If you have a problem with playing, please post to the Help and Support forum.");

        /// <summary>
        /// "You need to play the game more before you can make additional posts. If you&#39;re still having trouble playing the game, email support@ppy.sh"
        /// </summary>
        public static LocalisableString ForumPostStoreTooManyHelpPosts => new TranslatableString(getKey(@"forum.post.store.too_many_help_posts"), @"You need to play the game more before you can make additional posts. If you're still having trouble playing the game, email support@ppy.sh");

        /// <summary>
        /// "Please edit your last post instead of posting again."
        /// </summary>
        public static LocalisableString ForumTopicReplyDoublePost => new TranslatableString(getKey(@"forum.topic.reply.double_post"), @"Please edit your last post instead of posting again.");

        /// <summary>
        /// "Can not reply to a locked thread."
        /// </summary>
        public static LocalisableString ForumTopicReplyLocked => new TranslatableString(getKey(@"forum.topic.reply.locked"), @"Can not reply to a locked thread.");

        /// <summary>
        /// "Access to requested forum is required."
        /// </summary>
        public static LocalisableString ForumTopicReplyNoForumAccess => new TranslatableString(getKey(@"forum.topic.reply.no_forum_access"), @"Access to requested forum is required.");

        /// <summary>
        /// "No permission to reply."
        /// </summary>
        public static LocalisableString ForumTopicReplyNoPermission => new TranslatableString(getKey(@"forum.topic.reply.no_permission"), @"No permission to reply.");

        /// <summary>
        /// "Please sign in to reply."
        /// </summary>
        public static LocalisableString ForumTopicReplyUserRequireLogin => new TranslatableString(getKey(@"forum.topic.reply.user.require_login"), @"Please sign in to reply.");

        /// <summary>
        /// "Can&#39;t reply while restricted."
        /// </summary>
        public static LocalisableString ForumTopicReplyUserRestricted => new TranslatableString(getKey(@"forum.topic.reply.user.restricted"), @"Can't reply while restricted.");

        /// <summary>
        /// "Can&#39;t reply while silenced."
        /// </summary>
        public static LocalisableString ForumTopicReplyUserSilenced => new TranslatableString(getKey(@"forum.topic.reply.user.silenced"), @"Can't reply while silenced.");

        /// <summary>
        /// "Access to requested forum is required."
        /// </summary>
        public static LocalisableString ForumTopicStoreNoForumAccess => new TranslatableString(getKey(@"forum.topic.store.no_forum_access"), @"Access to requested forum is required.");

        /// <summary>
        /// "No permission to create new topic."
        /// </summary>
        public static LocalisableString ForumTopicStoreNoPermission => new TranslatableString(getKey(@"forum.topic.store.no_permission"), @"No permission to create new topic.");

        /// <summary>
        /// "Forum is closed and can not be posted to."
        /// </summary>
        public static LocalisableString ForumTopicStoreForumClosed => new TranslatableString(getKey(@"forum.topic.store.forum_closed"), @"Forum is closed and can not be posted to.");

        /// <summary>
        /// "Access to requested forum is required."
        /// </summary>
        public static LocalisableString ForumTopicVoteNoForumAccess => new TranslatableString(getKey(@"forum.topic.vote.no_forum_access"), @"Access to requested forum is required.");

        /// <summary>
        /// "Polling is over and can not be voted on anymore."
        /// </summary>
        public static LocalisableString ForumTopicVoteOver => new TranslatableString(getKey(@"forum.topic.vote.over"), @"Polling is over and can not be voted on anymore.");

        /// <summary>
        /// "You need to play more before voting on forum."
        /// </summary>
        public static LocalisableString ForumTopicVotePlayMore => new TranslatableString(getKey(@"forum.topic.vote.play_more"), @"You need to play more before voting on forum.");

        /// <summary>
        /// "Changing vote is not allowed."
        /// </summary>
        public static LocalisableString ForumTopicVoteVoted => new TranslatableString(getKey(@"forum.topic.vote.voted"), @"Changing vote is not allowed.");

        /// <summary>
        /// "Please sign in to vote."
        /// </summary>
        public static LocalisableString ForumTopicVoteUserRequireLogin => new TranslatableString(getKey(@"forum.topic.vote.user.require_login"), @"Please sign in to vote.");

        /// <summary>
        /// "Can&#39;t vote while restricted."
        /// </summary>
        public static LocalisableString ForumTopicVoteUserRestricted => new TranslatableString(getKey(@"forum.topic.vote.user.restricted"), @"Can't vote while restricted.");

        /// <summary>
        /// "Can&#39;t vote while silenced."
        /// </summary>
        public static LocalisableString ForumTopicVoteUserSilenced => new TranslatableString(getKey(@"forum.topic.vote.user.silenced"), @"Can't vote while silenced.");

        /// <summary>
        /// "Access to requested forum is required."
        /// </summary>
        public static LocalisableString ForumTopicWatchNoForumAccess => new TranslatableString(getKey(@"forum.topic.watch.no_forum_access"), @"Access to requested forum is required.");

        /// <summary>
        /// "Invalid cover specified."
        /// </summary>
        public static LocalisableString ForumTopicCoverEditUneditable => new TranslatableString(getKey(@"forum.topic_cover.edit.uneditable"), @"Invalid cover specified.");

        /// <summary>
        /// "Only owner can edit cover."
        /// </summary>
        public static LocalisableString ForumTopicCoverEditNotOwner => new TranslatableString(getKey(@"forum.topic_cover.edit.not_owner"), @"Only owner can edit cover.");

        /// <summary>
        /// "This forum does not accept topic covers."
        /// </summary>
        public static LocalisableString ForumTopicCoverStoreForumNotAllowed => new TranslatableString(getKey(@"forum.topic_cover.store.forum_not_allowed"), @"This forum does not accept topic covers.");

        /// <summary>
        /// "Only admin can view this forum."
        /// </summary>
        public static LocalisableString ForumViewAdminOnly => new TranslatableString(getKey(@"forum.view.admin_only"), @"Only admin can view this forum.");

        /// <summary>
        /// "User page is locked."
        /// </summary>
        public static LocalisableString UserPageEditLocked => new TranslatableString(getKey(@"user.page.edit.locked"), @"User page is locked.");

        /// <summary>
        /// "Can only edit own user page."
        /// </summary>
        public static LocalisableString UserPageEditNotOwner => new TranslatableString(getKey(@"user.page.edit.not_owner"), @"Can only edit own user page.");

        /// <summary>
        /// "osu!supporter tag is required."
        /// </summary>
        public static LocalisableString UserPageEditRequireSupporterTag => new TranslatableString(getKey(@"user.page.edit.require_supporter_tag"), @"osu!supporter tag is required.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}