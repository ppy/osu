// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class ForumStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Forum";

        /// <summary>
        /// "Pinned Topics"
        /// </summary>
        public static LocalisableString PinnedTopics => new TranslatableString(getKey(@"pinned_topics"), @"Pinned Topics");

        /// <summary>
        /// "it&#39;s dangerous to play alone."
        /// </summary>
        public static LocalisableString Slogan => new TranslatableString(getKey(@"slogan"), @"it's dangerous to play alone.");

        /// <summary>
        /// "Subforums"
        /// </summary>
        public static LocalisableString Subforums => new TranslatableString(getKey(@"subforums"), @"Subforums");

        /// <summary>
        /// "Forums"
        /// </summary>
        public static LocalisableString Title => new TranslatableString(getKey(@"title"), @"Forums");

        /// <summary>
        /// "Edit cover"
        /// </summary>
        public static LocalisableString CoversEdit => new TranslatableString(getKey(@"covers.edit"), @"Edit cover");

        /// <summary>
        /// "Set cover image"
        /// </summary>
        public static LocalisableString CoversCreateDefault => new TranslatableString(getKey(@"covers.create._"), @"Set cover image");

        /// <summary>
        /// "Upload cover"
        /// </summary>
        public static LocalisableString CoversCreateButton => new TranslatableString(getKey(@"covers.create.button"), @"Upload cover");

        /// <summary>
        /// "Cover size should be at {0}. You can also drop your image here to upload."
        /// </summary>
        public static LocalisableString CoversCreateInfo(string dimensions) => new TranslatableString(getKey(@"covers.create.info"), @"Cover size should be at {0}. You can also drop your image here to upload.", dimensions);

        /// <summary>
        /// "Remove cover"
        /// </summary>
        public static LocalisableString CoversDestroyDefault => new TranslatableString(getKey(@"covers.destroy._"), @"Remove cover");

        /// <summary>
        /// "Are you sure you want to remove the cover image?"
        /// </summary>
        public static LocalisableString CoversDestroyConfirm => new TranslatableString(getKey(@"covers.destroy.confirm"), @"Are you sure you want to remove the cover image?");

        /// <summary>
        /// "Latest Post"
        /// </summary>
        public static LocalisableString ForumsLatestPost => new TranslatableString(getKey(@"forums.latest_post"), @"Latest Post");

        /// <summary>
        /// "Forum Index"
        /// </summary>
        public static LocalisableString ForumsIndexTitle => new TranslatableString(getKey(@"forums.index.title"), @"Forum Index");

        /// <summary>
        /// "No topics!"
        /// </summary>
        public static LocalisableString ForumsTopicsEmpty => new TranslatableString(getKey(@"forums.topics.empty"), @"No topics!");

        /// <summary>
        /// "Mark forum as read"
        /// </summary>
        public static LocalisableString MarkAsReadForum => new TranslatableString(getKey(@"mark_as_read.forum"), @"Mark forum as read");

        /// <summary>
        /// "Mark forums as read"
        /// </summary>
        public static LocalisableString MarkAsReadForums => new TranslatableString(getKey(@"mark_as_read.forums"), @"Mark forums as read");

        /// <summary>
        /// "Marking as read..."
        /// </summary>
        public static LocalisableString MarkAsReadBusy => new TranslatableString(getKey(@"mark_as_read.busy"), @"Marking as read...");

        /// <summary>
        /// "Really delete post?"
        /// </summary>
        public static LocalisableString PostConfirmDestroy => new TranslatableString(getKey(@"post.confirm_destroy"), @"Really delete post?");

        /// <summary>
        /// "Really restore post?"
        /// </summary>
        public static LocalisableString PostConfirmRestore => new TranslatableString(getKey(@"post.confirm_restore"), @"Really restore post?");

        /// <summary>
        /// "Last edited by {0} {1}, edited {2} time in total.|Last edited by {0} {1}, edited {2} times in total."
        /// </summary>
        public static LocalisableString PostEdited(string user, string when, string countDelimited) => new TranslatableString(getKey(@"post.edited"), @"Last edited by {0} {1}, edited {2} time in total.|Last edited by {0} {1}, edited {2} times in total.", user, when, countDelimited);

        /// <summary>
        /// "posted {0}"
        /// </summary>
        public static LocalisableString PostPostedAt(string when) => new TranslatableString(getKey(@"post.posted_at"), @"posted {0}", when);

        /// <summary>
        /// "posted by {0}"
        /// </summary>
        public static LocalisableString PostPostedBy(string username) => new TranslatableString(getKey(@"post.posted_by"), @"posted by {0}", username);

        /// <summary>
        /// "Delete post"
        /// </summary>
        public static LocalisableString PostActionsDestroy => new TranslatableString(getKey(@"post.actions.destroy"), @"Delete post");

        /// <summary>
        /// "Edit post"
        /// </summary>
        public static LocalisableString PostActionsEdit => new TranslatableString(getKey(@"post.actions.edit"), @"Edit post");

        /// <summary>
        /// "Report post"
        /// </summary>
        public static LocalisableString PostActionsReport => new TranslatableString(getKey(@"post.actions.report"), @"Report post");

        /// <summary>
        /// "Restore post"
        /// </summary>
        public static LocalisableString PostActionsRestore => new TranslatableString(getKey(@"post.actions.restore"), @"Restore post");

        /// <summary>
        /// "New reply"
        /// </summary>
        public static LocalisableString PostCreateTitleReply => new TranslatableString(getKey(@"post.create.title.reply"), @"New reply");

        /// <summary>
        /// "{0} post|{0} posts"
        /// </summary>
        public static LocalisableString PostInfoPostCount(string countDelimited) => new TranslatableString(getKey(@"post.info.post_count"), @"{0} post|{0} posts", countDelimited);

        /// <summary>
        /// "Topic Starter"
        /// </summary>
        public static LocalisableString PostInfoTopicStarter => new TranslatableString(getKey(@"post.info.topic_starter"), @"Topic Starter");

        /// <summary>
        /// "Go to post"
        /// </summary>
        public static LocalisableString SearchGoToPost => new TranslatableString(getKey(@"search.go_to_post"), @"Go to post");

        /// <summary>
        /// "enter post number"
        /// </summary>
        public static LocalisableString SearchPostNumberInput => new TranslatableString(getKey(@"search.post_number_input"), @"enter post number");

        /// <summary>
        /// "{0} posts total"
        /// </summary>
        public static LocalisableString SearchTotalPosts(string postsCount) => new TranslatableString(getKey(@"search.total_posts"), @"{0} posts total", postsCount);

        /// <summary>
        /// "Really delete topic?"
        /// </summary>
        public static LocalisableString TopicConfirmDestroy => new TranslatableString(getKey(@"topic.confirm_destroy"), @"Really delete topic?");

        /// <summary>
        /// "Really restore topic?"
        /// </summary>
        public static LocalisableString TopicConfirmRestore => new TranslatableString(getKey(@"topic.confirm_restore"), @"Really restore topic?");

        /// <summary>
        /// "deleted topic"
        /// </summary>
        public static LocalisableString TopicDeleted => new TranslatableString(getKey(@"topic.deleted"), @"deleted topic");

        /// <summary>
        /// "view latest post"
        /// </summary>
        public static LocalisableString TopicGoToLatest => new TranslatableString(getKey(@"topic.go_to_latest"), @"view latest post");

        /// <summary>
        /// "You have replied to this topic"
        /// </summary>
        public static LocalisableString TopicHasReplied => new TranslatableString(getKey(@"topic.has_replied"), @"You have replied to this topic");

        /// <summary>
        /// "in {0}"
        /// </summary>
        public static LocalisableString TopicInForum(string forum) => new TranslatableString(getKey(@"topic.in_forum"), @"in {0}", forum);

        /// <summary>
        /// "{0} by {1}"
        /// </summary>
        public static LocalisableString TopicLatestPost(string when, string user) => new TranslatableString(getKey(@"topic.latest_post"), @"{0} by {1}", when, user);

        /// <summary>
        /// "last reply by {0}"
        /// </summary>
        public static LocalisableString TopicLatestReplyBy(string user) => new TranslatableString(getKey(@"topic.latest_reply_by"), @"last reply by {0}", user);

        /// <summary>
        /// "New topic"
        /// </summary>
        public static LocalisableString TopicNewTopic => new TranslatableString(getKey(@"topic.new_topic"), @"New topic");

        /// <summary>
        /// "Sign in to post new topic"
        /// </summary>
        public static LocalisableString TopicNewTopicLogin => new TranslatableString(getKey(@"topic.new_topic_login"), @"Sign in to post new topic");

        /// <summary>
        /// "Post"
        /// </summary>
        public static LocalisableString TopicPostReply => new TranslatableString(getKey(@"topic.post_reply"), @"Post");

        /// <summary>
        /// "Type here to reply"
        /// </summary>
        public static LocalisableString TopicReplyBoxPlaceholder => new TranslatableString(getKey(@"topic.reply_box_placeholder"), @"Type here to reply");

        /// <summary>
        /// "Re"
        /// </summary>
        public static LocalisableString TopicReplyTitlePrefix => new TranslatableString(getKey(@"topic.reply_title_prefix"), @"Re");

        /// <summary>
        /// "by {0}"
        /// </summary>
        public static LocalisableString TopicStartedBy(string user) => new TranslatableString(getKey(@"topic.started_by"), @"by {0}", user);

        /// <summary>
        /// "started by {0}"
        /// </summary>
        public static LocalisableString TopicStartedByVerbose(string user) => new TranslatableString(getKey(@"topic.started_by_verbose"), @"started by {0}", user);

        /// <summary>
        /// "Delete topic"
        /// </summary>
        public static LocalisableString TopicActionsDestroy => new TranslatableString(getKey(@"topic.actions.destroy"), @"Delete topic");

        /// <summary>
        /// "Restore topic"
        /// </summary>
        public static LocalisableString TopicActionsRestore => new TranslatableString(getKey(@"topic.actions.restore"), @"Restore topic");

        /// <summary>
        /// "Close"
        /// </summary>
        public static LocalisableString TopicCreateClose => new TranslatableString(getKey(@"topic.create.close"), @"Close");

        /// <summary>
        /// "Preview"
        /// </summary>
        public static LocalisableString TopicCreatePreview => new TranslatableString(getKey(@"topic.create.preview"), @"Preview");

        /// <summary>
        /// "Write"
        /// </summary>
        public static LocalisableString TopicCreatePreviewHide => new TranslatableString(getKey(@"topic.create.preview_hide"), @"Write");

        /// <summary>
        /// "Post"
        /// </summary>
        public static LocalisableString TopicCreateSubmit => new TranslatableString(getKey(@"topic.create.submit"), @"Post");

        /// <summary>
        /// "This topic has been inactive for a while. Only post here if you have a specific reason to do so."
        /// </summary>
        public static LocalisableString TopicCreateNecropostDefault => new TranslatableString(getKey(@"topic.create.necropost.default"), @"This topic has been inactive for a while. Only post here if you have a specific reason to do so.");

        /// <summary>
        /// "This topic has been inactive for a while. If you don&#39;t have a specific reason to post here, please {0} instead."
        /// </summary>
        public static LocalisableString TopicCreateNecropostNewTopicDefault(string create) => new TranslatableString(getKey(@"topic.create.necropost.new_topic._"), @"This topic has been inactive for a while. If you don't have a specific reason to post here, please {0} instead.", create);

        /// <summary>
        /// "create a new topic"
        /// </summary>
        public static LocalisableString TopicCreateNecropostNewTopicCreate => new TranslatableString(getKey(@"topic.create.necropost.new_topic.create"), @"create a new topic");

        /// <summary>
        /// "Type post content here"
        /// </summary>
        public static LocalisableString TopicCreatePlaceholderBody => new TranslatableString(getKey(@"topic.create.placeholder.body"), @"Type post content here");

        /// <summary>
        /// "Click here to set title"
        /// </summary>
        public static LocalisableString TopicCreatePlaceholderTitle => new TranslatableString(getKey(@"topic.create.placeholder.title"), @"Click here to set title");

        /// <summary>
        /// "click to enter specific post number"
        /// </summary>
        public static LocalisableString TopicJumpEnter => new TranslatableString(getKey(@"topic.jump.enter"), @"click to enter specific post number");

        /// <summary>
        /// "go to first post"
        /// </summary>
        public static LocalisableString TopicJumpFirst => new TranslatableString(getKey(@"topic.jump.first"), @"go to first post");

        /// <summary>
        /// "go to last post"
        /// </summary>
        public static LocalisableString TopicJumpLast => new TranslatableString(getKey(@"topic.jump.last"), @"go to last post");

        /// <summary>
        /// "skip next 10 posts"
        /// </summary>
        public static LocalisableString TopicJumpNext => new TranslatableString(getKey(@"topic.jump.next"), @"skip next 10 posts");

        /// <summary>
        /// "go back 10 posts"
        /// </summary>
        public static LocalisableString TopicJumpPrevious => new TranslatableString(getKey(@"topic.jump.previous"), @"go back 10 posts");

        /// <summary>
        /// "Cancel"
        /// </summary>
        public static LocalisableString TopicPostEditCancel => new TranslatableString(getKey(@"topic.post_edit.cancel"), @"Cancel");

        /// <summary>
        /// "Save"
        /// </summary>
        public static LocalisableString TopicPostEditPost => new TranslatableString(getKey(@"topic.post_edit.post"), @"Save");

        /// <summary>
        /// "forum topic watchlist"
        /// </summary>
        public static LocalisableString TopicWatchesIndexTitleCompact => new TranslatableString(getKey(@"topic_watches.index.title_compact"), @"forum topic watchlist");

        /// <summary>
        /// "Topics subscribed"
        /// </summary>
        public static LocalisableString TopicWatchesIndexBoxTotal => new TranslatableString(getKey(@"topic_watches.index.box.total"), @"Topics subscribed");

        /// <summary>
        /// "Topics with new replies"
        /// </summary>
        public static LocalisableString TopicWatchesIndexBoxUnread => new TranslatableString(getKey(@"topic_watches.index.box.unread"), @"Topics with new replies");

        /// <summary>
        /// "You subscribed to {0} topics."
        /// </summary>
        public static LocalisableString TopicWatchesIndexInfoTotal(string total) => new TranslatableString(getKey(@"topic_watches.index.info.total"), @"You subscribed to {0} topics.", total);

        /// <summary>
        /// "You have {0} unread replies to subscribed topics."
        /// </summary>
        public static LocalisableString TopicWatchesIndexInfoUnread(string unread) => new TranslatableString(getKey(@"topic_watches.index.info.unread"), @"You have {0} unread replies to subscribed topics.", unread);

        /// <summary>
        /// "Unsubscribe from topic?"
        /// </summary>
        public static LocalisableString TopicWatchesTopicButtonsRemoveConfirmation => new TranslatableString(getKey(@"topic_watches.topic_buttons.remove.confirmation"), @"Unsubscribe from topic?");

        /// <summary>
        /// "Unsubscribe"
        /// </summary>
        public static LocalisableString TopicWatchesTopicButtonsRemoveTitle => new TranslatableString(getKey(@"topic_watches.topic_buttons.remove.title"), @"Unsubscribe");

        /// <summary>
        /// "Topics"
        /// </summary>
        public static LocalisableString TopicsDefault => new TranslatableString(getKey(@"topics._"), @"Topics");

        /// <summary>
        /// "Sign in to Reply"
        /// </summary>
        public static LocalisableString TopicsActionsLoginReply => new TranslatableString(getKey(@"topics.actions.login_reply"), @"Sign in to Reply");

        /// <summary>
        /// "Reply"
        /// </summary>
        public static LocalisableString TopicsActionsReply => new TranslatableString(getKey(@"topics.actions.reply"), @"Reply");

        /// <summary>
        /// "Quote post for reply"
        /// </summary>
        public static LocalisableString TopicsActionsReplyWithQuote => new TranslatableString(getKey(@"topics.actions.reply_with_quote"), @"Quote post for reply");

        /// <summary>
        /// "Search"
        /// </summary>
        public static LocalisableString TopicsActionsSearch => new TranslatableString(getKey(@"topics.actions.search"), @"Search");

        /// <summary>
        /// "Poll Creation"
        /// </summary>
        public static LocalisableString TopicsCreateCreatePoll => new TranslatableString(getKey(@"topics.create.create_poll"), @"Poll Creation");

        /// <summary>
        /// "Post Preview"
        /// </summary>
        public static LocalisableString TopicsCreatePreview => new TranslatableString(getKey(@"topics.create.preview"), @"Post Preview");

        /// <summary>
        /// "Create a poll"
        /// </summary>
        public static LocalisableString TopicsCreateCreatePollButtonAdd => new TranslatableString(getKey(@"topics.create.create_poll_button.add"), @"Create a poll");

        /// <summary>
        /// "Cancel creating a poll"
        /// </summary>
        public static LocalisableString TopicsCreateCreatePollButtonRemove => new TranslatableString(getKey(@"topics.create.create_poll_button.remove"), @"Cancel creating a poll");

        /// <summary>
        /// "Hide the results of the poll."
        /// </summary>
        public static LocalisableString TopicsCreatePollHideResults => new TranslatableString(getKey(@"topics.create.poll.hide_results"), @"Hide the results of the poll.");

        /// <summary>
        /// "They will be shown only after the poll concludes."
        /// </summary>
        public static LocalisableString TopicsCreatePollHideResultsInfo => new TranslatableString(getKey(@"topics.create.poll.hide_results_info"), @"They will be shown only after the poll concludes.");

        /// <summary>
        /// "Run poll for"
        /// </summary>
        public static LocalisableString TopicsCreatePollLength => new TranslatableString(getKey(@"topics.create.poll.length"), @"Run poll for");

        /// <summary>
        /// "days"
        /// </summary>
        public static LocalisableString TopicsCreatePollLengthDaysSuffix => new TranslatableString(getKey(@"topics.create.poll.length_days_suffix"), @"days");

        /// <summary>
        /// "Leave blank for a never ending poll"
        /// </summary>
        public static LocalisableString TopicsCreatePollLengthInfo => new TranslatableString(getKey(@"topics.create.poll.length_info"), @"Leave blank for a never ending poll");

        /// <summary>
        /// "Options per user"
        /// </summary>
        public static LocalisableString TopicsCreatePollMaxOptions => new TranslatableString(getKey(@"topics.create.poll.max_options"), @"Options per user");

        /// <summary>
        /// "This is the number of options each user may select when voting."
        /// </summary>
        public static LocalisableString TopicsCreatePollMaxOptionsInfo => new TranslatableString(getKey(@"topics.create.poll.max_options_info"), @"This is the number of options each user may select when voting.");

        /// <summary>
        /// "Options"
        /// </summary>
        public static LocalisableString TopicsCreatePollOptions => new TranslatableString(getKey(@"topics.create.poll.options"), @"Options");

        /// <summary>
        /// "Place each options on a new line. You may enter up to 10 options."
        /// </summary>
        public static LocalisableString TopicsCreatePollOptionsInfo => new TranslatableString(getKey(@"topics.create.poll.options_info"), @"Place each options on a new line. You may enter up to 10 options.");

        /// <summary>
        /// "Question"
        /// </summary>
        public static LocalisableString TopicsCreatePollTitle => new TranslatableString(getKey(@"topics.create.poll.title"), @"Question");

        /// <summary>
        /// "Allow re-voting."
        /// </summary>
        public static LocalisableString TopicsCreatePollVoteChange => new TranslatableString(getKey(@"topics.create.poll.vote_change"), @"Allow re-voting.");

        /// <summary>
        /// "If enabled, users are able to change their vote."
        /// </summary>
        public static LocalisableString TopicsCreatePollVoteChangeInfo => new TranslatableString(getKey(@"topics.create.poll.vote_change_info"), @"If enabled, users are able to change their vote.");

        /// <summary>
        /// "Edit title"
        /// </summary>
        public static LocalisableString TopicsEditTitleStart => new TranslatableString(getKey(@"topics.edit_title.start"), @"Edit title");

        /// <summary>
        /// "star priority"
        /// </summary>
        public static LocalisableString TopicsIndexFeatureVotes => new TranslatableString(getKey(@"topics.index.feature_votes"), @"star priority");

        /// <summary>
        /// "replies"
        /// </summary>
        public static LocalisableString TopicsIndexReplies => new TranslatableString(getKey(@"topics.index.replies"), @"replies");

        /// <summary>
        /// "views"
        /// </summary>
        public static LocalisableString TopicsIndexViews => new TranslatableString(getKey(@"topics.index.views"), @"views");

        /// <summary>
        /// "Remove &quot;added&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagAddedTo0 => new TranslatableString(getKey(@"topics.issue_tag_added.to_0"), @"Remove ""added"" tag");

        /// <summary>
        /// "Removed &quot;added&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagAddedTo0Done => new TranslatableString(getKey(@"topics.issue_tag_added.to_0_done"), @"Removed ""added"" tag");

        /// <summary>
        /// "Add &quot;added&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagAddedTo1 => new TranslatableString(getKey(@"topics.issue_tag_added.to_1"), @"Add ""added"" tag");

        /// <summary>
        /// "Added &quot;added&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagAddedTo1Done => new TranslatableString(getKey(@"topics.issue_tag_added.to_1_done"), @"Added ""added"" tag");

        /// <summary>
        /// "Remove &quot;assigned&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagAssignedTo0 => new TranslatableString(getKey(@"topics.issue_tag_assigned.to_0"), @"Remove ""assigned"" tag");

        /// <summary>
        /// "Removed &quot;assigned&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagAssignedTo0Done => new TranslatableString(getKey(@"topics.issue_tag_assigned.to_0_done"), @"Removed ""assigned"" tag");

        /// <summary>
        /// "Add &quot;assigned&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagAssignedTo1 => new TranslatableString(getKey(@"topics.issue_tag_assigned.to_1"), @"Add ""assigned"" tag");

        /// <summary>
        /// "Added &quot;assigned&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagAssignedTo1Done => new TranslatableString(getKey(@"topics.issue_tag_assigned.to_1_done"), @"Added ""assigned"" tag");

        /// <summary>
        /// "Remove &quot;confirmed&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagConfirmedTo0 => new TranslatableString(getKey(@"topics.issue_tag_confirmed.to_0"), @"Remove ""confirmed"" tag");

        /// <summary>
        /// "Removed &quot;confirmed&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagConfirmedTo0Done => new TranslatableString(getKey(@"topics.issue_tag_confirmed.to_0_done"), @"Removed ""confirmed"" tag");

        /// <summary>
        /// "Add &quot;confirmed&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagConfirmedTo1 => new TranslatableString(getKey(@"topics.issue_tag_confirmed.to_1"), @"Add ""confirmed"" tag");

        /// <summary>
        /// "Added &quot;confirmed&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagConfirmedTo1Done => new TranslatableString(getKey(@"topics.issue_tag_confirmed.to_1_done"), @"Added ""confirmed"" tag");

        /// <summary>
        /// "Remove &quot;duplicate&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagDuplicateTo0 => new TranslatableString(getKey(@"topics.issue_tag_duplicate.to_0"), @"Remove ""duplicate"" tag");

        /// <summary>
        /// "Removed &quot;duplicate&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagDuplicateTo0Done => new TranslatableString(getKey(@"topics.issue_tag_duplicate.to_0_done"), @"Removed ""duplicate"" tag");

        /// <summary>
        /// "Add &quot;duplicate&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagDuplicateTo1 => new TranslatableString(getKey(@"topics.issue_tag_duplicate.to_1"), @"Add ""duplicate"" tag");

        /// <summary>
        /// "Added &quot;duplicate&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagDuplicateTo1Done => new TranslatableString(getKey(@"topics.issue_tag_duplicate.to_1_done"), @"Added ""duplicate"" tag");

        /// <summary>
        /// "Remove &quot;invalid&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagInvalidTo0 => new TranslatableString(getKey(@"topics.issue_tag_invalid.to_0"), @"Remove ""invalid"" tag");

        /// <summary>
        /// "Removed &quot;invalid&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagInvalidTo0Done => new TranslatableString(getKey(@"topics.issue_tag_invalid.to_0_done"), @"Removed ""invalid"" tag");

        /// <summary>
        /// "Add &quot;invalid&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagInvalidTo1 => new TranslatableString(getKey(@"topics.issue_tag_invalid.to_1"), @"Add ""invalid"" tag");

        /// <summary>
        /// "Added &quot;invalid&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagInvalidTo1Done => new TranslatableString(getKey(@"topics.issue_tag_invalid.to_1_done"), @"Added ""invalid"" tag");

        /// <summary>
        /// "Remove &quot;resolved&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagResolvedTo0 => new TranslatableString(getKey(@"topics.issue_tag_resolved.to_0"), @"Remove ""resolved"" tag");

        /// <summary>
        /// "Removed &quot;resolved&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagResolvedTo0Done => new TranslatableString(getKey(@"topics.issue_tag_resolved.to_0_done"), @"Removed ""resolved"" tag");

        /// <summary>
        /// "Add &quot;resolved&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagResolvedTo1 => new TranslatableString(getKey(@"topics.issue_tag_resolved.to_1"), @"Add ""resolved"" tag");

        /// <summary>
        /// "Added &quot;resolved&quot; tag"
        /// </summary>
        public static LocalisableString TopicsIssueTagResolvedTo1Done => new TranslatableString(getKey(@"topics.issue_tag_resolved.to_1_done"), @"Added ""resolved"" tag");

        /// <summary>
        /// "This topic is locked and can not be replied to"
        /// </summary>
        public static LocalisableString TopicsLockIsLocked => new TranslatableString(getKey(@"topics.lock.is_locked"), @"This topic is locked and can not be replied to");

        /// <summary>
        /// "Unlock topic"
        /// </summary>
        public static LocalisableString TopicsLockTo0 => new TranslatableString(getKey(@"topics.lock.to_0"), @"Unlock topic");

        /// <summary>
        /// "Unlock topic?"
        /// </summary>
        public static LocalisableString TopicsLockTo0Confirm => new TranslatableString(getKey(@"topics.lock.to_0_confirm"), @"Unlock topic?");

        /// <summary>
        /// "Topic has been unlocked"
        /// </summary>
        public static LocalisableString TopicsLockTo0Done => new TranslatableString(getKey(@"topics.lock.to_0_done"), @"Topic has been unlocked");

        /// <summary>
        /// "Lock topic"
        /// </summary>
        public static LocalisableString TopicsLockTo1 => new TranslatableString(getKey(@"topics.lock.to_1"), @"Lock topic");

        /// <summary>
        /// "Lock topic?"
        /// </summary>
        public static LocalisableString TopicsLockTo1Confirm => new TranslatableString(getKey(@"topics.lock.to_1_confirm"), @"Lock topic?");

        /// <summary>
        /// "Topic has been locked"
        /// </summary>
        public static LocalisableString TopicsLockTo1Done => new TranslatableString(getKey(@"topics.lock.to_1_done"), @"Topic has been locked");

        /// <summary>
        /// "Move to another forum"
        /// </summary>
        public static LocalisableString TopicsModerateMoveTitle => new TranslatableString(getKey(@"topics.moderate_move.title"), @"Move to another forum");

        /// <summary>
        /// "Unpin topic"
        /// </summary>
        public static LocalisableString TopicsModeratePinTo0 => new TranslatableString(getKey(@"topics.moderate_pin.to_0"), @"Unpin topic");

        /// <summary>
        /// "Unpin topic?"
        /// </summary>
        public static LocalisableString TopicsModeratePinTo0Confirm => new TranslatableString(getKey(@"topics.moderate_pin.to_0_confirm"), @"Unpin topic?");

        /// <summary>
        /// "Topic has been unpinned"
        /// </summary>
        public static LocalisableString TopicsModeratePinTo0Done => new TranslatableString(getKey(@"topics.moderate_pin.to_0_done"), @"Topic has been unpinned");

        /// <summary>
        /// "Pin topic"
        /// </summary>
        public static LocalisableString TopicsModeratePinTo1 => new TranslatableString(getKey(@"topics.moderate_pin.to_1"), @"Pin topic");

        /// <summary>
        /// "Pin topic?"
        /// </summary>
        public static LocalisableString TopicsModeratePinTo1Confirm => new TranslatableString(getKey(@"topics.moderate_pin.to_1_confirm"), @"Pin topic?");

        /// <summary>
        /// "Topic has been pinned"
        /// </summary>
        public static LocalisableString TopicsModeratePinTo1Done => new TranslatableString(getKey(@"topics.moderate_pin.to_1_done"), @"Topic has been pinned");

        /// <summary>
        /// "Pin topic and mark as announcement"
        /// </summary>
        public static LocalisableString TopicsModeratePinTo2 => new TranslatableString(getKey(@"topics.moderate_pin.to_2"), @"Pin topic and mark as announcement");

        /// <summary>
        /// "Pin topic and mark as announcement?"
        /// </summary>
        public static LocalisableString TopicsModeratePinTo2Confirm => new TranslatableString(getKey(@"topics.moderate_pin.to_2_confirm"), @"Pin topic and mark as announcement?");

        /// <summary>
        /// "Topic has been pinned and marked as announcement"
        /// </summary>
        public static LocalisableString TopicsModeratePinTo2Done => new TranslatableString(getKey(@"topics.moderate_pin.to_2_done"), @"Topic has been pinned and marked as announcement");

        /// <summary>
        /// "Show deleted posts"
        /// </summary>
        public static LocalisableString TopicsModerateToggleDeletedShow => new TranslatableString(getKey(@"topics.moderate_toggle_deleted.show"), @"Show deleted posts");

        /// <summary>
        /// "Hide deleted posts"
        /// </summary>
        public static LocalisableString TopicsModerateToggleDeletedHide => new TranslatableString(getKey(@"topics.moderate_toggle_deleted.hide"), @"Hide deleted posts");

        /// <summary>
        /// "Deleted Posts"
        /// </summary>
        public static LocalisableString TopicsShowDeletedPosts => new TranslatableString(getKey(@"topics.show.deleted-posts"), @"Deleted Posts");

        /// <summary>
        /// "Total Posts"
        /// </summary>
        public static LocalisableString TopicsShowTotalPosts => new TranslatableString(getKey(@"topics.show.total_posts"), @"Total Posts");

        /// <summary>
        /// "Current Priority: +{0}"
        /// </summary>
        public static LocalisableString TopicsShowFeatureVoteCurrent(string count) => new TranslatableString(getKey(@"topics.show.feature_vote.current"), @"Current Priority: +{0}", count);

        /// <summary>
        /// "Promote this request"
        /// </summary>
        public static LocalisableString TopicsShowFeatureVoteDo => new TranslatableString(getKey(@"topics.show.feature_vote.do"), @"Promote this request");

        /// <summary>
        /// "This is a {0}. Feature requests can be voted up by {1}."
        /// </summary>
        public static LocalisableString TopicsShowFeatureVoteInfoDefault(string featureRequest, string supporters) => new TranslatableString(getKey(@"topics.show.feature_vote.info._"), @"This is a {0}. Feature requests can be voted up by {1}.", featureRequest, supporters);

        /// <summary>
        /// "feature request"
        /// </summary>
        public static LocalisableString TopicsShowFeatureVoteInfoFeatureRequest => new TranslatableString(getKey(@"topics.show.feature_vote.info.feature_request"), @"feature request");

        /// <summary>
        /// "supporters"
        /// </summary>
        public static LocalisableString TopicsShowFeatureVoteInfoSupporters => new TranslatableString(getKey(@"topics.show.feature_vote.info.supporters"), @"supporters");

        /// <summary>
        /// "{0} no votes|{1} {0} vote|[2,*] {0} votes"
        /// </summary>
        public static LocalisableString TopicsShowFeatureVoteUserCount(string countDelimited) => new TranslatableString(getKey(@"topics.show.feature_vote.user.count"), @"{0} no votes|{1} {0} vote|[2,*] {0} votes", countDelimited);

        /// <summary>
        /// "You have {0} remaining."
        /// </summary>
        public static LocalisableString TopicsShowFeatureVoteUserCurrent(string votes) => new TranslatableString(getKey(@"topics.show.feature_vote.user.current"), @"You have {0} remaining.", votes);

        /// <summary>
        /// "You don&#39;t have any more votes remaining"
        /// </summary>
        public static LocalisableString TopicsShowFeatureVoteUserNotEnough => new TranslatableString(getKey(@"topics.show.feature_vote.user.not_enough"), @"You don't have any more votes remaining");

        /// <summary>
        /// "Poll Edit"
        /// </summary>
        public static LocalisableString TopicsShowPollEdit => new TranslatableString(getKey(@"topics.show.poll.edit"), @"Poll Edit");

        /// <summary>
        /// "Editing a poll will remove the current results!"
        /// </summary>
        public static LocalisableString TopicsShowPollEditWarning => new TranslatableString(getKey(@"topics.show.poll.edit_warning"), @"Editing a poll will remove the current results!");

        /// <summary>
        /// "Vote"
        /// </summary>
        public static LocalisableString TopicsShowPollVote => new TranslatableString(getKey(@"topics.show.poll.vote"), @"Vote");

        /// <summary>
        /// "Change vote"
        /// </summary>
        public static LocalisableString TopicsShowPollButtonChangeVote => new TranslatableString(getKey(@"topics.show.poll.button.change_vote"), @"Change vote");

        /// <summary>
        /// "Edit poll"
        /// </summary>
        public static LocalisableString TopicsShowPollButtonEdit => new TranslatableString(getKey(@"topics.show.poll.button.edit"), @"Edit poll");

        /// <summary>
        /// "Skip to results"
        /// </summary>
        public static LocalisableString TopicsShowPollButtonViewResults => new TranslatableString(getKey(@"topics.show.poll.button.view_results"), @"Skip to results");

        /// <summary>
        /// "Vote"
        /// </summary>
        public static LocalisableString TopicsShowPollButtonVote => new TranslatableString(getKey(@"topics.show.poll.button.vote"), @"Vote");

        /// <summary>
        /// "Polling will end at {0}"
        /// </summary>
        public static LocalisableString TopicsShowPollDetailEndTime(string time) => new TranslatableString(getKey(@"topics.show.poll.detail.end_time"), @"Polling will end at {0}", time);

        /// <summary>
        /// "Polling ended {0}"
        /// </summary>
        public static LocalisableString TopicsShowPollDetailEnded(string time) => new TranslatableString(getKey(@"topics.show.poll.detail.ended"), @"Polling ended {0}", time);

        /// <summary>
        /// "Results will be shown after polling ends."
        /// </summary>
        public static LocalisableString TopicsShowPollDetailResultsHidden => new TranslatableString(getKey(@"topics.show.poll.detail.results_hidden"), @"Results will be shown after polling ends.");

        /// <summary>
        /// "Total votes: {0}"
        /// </summary>
        public static LocalisableString TopicsShowPollDetailTotal(string count) => new TranslatableString(getKey(@"topics.show.poll.detail.total"), @"Total votes: {0}", count);

        /// <summary>
        /// "Not bookmarked"
        /// </summary>
        public static LocalisableString TopicsWatchToNotWatching => new TranslatableString(getKey(@"topics.watch.to_not_watching"), @"Not bookmarked");

        /// <summary>
        /// "Bookmark"
        /// </summary>
        public static LocalisableString TopicsWatchToWatching => new TranslatableString(getKey(@"topics.watch.to_watching"), @"Bookmark");

        /// <summary>
        /// "Bookmark with notification"
        /// </summary>
        public static LocalisableString TopicsWatchToWatchingMail => new TranslatableString(getKey(@"topics.watch.to_watching_mail"), @"Bookmark with notification");

        /// <summary>
        /// "Notification is enabled. Click to disable"
        /// </summary>
        public static LocalisableString TopicsWatchTooltipMailDisable => new TranslatableString(getKey(@"topics.watch.tooltip_mail_disable"), @"Notification is enabled. Click to disable");

        /// <summary>
        /// "Notification is disabled. Click to enable"
        /// </summary>
        public static LocalisableString TopicsWatchTooltipMailEnable => new TranslatableString(getKey(@"topics.watch.tooltip_mail_enable"), @"Notification is disabled. Click to enable");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}