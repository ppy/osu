// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DailyChallengeStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.DailyChallenge";

        /// <summary>
        /// "Today&#39;s daily challenge has concluded – thanks for playing!
        ///
        /// Tomorrow&#39;s challenge is now being prepared and will appear soon."
        /// </summary>
        public static LocalisableString ChallengeEndedNotification => new TranslatableString(getKey(@"todays_daily_challenge_has_concluded"),
            @"Today's daily challenge has concluded – thanks for playing!

Tomorrow's challenge is now being prepared and will appear soon.");

        /// <summary>
        /// "Today&#39;s daily challenge is now live! Click here to play."
        /// </summary>
        public static LocalisableString ChallengeLiveNotification => new TranslatableString(getKey(@"todays_daily_challenge_is_now"), @"Today's daily challenge is now live! Click here to play.");

        /// <summary>
        /// "Today&#39;s Challenge"
        /// </summary>
        public static LocalisableString TodaysChallenge => new TranslatableString(getKey(@"todays_challenge"), @"Today's Challenge");

        /// <summary>
        /// "Difficulty: {0}"
        /// </summary>
        public static LocalisableString DifficultyInfo(string difficultyName) => new TranslatableString(getKey(@"difficulty_info"), @"Difficulty: {0}", difficultyName);

        /// <summary>
        /// "Time remaining"
        /// </summary>
        public static LocalisableString SectionTimeRemaining => new TranslatableString(getKey(@"section_time_remaining"), @"Time remaining");

        /// <summary>
        /// "Score breakdown"
        /// </summary>
        public static LocalisableString SectionScoreBreakdown => new TranslatableString(getKey(@"section_score_breakdown"), @"Score breakdown");

        /// <summary>
        /// "Total pass count"
        /// </summary>
        public static LocalisableString SectionTotalPasses => new TranslatableString(getKey(@"section_total_passes"), @"Total pass count");

        /// <summary>
        /// "Cumulative total score"
        /// </summary>
        public static LocalisableString SectionCumulativeScore => new TranslatableString(getKey(@"section_cumulative_score"), @"Cumulative total score");

        /// <summary>
        /// "Events"
        /// </summary>
        public static LocalisableString SectionEvents => new TranslatableString(getKey(@"section_events"), @"Events");

        /// <summary>
        /// "remaining"
        /// </summary>
        public static LocalisableString Remaining => new TranslatableString(getKey(@"remaining"), @"remaining");

        /// <summary>
        /// "You"
        /// </summary>
        public static LocalisableString You => new TranslatableString(getKey(@"you"), @"You");

        /// <summary>
        /// "{0:N0} passes in {1:N0} - {2:N0} range"
        /// </summary>
        public static LocalisableString ScoreBreakdownBarTooltip(long passesCount, int minScore, int maxScore) => new TranslatableString(getKey(@"score_breakdown_bar_tooltip"), @"{0:N0} passes in {1:N0} - {2:N0} range", passesCount, minScore, maxScore);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
