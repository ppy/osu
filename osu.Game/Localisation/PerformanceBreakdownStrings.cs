// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class PerformanceBreakdownStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.PerformanceBreakdown";

        /// <summary>
        /// "Performance Breakdown"
        /// </summary>
        public static LocalisableString PerformanceBreakdownHeader => new TranslatableString(getKey(@"performance_breakdown_header"), @"Performance Breakdown");

        /// <summary>
        /// "Difficulty"
        /// </summary>
        public static LocalisableString DifficultyAttribute => new TranslatableString(getKey(@"difficulty_attribute"), @"Difficulty");

        /// <summary>
        /// "Aim"
        /// </summary>
        public static LocalisableString AimAttribute => new TranslatableString(getKey(@"aim_attribute"), @"Aim");

        /// <summary>
        /// "Speed"
        /// </summary>
        public static LocalisableString SpeedAttribute => new TranslatableString(getKey(@"speed_attribute"), @"Speed");

        /// <summary>
        /// "Accuracy"
        /// </summary>
        public static LocalisableString AccuracyAttribute => new TranslatableString(getKey(@"accuracy_attribute"), @"Accuracy");

        /// <summary>
        /// "Flashlight Bonus"
        /// </summary>
        public static LocalisableString FlashlightBonusAttribute => new TranslatableString(getKey(@"flashlight_bonus_attribute"), @"Flashlight Bonus");

        /// <summary>
        /// "Reading"
        /// </summary>
        public static LocalisableString ReadingAttribute => new TranslatableString(getKey(@"reading_attribute"), @"Reading");

        /// <summary>
        /// "Achieved PP"
        /// </summary>
        public static LocalisableString AchievedPP => new TranslatableString(getKey(@"achieved_pp"), @"Achieved PP");

        /// <summary>
        /// "Maximum PP"
        /// </summary>
        public static LocalisableString MaximumPP => new TranslatableString(getKey(@"maximum_pp"), @"Maximum PP");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
