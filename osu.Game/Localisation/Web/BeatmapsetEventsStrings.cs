// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class BeatmapsetEventsStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.BeatmapsetEvents";

        /// <summary>
        /// "Approved."
        /// </summary>
        public static LocalisableString EventApprove => new TranslatableString(getKey(@"event.approve"), @"Approved.");

        /// <summary>
        /// "Owner of difficulty {0} changed to {1}."
        /// </summary>
        public static LocalisableString EventBeatmapOwnerChange(string beatmap, string newUser) => new TranslatableString(getKey(@"event.beatmap_owner_change"), @"Owner of difficulty {0} changed to {1}.", beatmap, newUser);

        /// <summary>
        /// "Moderator deleted discussion {0}."
        /// </summary>
        public static LocalisableString EventDiscussionDelete(string discussion) => new TranslatableString(getKey(@"event.discussion_delete"), @"Moderator deleted discussion {0}.", discussion);

        /// <summary>
        /// "Discussion for this beatmap has been disabled. ({0})"
        /// </summary>
        public static LocalisableString EventDiscussionLock(string text) => new TranslatableString(getKey(@"event.discussion_lock"), @"Discussion for this beatmap has been disabled. ({0})", text);

        /// <summary>
        /// "Moderator deleted post from discussion {0}."
        /// </summary>
        public static LocalisableString EventDiscussionPostDelete(string discussion) => new TranslatableString(getKey(@"event.discussion_post_delete"), @"Moderator deleted post from discussion {0}.", discussion);

        /// <summary>
        /// "Moderator restored post from discussion {0}."
        /// </summary>
        public static LocalisableString EventDiscussionPostRestore(string discussion) => new TranslatableString(getKey(@"event.discussion_post_restore"), @"Moderator restored post from discussion {0}.", discussion);

        /// <summary>
        /// "Moderator restored discussion {0}."
        /// </summary>
        public static LocalisableString EventDiscussionRestore(string discussion) => new TranslatableString(getKey(@"event.discussion_restore"), @"Moderator restored discussion {0}.", discussion);

        /// <summary>
        /// "Discussion for this beatmap has been enabled."
        /// </summary>
        public static LocalisableString EventDiscussionUnlock => new TranslatableString(getKey(@"event.discussion_unlock"), @"Discussion for this beatmap has been enabled.");

        /// <summary>
        /// "Disqualified by {0}. Reason: {1} ({2})."
        /// </summary>
        public static LocalisableString EventDisqualify(string user, string discussion, string text) => new TranslatableString(getKey(@"event.disqualify"), @"Disqualified by {0}. Reason: {1} ({2}).", user, discussion, text);

        /// <summary>
        /// "Disqualified by {0}. Reason: {1}."
        /// </summary>
        public static LocalisableString EventDisqualifyLegacy(string user, string text) => new TranslatableString(getKey(@"event.disqualify_legacy"), @"Disqualified by {0}. Reason: {1}.", user, text);

        /// <summary>
        /// "Genre changed from {0} to {1}."
        /// </summary>
        public static LocalisableString EventGenreEdit(string old, string @new) => new TranslatableString(getKey(@"event.genre_edit"), @"Genre changed from {0} to {1}.", old, @new);

        /// <summary>
        /// "Resolved issue {0} by {1} reopened by {2}."
        /// </summary>
        public static LocalisableString EventIssueReopen(string discussion, string discussionUser, string user) => new TranslatableString(getKey(@"event.issue_reopen"), @"Resolved issue {0} by {1} reopened by {2}.", discussion, discussionUser, user);

        /// <summary>
        /// "Issue {0} by {1} marked as resolved by {2}."
        /// </summary>
        public static LocalisableString EventIssueResolve(string discussion, string discussionUser, string user) => new TranslatableString(getKey(@"event.issue_resolve"), @"Issue {0} by {1} marked as resolved by {2}.", discussion, discussionUser, user);

        /// <summary>
        /// "Kudosu denial for discussion {0} has been removed."
        /// </summary>
        public static LocalisableString EventKudosuAllow(string discussion) => new TranslatableString(getKey(@"event.kudosu_allow"), @"Kudosu denial for discussion {0} has been removed.", discussion);

        /// <summary>
        /// "Discussion {0} denied for kudosu."
        /// </summary>
        public static LocalisableString EventKudosuDeny(string discussion) => new TranslatableString(getKey(@"event.kudosu_deny"), @"Discussion {0} denied for kudosu.", discussion);

        /// <summary>
        /// "Discussion {0} by {1} obtained enough votes for kudosu."
        /// </summary>
        public static LocalisableString EventKudosuGain(string discussion, string user) => new TranslatableString(getKey(@"event.kudosu_gain"), @"Discussion {0} by {1} obtained enough votes for kudosu.", discussion, user);

        /// <summary>
        /// "Discussion {0} by {1} lost votes and granted kudosu has been removed."
        /// </summary>
        public static LocalisableString EventKudosuLost(string discussion, string user) => new TranslatableString(getKey(@"event.kudosu_lost"), @"Discussion {0} by {1} lost votes and granted kudosu has been removed.", discussion, user);

        /// <summary>
        /// "Discussion {0} has had its kudosu grants recalculated."
        /// </summary>
        public static LocalisableString EventKudosuRecalculate(string discussion) => new TranslatableString(getKey(@"event.kudosu_recalculate"), @"Discussion {0} has had its kudosu grants recalculated.", discussion);

        /// <summary>
        /// "Language changed from {0} to {1}."
        /// </summary>
        public static LocalisableString EventLanguageEdit(string old, string @new) => new TranslatableString(getKey(@"event.language_edit"), @"Language changed from {0} to {1}.", old, @new);

        /// <summary>
        /// "Loved by {0}."
        /// </summary>
        public static LocalisableString EventLove(string user) => new TranslatableString(getKey(@"event.love"), @"Loved by {0}.", user);

        /// <summary>
        /// "Nominated by {0}."
        /// </summary>
        public static LocalisableString EventNominate(string user) => new TranslatableString(getKey(@"event.nominate"), @"Nominated by {0}.", user);

        /// <summary>
        /// "Nominated by {0} ({1})."
        /// </summary>
        public static LocalisableString EventNominateModes(string user, string modes) => new TranslatableString(getKey(@"event.nominate_modes"), @"Nominated by {0} ({1}).", user, modes);

        /// <summary>
        /// "New problem {0} ({1}) triggered a nomination reset."
        /// </summary>
        public static LocalisableString EventNominationReset(string discussion, string text) => new TranslatableString(getKey(@"event.nomination_reset"), @"New problem {0} ({1}) triggered a nomination reset.", discussion, text);

        /// <summary>
        /// "This beatmap has reached the required number of nominations and has been qualified."
        /// </summary>
        public static LocalisableString EventQualify => new TranslatableString(getKey(@"event.qualify"), @"This beatmap has reached the required number of nominations and has been qualified.");

        /// <summary>
        /// "Ranked."
        /// </summary>
        public static LocalisableString EventRank => new TranslatableString(getKey(@"event.rank"), @"Ranked.");

        /// <summary>
        /// "Removed from Loved by {0}. ({1})"
        /// </summary>
        public static LocalisableString EventRemoveFromLoved(string user, string text) => new TranslatableString(getKey(@"event.remove_from_loved"), @"Removed from Loved by {0}. ({1})", user, text);

        /// <summary>
        /// "Removed explicit mark"
        /// </summary>
        public static LocalisableString EventNsfwToggleTo0 => new TranslatableString(getKey(@"event.nsfw_toggle.to_0"), @"Removed explicit mark");

        /// <summary>
        /// "Marked as explicit"
        /// </summary>
        public static LocalisableString EventNsfwToggleTo1 => new TranslatableString(getKey(@"event.nsfw_toggle.to_1"), @"Marked as explicit");

        /// <summary>
        /// "Beatmapset Events"
        /// </summary>
        public static LocalisableString IndexTitle => new TranslatableString(getKey(@"index.title"), @"Beatmapset Events");

        /// <summary>
        /// "Period"
        /// </summary>
        public static LocalisableString IndexFormPeriod => new TranslatableString(getKey(@"index.form.period"), @"Period");

        /// <summary>
        /// "Types"
        /// </summary>
        public static LocalisableString IndexFormTypes => new TranslatableString(getKey(@"index.form.types"), @"Types");

        /// <summary>
        /// "Content"
        /// </summary>
        public static LocalisableString ItemContent => new TranslatableString(getKey(@"item.content"), @"Content");

        /// <summary>
        /// "[deleted]"
        /// </summary>
        public static LocalisableString ItemDiscussionDeleted => new TranslatableString(getKey(@"item.discussion_deleted"), @"[deleted]");

        /// <summary>
        /// "Type"
        /// </summary>
        public static LocalisableString ItemType => new TranslatableString(getKey(@"item.type"), @"Type");

        /// <summary>
        /// "Approval"
        /// </summary>
        public static LocalisableString TypeApprove => new TranslatableString(getKey(@"type.approve"), @"Approval");

        /// <summary>
        /// "Difficulty owner change"
        /// </summary>
        public static LocalisableString TypeBeatmapOwnerChange => new TranslatableString(getKey(@"type.beatmap_owner_change"), @"Difficulty owner change");

        /// <summary>
        /// "Discussion deletion"
        /// </summary>
        public static LocalisableString TypeDiscussionDelete => new TranslatableString(getKey(@"type.discussion_delete"), @"Discussion deletion");

        /// <summary>
        /// "Discussion reply deletion"
        /// </summary>
        public static LocalisableString TypeDiscussionPostDelete => new TranslatableString(getKey(@"type.discussion_post_delete"), @"Discussion reply deletion");

        /// <summary>
        /// "Discussion reply restoration"
        /// </summary>
        public static LocalisableString TypeDiscussionPostRestore => new TranslatableString(getKey(@"type.discussion_post_restore"), @"Discussion reply restoration");

        /// <summary>
        /// "Discussion restoration"
        /// </summary>
        public static LocalisableString TypeDiscussionRestore => new TranslatableString(getKey(@"type.discussion_restore"), @"Discussion restoration");

        /// <summary>
        /// "Disqualification"
        /// </summary>
        public static LocalisableString TypeDisqualify => new TranslatableString(getKey(@"type.disqualify"), @"Disqualification");

        /// <summary>
        /// "Genre edit"
        /// </summary>
        public static LocalisableString TypeGenreEdit => new TranslatableString(getKey(@"type.genre_edit"), @"Genre edit");

        /// <summary>
        /// "Discussion reopening"
        /// </summary>
        public static LocalisableString TypeIssueReopen => new TranslatableString(getKey(@"type.issue_reopen"), @"Discussion reopening");

        /// <summary>
        /// "Discussion resolving"
        /// </summary>
        public static LocalisableString TypeIssueResolve => new TranslatableString(getKey(@"type.issue_resolve"), @"Discussion resolving");

        /// <summary>
        /// "Kudosu allowance"
        /// </summary>
        public static LocalisableString TypeKudosuAllow => new TranslatableString(getKey(@"type.kudosu_allow"), @"Kudosu allowance");

        /// <summary>
        /// "Kudosu denial"
        /// </summary>
        public static LocalisableString TypeKudosuDeny => new TranslatableString(getKey(@"type.kudosu_deny"), @"Kudosu denial");

        /// <summary>
        /// "Kudosu gain"
        /// </summary>
        public static LocalisableString TypeKudosuGain => new TranslatableString(getKey(@"type.kudosu_gain"), @"Kudosu gain");

        /// <summary>
        /// "Kudosu loss"
        /// </summary>
        public static LocalisableString TypeKudosuLost => new TranslatableString(getKey(@"type.kudosu_lost"), @"Kudosu loss");

        /// <summary>
        /// "Kudosu recalculation"
        /// </summary>
        public static LocalisableString TypeKudosuRecalculate => new TranslatableString(getKey(@"type.kudosu_recalculate"), @"Kudosu recalculation");

        /// <summary>
        /// "Language edit"
        /// </summary>
        public static LocalisableString TypeLanguageEdit => new TranslatableString(getKey(@"type.language_edit"), @"Language edit");

        /// <summary>
        /// "Love"
        /// </summary>
        public static LocalisableString TypeLove => new TranslatableString(getKey(@"type.love"), @"Love");

        /// <summary>
        /// "Nomination"
        /// </summary>
        public static LocalisableString TypeNominate => new TranslatableString(getKey(@"type.nominate"), @"Nomination");

        /// <summary>
        /// "Nomination resetting"
        /// </summary>
        public static LocalisableString TypeNominationReset => new TranslatableString(getKey(@"type.nomination_reset"), @"Nomination resetting");

        /// <summary>
        /// "Explicit mark"
        /// </summary>
        public static LocalisableString TypeNsfwToggle => new TranslatableString(getKey(@"type.nsfw_toggle"), @"Explicit mark");

        /// <summary>
        /// "Qualification"
        /// </summary>
        public static LocalisableString TypeQualify => new TranslatableString(getKey(@"type.qualify"), @"Qualification");

        /// <summary>
        /// "Ranking"
        /// </summary>
        public static LocalisableString TypeRank => new TranslatableString(getKey(@"type.rank"), @"Ranking");

        /// <summary>
        /// "Loved removal"
        /// </summary>
        public static LocalisableString TypeRemoveFromLoved => new TranslatableString(getKey(@"type.remove_from_loved"), @"Loved removal");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}