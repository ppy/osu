// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class BeatmapSubmissionStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.BeatmapSubmission";

        /// <summary>
        /// "Beatmap submission"
        /// </summary>
        public static LocalisableString BeatmapSubmissionTitle => new TranslatableString(getKey(@"beatmap_submission_title"), @"Beatmap submission");

        /// <summary>
        /// "Share your beatmap with the world!"
        /// </summary>
        public static LocalisableString BeatmapSubmissionDescription => new TranslatableString(getKey(@"beatmap_submission_description"), @"Share your beatmap with the world!");

        /// <summary>
        /// "Content permissions"
        /// </summary>
        public static LocalisableString ContentPermissions => new TranslatableString(getKey(@"content_permissions"), @"Content permissions");

        /// <summary>
        /// "I understand"
        /// </summary>
        public static LocalisableString ContentPermissionsAcknowledgement => new TranslatableString(getKey(@"content_permissions_acknowledgement"), @"I understand");

        /// <summary>
        /// "Frequently asked questions"
        /// </summary>
        public static LocalisableString FrequentlyAskedQuestions => new TranslatableString(getKey(@"frequently_asked_questions"), @"Frequently asked questions");

        /// <summary>
        /// "Submission settings"
        /// </summary>
        public static LocalisableString SubmissionSettings => new TranslatableString(getKey(@"submission_settings"), @"Submission settings");

        /// <summary>
        /// "Submit beatmap!"
        /// </summary>
        public static LocalisableString ConfirmSubmission => new TranslatableString(getKey(@"confirm_submission"), @"Submit beatmap!");

        /// <summary>
        /// "Exporting beatmap for compatibility..."
        /// </summary>
        public static LocalisableString Exporting => new TranslatableString(getKey(@"exporting"), @"Exporting beatmap for compatibility...");

        /// <summary>
        /// "Preparing for upload..."
        /// </summary>
        public static LocalisableString Preparing => new TranslatableString(getKey(@"preparing"), @"Preparing for upload...");

        /// <summary>
        /// "Uploading beatmap contents..."
        /// </summary>
        public static LocalisableString Uploading => new TranslatableString(getKey(@"uploading"), @"Uploading beatmap contents...");

        /// <summary>
        /// "Finishing up..."
        /// </summary>
        public static LocalisableString Finishing => new TranslatableString(getKey(@"finishing"), @"Finishing up...");

        /// <summary>
        /// "Before you continue, we ask you to check whether the content you are uploading has been cleared for upload. Please understand that you are responsible for the content you upload to the platform and if in doubt, should ask permission from the creators before uploading!"
        /// </summary>
        public static LocalisableString ContentPermissionsDisclaimer => new TranslatableString(getKey(@"content_permissions_disclaimer"), @"Before you continue, we ask you to check whether the content you are uploading has been cleared for upload. Please understand that you are responsible for the content you upload to the platform and if in doubt, should ask permission from the creators before uploading!");

        /// <summary>
        /// "Check the content usage guidelines for more information"
        /// </summary>
        public static LocalisableString CheckContentUsageGuidelines => new TranslatableString(getKey(@"check_content_usage_guidelines"), @"Check the content usage guidelines for more information");

        /// <summary>
        /// "Beatmap ranking criteria"
        /// </summary>
        public static LocalisableString BeatmapRankingCriteria => new TranslatableString(getKey(@"beatmap_ranking_criteria"), @"Beatmap ranking criteria");

        /// <summary>
        /// "Not sure you meet the guidelines? Check the list and speed up the ranking process!"
        /// </summary>
        public static LocalisableString BeatmapRankingCriteriaDescription => new TranslatableString(getKey(@"beatmap_ranking_criteria_description"), @"Not sure you meet the guidelines? Check the list and speed up the ranking process!");

        /// <summary>
        /// "Submission process"
        /// </summary>
        public static LocalisableString SubmissionProcess => new TranslatableString(getKey(@"submission_process"), @"Submission process");

        /// <summary>
        /// "Unsure about the submission process? Check out the wiki entry!"
        /// </summary>
        public static LocalisableString SubmissionProcessDescription => new TranslatableString(getKey(@"submission_process_description"), @"Unsure about the submission process? Check out the wiki entry!");

        /// <summary>
        /// "Mapping help forum"
        /// </summary>
        public static LocalisableString MappingHelpForum => new TranslatableString(getKey(@"mapping_help_forum"), @"Mapping help forum");

        /// <summary>
        /// "Got some questions about mapping and submission? Ask them in the forums!"
        /// </summary>
        public static LocalisableString MappingHelpForumDescription => new TranslatableString(getKey(@"mapping_help_forum_description"), @"Got some questions about mapping and submission? Ask them in the forums!");

        /// <summary>
        /// "Modding queues forum"
        /// </summary>
        public static LocalisableString ModdingQueuesForum => new TranslatableString(getKey(@"modding_queues_forum"), @"Modding queues forum");

        /// <summary>
        /// "Having trouble getting feedback? Why not ask in a mod queue!"
        /// </summary>
        public static LocalisableString ModdingQueuesForumDescription => new TranslatableString(getKey(@"modding_queues_forum_description"), @"Having trouble getting feedback? Why not ask in a mod queue!");

        /// <summary>
        /// "Where would you like to post your beatmap?"
        /// </summary>
        public static LocalisableString BeatmapSubmissionTargetCaption => new TranslatableString(getKey(@"beatmap_submission_target_caption"), @"Where would you like to post your beatmap?");

        /// <summary>
        /// "Works in Progress / Help (incomplete, not ready for ranking)"
        /// </summary>
        public static LocalisableString BeatmapSubmissionTargetWIP => new TranslatableString(getKey(@"beatmap_submission_target_wip"), @"Works in Progress / Help (incomplete, not ready for ranking)");

        /// <summary>
        /// "Pending (complete, ready for ranking)"
        /// </summary>
        public static LocalisableString BeatmapSubmissionTargetPending => new TranslatableString(getKey(@"beatmap_submission_target_pending"), @"Pending (complete, ready for ranking)");

        /// <summary>
        /// "Receive notifications for discussion replies"
        /// </summary>
        public static LocalisableString NotifyOnDiscussionReplies => new TranslatableString(getKey(@"notify_for_discussion_replies"), @"Receive notifications for discussion replies");

        /// <summary>
        /// "Load in browser after submission"
        /// </summary>
        public static LocalisableString LoadInBrowserAfterSubmission => new TranslatableString(getKey(@"load_in_browser_after_submission"), @"Load in browser after submission");

        /// <summary>
        /// "Note: In order to make it possible for users of all osu! versions to enjoy your beatmap, it will be exported in a backwards-compatible format. While we have made efforts to ensure that process keeps the beatmap playable in its intended form, some data related to features that previous versions of osu! do not support may be lost."
        /// </summary>
        public static LocalisableString LegacyExportDisclaimer => new TranslatableString(getKey(@"legacy_export_disclaimer"), @"Note: In order to make it possible for users of all osu! versions to enjoy your beatmap, it will be exported in a backwards-compatible format. While we have made efforts to ensure that process keeps the beatmap playable in its intended form, some data related to features that previous versions of osu! do not support may be lost.");

        /// <summary>
        /// "Empty beatmaps cannot be submitted."
        /// </summary>
        public static LocalisableString EmptyBeatmapsCannotBeSubmitted => new TranslatableString(getKey(@"empty_beatmaps_cannot_be_submitted"), @"Empty beatmaps cannot be submitted.");

        /// <summary>
        /// "Update beatmap!"
        /// </summary>
        public static LocalisableString UpdateBeatmap => new TranslatableString(getKey(@"update_beatmap"), @"Update beatmap!");

        /// <summary>
        /// "Upload NEW beatmap!"
        /// </summary>
        public static LocalisableString UploadNewBeatmap => new TranslatableString(getKey(@"upload_new_beatmap"), @"Upload NEW beatmap!");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
