// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class BeatmapDiscussionsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.BeatmapDiscussions";

        /// <summary>
        /// "Must be signed in to edit."
        /// </summary>
        public static LocalisableString AuthorizationsUpdateNullUser => new TranslatableString(getKey(@"authorizations.update.null_user"), @"Must be signed in to edit.");

        /// <summary>
        /// "System-generated post can not be edited."
        /// </summary>
        public static LocalisableString AuthorizationsUpdateSystemGenerated => new TranslatableString(getKey(@"authorizations.update.system_generated"), @"System-generated post can not be edited.");

        /// <summary>
        /// "Must be owner of the post to edit."
        /// </summary>
        public static LocalisableString AuthorizationsUpdateWrongUser => new TranslatableString(getKey(@"authorizations.update.wrong_user"), @"Must be owner of the post to edit.");

        /// <summary>
        /// "Nothing has happened... yet."
        /// </summary>
        public static LocalisableString EventsEmpty => new TranslatableString(getKey(@"events.empty"), @"Nothing has happened... yet.");

        /// <summary>
        /// "deleted"
        /// </summary>
        public static LocalisableString IndexDeletedBeatmap => new TranslatableString(getKey(@"index.deleted_beatmap"), @"deleted");

        /// <summary>
        /// "No discussions matching that search criteria were found."
        /// </summary>
        public static LocalisableString IndexNoneFound => new TranslatableString(getKey(@"index.none_found"), @"No discussions matching that search criteria were found.");

        /// <summary>
        /// "Beatmap Discussions"
        /// </summary>
        public static LocalisableString IndexTitle => new TranslatableString(getKey(@"index.title"), @"Beatmap Discussions");

        /// <summary>
        /// "Search"
        /// </summary>
        public static LocalisableString IndexFormDefault => new TranslatableString(getKey(@"index.form._"), @"Search");

        /// <summary>
        /// "Include deleted discussions"
        /// </summary>
        public static LocalisableString IndexFormDeleted => new TranslatableString(getKey(@"index.form.deleted"), @"Include deleted discussions");

        /// <summary>
        /// "Beatmap mode"
        /// </summary>
        public static LocalisableString IndexFormMode => new TranslatableString(getKey(@"index.form.mode"), @"Beatmap mode");

        /// <summary>
        /// "Show only unresolved discussions"
        /// </summary>
        public static LocalisableString IndexFormOnlyUnresolved => new TranslatableString(getKey(@"index.form.only_unresolved"), @"Show only unresolved discussions");

        /// <summary>
        /// "Message types"
        /// </summary>
        public static LocalisableString IndexFormTypes => new TranslatableString(getKey(@"index.form.types"), @"Message types");

        /// <summary>
        /// "Username"
        /// </summary>
        public static LocalisableString IndexFormUsername => new TranslatableString(getKey(@"index.form.username"), @"Username");

        /// <summary>
        /// "Beatmap Status"
        /// </summary>
        public static LocalisableString IndexFormBeatmapsetStatusDefault => new TranslatableString(getKey(@"index.form.beatmapset_status._"), @"Beatmap Status");

        /// <summary>
        /// "All"
        /// </summary>
        public static LocalisableString IndexFormBeatmapsetStatusAll => new TranslatableString(getKey(@"index.form.beatmapset_status.all"), @"All");

        /// <summary>
        /// "Disqualified"
        /// </summary>
        public static LocalisableString IndexFormBeatmapsetStatusDisqualified => new TranslatableString(getKey(@"index.form.beatmapset_status.disqualified"), @"Disqualified");

        /// <summary>
        /// "Never Qualified"
        /// </summary>
        public static LocalisableString IndexFormBeatmapsetStatusNeverQualified => new TranslatableString(getKey(@"index.form.beatmapset_status.never_qualified"), @"Never Qualified");

        /// <summary>
        /// "Qualified"
        /// </summary>
        public static LocalisableString IndexFormBeatmapsetStatusQualified => new TranslatableString(getKey(@"index.form.beatmapset_status.qualified"), @"Qualified");

        /// <summary>
        /// "Ranked"
        /// </summary>
        public static LocalisableString IndexFormBeatmapsetStatusRanked => new TranslatableString(getKey(@"index.form.beatmapset_status.ranked"), @"Ranked");

        /// <summary>
        /// "User"
        /// </summary>
        public static LocalisableString IndexFormUserLabel => new TranslatableString(getKey(@"index.form.user.label"), @"User");

        /// <summary>
        /// "Activities overview"
        /// </summary>
        public static LocalisableString IndexFormUserOverview => new TranslatableString(getKey(@"index.form.user.overview"), @"Activities overview");

        /// <summary>
        /// "Post date"
        /// </summary>
        public static LocalisableString ItemCreatedAt => new TranslatableString(getKey(@"item.created_at"), @"Post date");

        /// <summary>
        /// "Deletion date"
        /// </summary>
        public static LocalisableString ItemDeletedAt => new TranslatableString(getKey(@"item.deleted_at"), @"Deletion date");

        /// <summary>
        /// "Type"
        /// </summary>
        public static LocalisableString ItemMessageType => new TranslatableString(getKey(@"item.message_type"), @"Type");

        /// <summary>
        /// "Permalink"
        /// </summary>
        public static LocalisableString ItemPermalink => new TranslatableString(getKey(@"item.permalink"), @"Permalink");

        /// <summary>
        /// "None of the posts address my concern"
        /// </summary>
        public static LocalisableString NearbyPostsConfirm => new TranslatableString(getKey(@"nearby_posts.confirm"), @"None of the posts address my concern");

        /// <summary>
        /// "There are posts around {0} ({1}). Please check them before posting."
        /// </summary>
        public static LocalisableString NearbyPostsNotice(string timestamp, string existingTimestamps) => new TranslatableString(getKey(@"nearby_posts.notice"), @"There are posts around {0} ({1}). Please check them before posting.", timestamp, existingTimestamps);

        /// <summary>
        /// "{0} in this review"
        /// </summary>
        public static LocalisableString NearbyPostsUnsaved(string count) => new TranslatableString(getKey(@"nearby_posts.unsaved"), @"{0} in this review", count);

        /// <summary>
        /// "Difficulty Owner"
        /// </summary>
        public static LocalisableString OwnerEditorButton => new TranslatableString(getKey(@"owner_editor.button"), @"Difficulty Owner");

        /// <summary>
        /// "Reset owner for this difficulty?"
        /// </summary>
        public static LocalisableString OwnerEditorResetConfirm => new TranslatableString(getKey(@"owner_editor.reset_confirm"), @"Reset owner for this difficulty?");

        /// <summary>
        /// "Owner"
        /// </summary>
        public static LocalisableString OwnerEditorUser => new TranslatableString(getKey(@"owner_editor.user"), @"Owner");

        /// <summary>
        /// "Difficulty"
        /// </summary>
        public static LocalisableString OwnerEditorVersion => new TranslatableString(getKey(@"owner_editor.version"), @"Difficulty");

        /// <summary>
        /// "Sign in to Respond"
        /// </summary>
        public static LocalisableString ReplyOpenGuest => new TranslatableString(getKey(@"reply.open.guest"), @"Sign in to Respond");

        /// <summary>
        /// "Respond"
        /// </summary>
        public static LocalisableString ReplyOpenUser => new TranslatableString(getKey(@"reply.open.user"), @"Respond");

        /// <summary>
        /// "{0} / {1} blocks used"
        /// </summary>
        public static LocalisableString ReviewBlockCount(string used, string max) => new TranslatableString(getKey(@"review.block_count"), @"{0} / {1} blocks used", used, max);

        /// <summary>
        /// "View Review Post"
        /// </summary>
        public static LocalisableString ReviewGoToParent => new TranslatableString(getKey(@"review.go_to_parent"), @"View Review Post");

        /// <summary>
        /// "View Discussion"
        /// </summary>
        public static LocalisableString ReviewGoToChild => new TranslatableString(getKey(@"review.go_to_child"), @"View Discussion");

        /// <summary>
        /// "each block may only contain up to {0} characters"
        /// </summary>
        public static LocalisableString ReviewValidationBlockTooLarge(string limit) => new TranslatableString(getKey(@"review.validation.block_too_large"), @"each block may only contain up to {0} characters", limit);

        /// <summary>
        /// "review contains references to issues that don&#39;t belong to this review"
        /// </summary>
        public static LocalisableString ReviewValidationExternalReferences => new TranslatableString(getKey(@"review.validation.external_references"), @"review contains references to issues that don't belong to this review");

        /// <summary>
        /// "invalid block type"
        /// </summary>
        public static LocalisableString ReviewValidationInvalidBlockType => new TranslatableString(getKey(@"review.validation.invalid_block_type"), @"invalid block type");

        /// <summary>
        /// "invalid review"
        /// </summary>
        public static LocalisableString ReviewValidationInvalidDocument => new TranslatableString(getKey(@"review.validation.invalid_document"), @"invalid review");

        /// <summary>
        /// "review must contain a minimum of {0} issue|review must contain a minimum of {0} issues"
        /// </summary>
        public static LocalisableString ReviewValidationMinimumIssues(string count) => new TranslatableString(getKey(@"review.validation.minimum_issues"), @"review must contain a minimum of {0} issue|review must contain a minimum of {0} issues", count);

        /// <summary>
        /// "block is missing text"
        /// </summary>
        public static LocalisableString ReviewValidationMissingText => new TranslatableString(getKey(@"review.validation.missing_text"), @"block is missing text");

        /// <summary>
        /// "reviews may only contain {0} paragraph/issue|reviews may only contain up to {0} paragraphs/issues"
        /// </summary>
        public static LocalisableString ReviewValidationTooManyBlocks(string count) => new TranslatableString(getKey(@"review.validation.too_many_blocks"), @"reviews may only contain {0} paragraph/issue|reviews may only contain up to {0} paragraphs/issues", count);

        /// <summary>
        /// "Marked as resolved by {0}"
        /// </summary>
        public static LocalisableString SystemResolvedTrue(string user) => new TranslatableString(getKey(@"system.resolved.true"), @"Marked as resolved by {0}", user);

        /// <summary>
        /// "Reopened by {0}"
        /// </summary>
        public static LocalisableString SystemResolvedFalse(string user) => new TranslatableString(getKey(@"system.resolved.false"), @"Reopened by {0}", user);

        /// <summary>
        /// "general"
        /// </summary>
        public static LocalisableString TimestampDisplayGeneral => new TranslatableString(getKey(@"timestamp_display.general"), @"general");

        /// <summary>
        /// "general (all)"
        /// </summary>
        public static LocalisableString TimestampDisplayGeneralAll => new TranslatableString(getKey(@"timestamp_display.general_all"), @"general (all)");

        /// <summary>
        /// "Everyone"
        /// </summary>
        public static LocalisableString UserFilterEveryone => new TranslatableString(getKey(@"user_filter.everyone"), @"Everyone");

        /// <summary>
        /// "Filter by user"
        /// </summary>
        public static LocalisableString UserFilterLabel => new TranslatableString(getKey(@"user_filter.label"), @"Filter by user");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}