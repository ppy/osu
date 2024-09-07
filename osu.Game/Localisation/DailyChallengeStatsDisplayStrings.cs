// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class DailyChallengeStatsDisplayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.DailyChallengeStatsDisplay";

        /// <summary>
        /// "{0}d"
        /// </summary>
        public static LocalisableString UnitDay(LocalisableString count) => new TranslatableString(getKey(@"unit_day"), @"{0}d", count);

        /// <summary>
        /// "{0}w"
        /// </summary>
        public static LocalisableString UnitWeek(LocalisableString count) => new TranslatableString(getKey(@"unit_week"), @"{0}w", count);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
