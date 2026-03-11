// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class BreakInfoStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.BreakInfo";

        /// <summary>
        /// "Current Progress"
        /// </summary>
        public static LocalisableString CurrentProgressTitle => new TranslatableString(getKey(@"current_progress_title"), @"Current Progress");

        /// <summary>
        /// "Grade"
        /// </summary>
        public static LocalisableString ShowInfoGrade => new TranslatableString(getKey(@"show_info_grade"), @"Grade");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
