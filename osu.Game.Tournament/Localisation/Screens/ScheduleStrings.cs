// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Tournament.Localisation.Screens
{
    public class ScheduleStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Tournament.Screens.Schedule";

        /// <summary>
        /// "Recent matches"
        /// </summary>
        public static LocalisableString RecentMatches => new TranslatableString(getKey(@"recent_matches"), @"Recent matches");

        /// <summary>
        /// "Upcoming matches"
        /// </summary>
        public static LocalisableString UpcomingMatches => new TranslatableString(getKey(@"upcoming_matches"), @"Upcoming matches");

        /// <summary>
        /// "Coming up next"
        /// </summary>
        public static LocalisableString ComingUpNext => new TranslatableString(getKey(@"coming_up_next"), @"Coming up next");

        /// <summary>
        /// " (conditional)"
        /// </summary>
        public static LocalisableString Conditional => new TranslatableString(getKey(@"conditional"), @" (conditional)");

        /// <summary>
        /// "Started {0}"
        /// </summary>
        public static LocalisableString Started(LocalisableString time) => new TranslatableString(getKey(@"started"), @"Started {0}", time);

        /// <summary>
        /// "Starting {0}"
        /// </summary>
        public static LocalisableString Starting(LocalisableString time) => new TranslatableString(getKey(@"starting"), @"Starting {0}", time);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
