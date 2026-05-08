// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class TimeDisplayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.TimeDisplay";

        /// <summary>
        /// "{0}{1}"
        /// </summary>
        public static LocalisableString TimeDisplay(int timeUnit, LocalisableString timeString) => new TranslatableString(getKey(@"time_display"), @"{0}{1}", timeUnit, timeString);

        /// <summary>
        /// "dy|dys"
        /// </summary>
        public static LocalisableString CountDayShortUnit(int quantity) => new PluralisableString(new TranslatableString(getKey(@"count_day_short_unit"), @"dy|dys"), quantity, '|');

        /// <summary>
        /// "mo|mos"
        /// </summary>
        public static LocalisableString CountMonthShortUnit(int quantity) => new PluralisableString(new TranslatableString(getKey(@"count_month_short_unit"), @"mo|mos"), quantity, '|');

        /// <summary>
        /// "yr|yrs"
        /// </summary>
        public static LocalisableString CountYearShortUnit(int quantity) => new PluralisableString(new TranslatableString(getKey(@"count_year_short_unit"), @"yr|yrs"), quantity, '|');

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
