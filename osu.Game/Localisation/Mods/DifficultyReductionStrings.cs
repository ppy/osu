// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class DifficultyReductionStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.DifficultyReduction";

        /// <summary>
        /// "Larger circles, more forgiving HP drain, less accuracy required, and three lives!"
        /// </summary>
        public static LocalisableString OsuEasyDescription =>
            new TranslatableString(getKey(@"osu_easy_desctiption"), "Larger circles, more forgiving HP drain, less accuracy required, and three lives!");

        /// <summary>
        /// "Beats move slower, and less accuracy required!"
        /// </summary>
        public static LocalisableString TaikoEasyDescription => new TranslatableString(getKey(@"taiko_easy_description"), "Beats move slower, and less accuracy required!");

        /// <summary>
        /// "More forgiving HP drain, less accuracy, required, and three lives!"
        /// </summary>
        public static LocalisableString ManiaEasyDescription => new TranslatableString(getKey(@"mania_easy_description"), "More forgiving HP drain, less accuracy, required, and three lives!");

        /// <summary>
        /// "Larger fruits, more forgiving HP drain, less accuracy required, and three lives!"
        /// </summary>
        public static LocalisableString CatchEasyDescription =>
            new TranslatableString(getKey(@"catch_easy_description"), "Larger fruits, more forgiving HP drain, less accuracy required, and three lives!");

        /// <summary>
        /// "Extra lives"
        /// </summary>
        public static LocalisableString EasyRetries => new TranslatableString(getKey(@"easy_retries"), "Extra lives");

        /// <summary>
        /// "Number of extra lives"
        /// </summary>
        public static LocalisableString EasyRetriesDescription => new TranslatableString(getKey(@"easy_retries_description"), "Number of extra lives");

        /// <summary>
        /// "You can't fail, no matter what."
        /// </summary>
        public static LocalisableString NoFailDescription => new TranslatableString(getKey(@"no_fail_description"), "You can't fail, no matter what.");

        /// <summary>
        /// "Less zoom..."
        /// </summary>
        public static LocalisableString HalfTimeDescription => new TranslatableString(getKey(@"half_time_description"), "Less zoom...");

        /// <summary>
        /// "Speed decrease"
        /// </summary>
        public static LocalisableString HalfTimeSpeedChange => new TranslatableString(getKey(@"half_time_speed_change"), "Speed decrease");

        /// <summary>
        /// "The actial decrease to apply"
        /// </summary>
        public static LocalisableString HalfTimeSpeedChangeDescription => new TranslatableString(getKey(@"half_time_speed_change_description"), "The actial decrease to apply");

        /// <summary>
        /// "Whoaaaaa..."
        /// </summary>
        public static LocalisableString DaycoreDescription => new TranslatableString(getKey(@"daycore_description"), "Whoaaaaa...");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
