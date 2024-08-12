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

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
