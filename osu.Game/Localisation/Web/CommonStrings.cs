// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Web
{
    public static class CommonStrings
    {
        private const string prefix = @"osu.Game.Localisation.Web.Common";

        /// <summary>
        /// "Are you sure?"
        /// </summary>
        public static LocalisableString Confirmation => new TranslatableString(getKey(@"confirmation"), @"Are you sure?");

        /// <summary>
        /// "Unsaved changes will be lost. Are you sure?"
        /// </summary>
        public static LocalisableString ConfirmationUnsaved => new TranslatableString(getKey(@"confirmation_unsaved"), @"Unsaved changes will be lost. Are you sure?");

        /// <summary>
        /// "Saved"
        /// </summary>
        public static LocalisableString Saved => new TranslatableString(getKey(@"saved"), @"Saved");

        /// <summary>
        /// ", "
        /// </summary>
        public static LocalisableString ArrayAndWordsConnector => new TranslatableString(getKey(@"array_and.words_connector"), @", ");

        /// <summary>
        /// " and "
        /// </summary>
        public static LocalisableString ArrayAndTwoWordsConnector => new TranslatableString(getKey(@"array_and.two_words_connector"), @" and ");

        /// <summary>
        /// ", and "
        /// </summary>
        public static LocalisableString ArrayAndLastWordConnector => new TranslatableString(getKey(@"array_and.last_word_connector"), @", and ");

        /// <summary>
        /// "NEW"
        /// </summary>
        public static LocalisableString BadgesNew => new TranslatableString(getKey(@"badges.new"), @"NEW");

        /// <summary>
        /// "Admin"
        /// </summary>
        public static LocalisableString ButtonsAdmin => new TranslatableString(getKey(@"buttons.admin"), @"Admin");

        /// <summary>
        /// "Authorise"
        /// </summary>
        public static LocalisableString ButtonsAuthorise => new TranslatableString(getKey(@"buttons.authorise"), @"Authorise");

        /// <summary>
        /// "Authorising..."
        /// </summary>
        public static LocalisableString ButtonsAuthorising => new TranslatableString(getKey(@"buttons.authorising"), @"Authorising...");

        /// <summary>
        /// "Return to previous position"
        /// </summary>
        public static LocalisableString ButtonsBackToPrevious => new TranslatableString(getKey(@"buttons.back_to_previous"), @"Return to previous position");

        /// <summary>
        /// "Back to top"
        /// </summary>
        public static LocalisableString ButtonsBackToTop => new TranslatableString(getKey(@"buttons.back_to_top"), @"Back to top");

        /// <summary>
        /// "Cancel"
        /// </summary>
        public static LocalisableString ButtonsCancel => new TranslatableString(getKey(@"buttons.cancel"), @"Cancel");

        /// <summary>
        /// "change"
        /// </summary>
        public static LocalisableString ButtonsChange => new TranslatableString(getKey(@"buttons.change"), @"change");

        /// <summary>
        /// "Clear"
        /// </summary>
        public static LocalisableString ButtonsClear => new TranslatableString(getKey(@"buttons.clear"), @"Clear");

        /// <summary>
        /// "click to copy to clipboard"
        /// </summary>
        public static LocalisableString ButtonsClickToCopy => new TranslatableString(getKey(@"buttons.click_to_copy"), @"click to copy to clipboard");

        /// <summary>
        /// "copied to clipboard!"
        /// </summary>
        public static LocalisableString ButtonsClickToCopyCopied => new TranslatableString(getKey(@"buttons.click_to_copy_copied"), @"copied to clipboard!");

        /// <summary>
        /// "Close"
        /// </summary>
        public static LocalisableString ButtonsClose => new TranslatableString(getKey(@"buttons.close"), @"Close");

        /// <summary>
        /// "collapse"
        /// </summary>
        public static LocalisableString ButtonsCollapse => new TranslatableString(getKey(@"buttons.collapse"), @"collapse");

        /// <summary>
        /// "Delete"
        /// </summary>
        public static LocalisableString ButtonsDelete => new TranslatableString(getKey(@"buttons.delete"), @"Delete");

        /// <summary>
        /// "Edit"
        /// </summary>
        public static LocalisableString ButtonsEdit => new TranslatableString(getKey(@"buttons.edit"), @"Edit");

        /// <summary>
        /// "expand"
        /// </summary>
        public static LocalisableString ButtonsExpand => new TranslatableString(getKey(@"buttons.expand"), @"expand");

        /// <summary>
        /// "hide"
        /// </summary>
        public static LocalisableString ButtonsHide => new TranslatableString(getKey(@"buttons.hide"), @"hide");

        /// <summary>
        /// "permalink"
        /// </summary>
        public static LocalisableString ButtonsPermalink => new TranslatableString(getKey(@"buttons.permalink"), @"permalink");

        /// <summary>
        /// "pin"
        /// </summary>
        public static LocalisableString ButtonsPin => new TranslatableString(getKey(@"buttons.pin"), @"pin");

        /// <summary>
        /// "Post"
        /// </summary>
        public static LocalisableString ButtonsPost => new TranslatableString(getKey(@"buttons.post"), @"Post");

        /// <summary>
        /// "read more"
        /// </summary>
        public static LocalisableString ButtonsReadMore => new TranslatableString(getKey(@"buttons.read_more"), @"read more");

        /// <summary>
        /// "Reply"
        /// </summary>
        public static LocalisableString ButtonsReply => new TranslatableString(getKey(@"buttons.reply"), @"Reply");

        /// <summary>
        /// "Reply and Reopen"
        /// </summary>
        public static LocalisableString ButtonsReplyReopen => new TranslatableString(getKey(@"buttons.reply_reopen"), @"Reply and Reopen");

        /// <summary>
        /// "Reply and Resolve"
        /// </summary>
        public static LocalisableString ButtonsReplyResolve => new TranslatableString(getKey(@"buttons.reply_resolve"), @"Reply and Resolve");

        /// <summary>
        /// "Reset"
        /// </summary>
        public static LocalisableString ButtonsReset => new TranslatableString(getKey(@"buttons.reset"), @"Reset");

        /// <summary>
        /// "Restore"
        /// </summary>
        public static LocalisableString ButtonsRestore => new TranslatableString(getKey(@"buttons.restore"), @"Restore");

        /// <summary>
        /// "Save"
        /// </summary>
        public static LocalisableString ButtonsSave => new TranslatableString(getKey(@"buttons.save"), @"Save");

        /// <summary>
        /// "Saving..."
        /// </summary>
        public static LocalisableString ButtonsSaving => new TranslatableString(getKey(@"buttons.saving"), @"Saving...");

        /// <summary>
        /// "Search"
        /// </summary>
        public static LocalisableString ButtonsSearch => new TranslatableString(getKey(@"buttons.search"), @"Search");

        /// <summary>
        /// "see more"
        /// </summary>
        public static LocalisableString ButtonsSeeMore => new TranslatableString(getKey(@"buttons.see_more"), @"see more");

        /// <summary>
        /// "show"
        /// </summary>
        public static LocalisableString ButtonsShow => new TranslatableString(getKey(@"buttons.show"), @"show");

        /// <summary>
        /// "Show deleted"
        /// </summary>
        public static LocalisableString ButtonsShowDeleted => new TranslatableString(getKey(@"buttons.show_deleted"), @"Show deleted");

        /// <summary>
        /// "show less"
        /// </summary>
        public static LocalisableString ButtonsShowLess => new TranslatableString(getKey(@"buttons.show_less"), @"show less");

        /// <summary>
        /// "show more"
        /// </summary>
        public static LocalisableString ButtonsShowMore => new TranslatableString(getKey(@"buttons.show_more"), @"show more");

        /// <summary>
        /// "show more options"
        /// </summary>
        public static LocalisableString ButtonsShowMoreOptions => new TranslatableString(getKey(@"buttons.show_more_options"), @"show more options");

        /// <summary>
        /// "unpin"
        /// </summary>
        public static LocalisableString ButtonsUnpin => new TranslatableString(getKey(@"buttons.unpin"), @"unpin");

        /// <summary>
        /// "Update"
        /// </summary>
        public static LocalisableString ButtonsUpdate => new TranslatableString(getKey(@"buttons.update"), @"Update");

        /// <summary>
        /// "upload image"
        /// </summary>
        public static LocalisableString ButtonsUploadImage => new TranslatableString(getKey(@"buttons.upload_image"), @"upload image");

        /// <summary>
        /// "Unwatch"
        /// </summary>
        public static LocalisableString ButtonsWatchTo0 => new TranslatableString(getKey(@"buttons.watch.to_0"), @"Unwatch");

        /// <summary>
        /// "Watch"
        /// </summary>
        public static LocalisableString ButtonsWatchTo1 => new TranslatableString(getKey(@"buttons.watch.to_1"), @"Watch");

        /// <summary>
        /// "{0} badge|{0} badges"
        /// </summary>
        public static LocalisableString CountBadges(string countDelimited) => new TranslatableString(getKey(@"count.badges"), @"{0} badge|{0} badges", countDelimited);

        /// <summary>
        /// "{0} day|{0} days"
        /// </summary>
        public static LocalisableString CountDays(string countDelimited) => new TranslatableString(getKey(@"count.days"), @"{0} day|{0} days", countDelimited);

        /// <summary>
        /// "hr|hrs"
        /// </summary>
        public static LocalisableString CountHourShortUnit => new TranslatableString(getKey(@"count.hour_short_unit"), @"hr|hrs");

        /// <summary>
        /// "{0} hour|{0} hours"
        /// </summary>
        public static LocalisableString CountHours(string countDelimited) => new TranslatableString(getKey(@"count.hours"), @"{0} hour|{0} hours", countDelimited);

        /// <summary>
        /// "{0} unit|{0} units"
        /// </summary>
        public static LocalisableString CountItem(string countDelimited) => new TranslatableString(getKey(@"count.item"), @"{0} unit|{0} units", countDelimited);

        /// <summary>
        /// "min|mins"
        /// </summary>
        public static LocalisableString CountMinuteShortUnit => new TranslatableString(getKey(@"count.minute_short_unit"), @"min|mins");

        /// <summary>
        /// "{0} minute|{0} minutes"
        /// </summary>
        public static LocalisableString CountMinutes(string countDelimited) => new TranslatableString(getKey(@"count.minutes"), @"{0} minute|{0} minutes", countDelimited);

        /// <summary>
        /// "{0} month|{0} months"
        /// </summary>
        public static LocalisableString CountMonths(string countDelimited) => new TranslatableString(getKey(@"count.months"), @"{0} month|{0} months", countDelimited);

        /// <summary>
        /// "{0} notification|{0} notifications"
        /// </summary>
        public static LocalisableString CountNotifications(string countDelimited) => new TranslatableString(getKey(@"count.notifications"), @"{0} notification|{0} notifications", countDelimited);

        /// <summary>
        /// "+ {0} other!|+ {0} others!"
        /// </summary>
        public static LocalisableString CountPlusOthers(string countDelimited) => new TranslatableString(getKey(@"count.plus_others"), @"+ {0} other!|+ {0} others!", countDelimited);

        /// <summary>
        /// "{0} post|{0} posts"
        /// </summary>
        public static LocalisableString CountPost(string countDelimited) => new TranslatableString(getKey(@"count.post"), @"{0} post|{0} posts", countDelimited);

        /// <summary>
        /// "sec|secs"
        /// </summary>
        public static LocalisableString CountSecondShortUnit => new TranslatableString(getKey(@"count.second_short_unit"), @"sec|secs");

        /// <summary>
        /// "{0} star priority|{0} star priorities"
        /// </summary>
        public static LocalisableString CountStarPriority(string countDelimited) => new TranslatableString(getKey(@"count.star_priority"), @"{0} star priority|{0} star priorities", countDelimited);

        /// <summary>
        /// "{0} update|{0} updates"
        /// </summary>
        public static LocalisableString CountUpdate(string countDelimited) => new TranslatableString(getKey(@"count.update"), @"{0} update|{0} updates", countDelimited);

        /// <summary>
        /// "{0} view|{0} views"
        /// </summary>
        public static LocalisableString CountView(string countDelimited) => new TranslatableString(getKey(@"count.view"), @"{0} view|{0} views", countDelimited);

        /// <summary>
        /// "{0} year|{0} years"
        /// </summary>
        public static LocalisableString CountYears(string countDelimited) => new TranslatableString(getKey(@"count.years"), @"{0} year|{0} years", countDelimited);

        /// <summary>
        /// "days"
        /// </summary>
        public static LocalisableString CountdownDays => new TranslatableString(getKey(@"countdown.days"), @"days");

        /// <summary>
        /// "hours"
        /// </summary>
        public static LocalisableString CountdownHours => new TranslatableString(getKey(@"countdown.hours"), @"hours");

        /// <summary>
        /// "minutes"
        /// </summary>
        public static LocalisableString CountdownMinutes => new TranslatableString(getKey(@"countdown.minutes"), @"minutes");

        /// <summary>
        /// "seconds"
        /// </summary>
        public static LocalisableString CountdownSeconds => new TranslatableString(getKey(@"countdown.seconds"), @"seconds");

        /// <summary>
        /// "MMMM YYYY"
        /// </summary>
        public static LocalisableString DatetimeYearMonthMoment => new TranslatableString(getKey(@"datetime.year_month.moment"), @"MMMM YYYY");

        /// <summary>
        /// "MMMM y"
        /// </summary>
        public static LocalisableString DatetimeYearMonthPhp => new TranslatableString(getKey(@"datetime.year_month.php"), @"MMMM y");

        /// <summary>
        /// "MMM YYYY"
        /// </summary>
        public static LocalisableString DatetimeYearMonthShortMoment => new TranslatableString(getKey(@"datetime.year_month_short.moment"), @"MMM YYYY");

        /// <summary>
        /// "Keyboard"
        /// </summary>
        public static LocalisableString DeviceKeyboard => new TranslatableString(getKey(@"device.keyboard"), @"Keyboard");

        /// <summary>
        /// "Mouse"
        /// </summary>
        public static LocalisableString DeviceMouse => new TranslatableString(getKey(@"device.mouse"), @"Mouse");

        /// <summary>
        /// "Tablet"
        /// </summary>
        public static LocalisableString DeviceTablet => new TranslatableString(getKey(@"device.tablet"), @"Tablet");

        /// <summary>
        /// "Touch Screen"
        /// </summary>
        public static LocalisableString DeviceTouch => new TranslatableString(getKey(@"device.touch"), @"Touch Screen");

        /// <summary>
        /// "drop here to upload"
        /// </summary>
        public static LocalisableString DropzoneTarget => new TranslatableString(getKey(@"dropzone.target"), @"drop here to upload");

        /// <summary>
        /// "search..."
        /// </summary>
        public static LocalisableString InputSearch => new TranslatableString(getKey(@"input.search"), @"search...");

        /// <summary>
        /// "prev"
        /// </summary>
        public static LocalisableString PaginationPrevious => new TranslatableString(getKey(@"pagination.previous"), @"prev");

        /// <summary>
        /// "next"
        /// </summary>
        public static LocalisableString PaginationNext => new TranslatableString(getKey(@"pagination.next"), @"next");

        /// <summary>
        /// "100"
        /// </summary>
        public static LocalisableString ScoreCountCount100 => new TranslatableString(getKey(@"score_count.count_100"), @"100");

        /// <summary>
        /// "300"
        /// </summary>
        public static LocalisableString ScoreCountCount300 => new TranslatableString(getKey(@"score_count.count_300"), @"300");

        /// <summary>
        /// "50"
        /// </summary>
        public static LocalisableString ScoreCountCount50 => new TranslatableString(getKey(@"score_count.count_50"), @"50");

        /// <summary>
        /// "MAX"
        /// </summary>
        public static LocalisableString ScoreCountCountGeki => new TranslatableString(getKey(@"score_count.count_geki"), @"MAX");

        /// <summary>
        /// "200"
        /// </summary>
        public static LocalisableString ScoreCountCountKatu => new TranslatableString(getKey(@"score_count.count_katu"), @"200");

        /// <summary>
        /// "Miss"
        /// </summary>
        public static LocalisableString ScoreCountCountMiss => new TranslatableString(getKey(@"score_count.count_miss"), @"Miss");

        /// <summary>
        /// "%dd"
        /// </summary>
        public static LocalisableString ScoreboardTimed => new TranslatableString(getKey(@"scoreboard_time.d"), @"%dd");

        /// <summary>
        /// "%dd"
        /// </summary>
        public static LocalisableString ScoreboardTimedd => new TranslatableString(getKey(@"scoreboard_time.dd"), @"%dd");

        /// <summary>
        /// "%dh"
        /// </summary>
        public static LocalisableString ScoreboardTimeh => new TranslatableString(getKey(@"scoreboard_time.h"), @"%dh");

        /// <summary>
        /// "%dh"
        /// </summary>
        public static LocalisableString ScoreboardTimehh => new TranslatableString(getKey(@"scoreboard_time.hh"), @"%dh");

        /// <summary>
        /// "%dm"
        /// </summary>
        public static LocalisableString ScoreboardTimeM => new TranslatableString(getKey(@"scoreboard_time.m"), @"%dm");

        /// <summary>
        /// "%dm"
        /// </summary>
        public static LocalisableString ScoreboardTimeMM => new TranslatableString(getKey(@"scoreboard_time.mm"), @"%dm");

        /// <summary>
        /// "%s"
        /// </summary>
        public static LocalisableString ScoreboardTimePast => new TranslatableString(getKey(@"scoreboard_time.past"), @"%s");

        /// <summary>
        /// "now"
        /// </summary>
        public static LocalisableString ScoreboardTimes => new TranslatableString(getKey(@"scoreboard_time.s"), @"now");

        /// <summary>
        /// "%dy"
        /// </summary>
        public static LocalisableString ScoreboardTimey => new TranslatableString(getKey(@"scoreboard_time.y"), @"%dy");

        /// <summary>
        /// "%dy"
        /// </summary>
        public static LocalisableString ScoreboardTimeyy => new TranslatableString(getKey(@"scoreboard_time.yy"), @"%dy");

        /// <summary>
        /// "{0} day ago|{0} days ago"
        /// </summary>
        public static LocalisableString TimeDaysAgo(string countDelimited) => new TranslatableString(getKey(@"time.days_ago"), @"{0} day ago|{0} days ago", countDelimited);

        /// <summary>
        /// "{0} hour ago|{0} hours ago"
        /// </summary>
        public static LocalisableString TimeHoursAgo(string countDelimited) => new TranslatableString(getKey(@"time.hours_ago"), @"{0} hour ago|{0} hours ago", countDelimited);

        /// <summary>
        /// "now"
        /// </summary>
        public static LocalisableString TimeNow => new TranslatableString(getKey(@"time.now"), @"now");

        /// <summary>
        /// "Time Remaining"
        /// </summary>
        public static LocalisableString TimeRemaining => new TranslatableString(getKey(@"time.remaining"), @"Time Remaining");

        /// <summary>
        /// "Notice"
        /// </summary>
        public static LocalisableString TitleNotice => new TranslatableString(getKey(@"title.notice"), @"Notice");

        /// <summary>
        /// "You&#39;re signed in as {0}. {1}."
        /// </summary>
        public static LocalisableString WrongUserDefault(string user, string logoutLink) => new TranslatableString(getKey(@"wrong_user._"), @"You're signed in as {0}. {1}.", user, logoutLink);

        /// <summary>
        /// "Click here to sign in as different user"
        /// </summary>
        public static LocalisableString WrongUserLogoutLink => new TranslatableString(getKey(@"wrong_user.logout_link"), @"Click here to sign in as different user");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}