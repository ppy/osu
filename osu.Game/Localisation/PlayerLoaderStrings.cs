// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class PlayerLoaderStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.PlayerLoader";

        /// <summary>
        /// "This beatmap is loved."
        /// </summary>
        public static LocalisableString LovedBeatmapDisclaimer => new TranslatableString(getKey(@"loved_beatmap_disclaimer"), @"This beatmap is loved.");

        /// <summary>
        /// "This beatmap is qualified."
        /// </summary>
        public static LocalisableString QualifiedBeatmapDisclaimer => new TranslatableString(getKey(@"qualified_beatmap_disclaimer"), @"This beatmap is qualified.");

        /// <summary>
        /// "No performance points will be awarded."
        /// </summary>
        public static LocalisableString NoPerformancePointsAwarded => new TranslatableString(getKey(@"no_performance_points_awarded"), @"No performance points will be awarded.");

        /// <summary>
        /// "Leaderboards will be reset when the beatmap is ranked."
        /// </summary>
        public static LocalisableString LeaderboardsWillBeResetOnRank => new TranslatableString(getKey(@"leaderboards_will_be_reset_on_rank"), @"Leaderboards will be reset when the beatmap is ranked.");

        /// <summary>
        /// "Leaderboards may be reset by the beatmap creator."
        /// </summary>
        public static LocalisableString LeaderboardsMayBeReset => new TranslatableString(getKey(@"leaderboards_may_be_reset"), @"Leaderboards may be reset by the beatmap creator.");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
