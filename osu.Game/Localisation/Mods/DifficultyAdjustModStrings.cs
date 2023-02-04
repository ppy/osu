// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class DifficultyAdjustModStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.DifficultyModAdjust";

        /// <summary>
        /// "Override a beatmap's difficulty settings."
        /// </summary>
        public static LocalisableString DifficultyAdjustDescription => new TranslatableString(getKey(@"difficulty_adjust_description"), "Override a beatmap's difficulty settings.");

        /// <summary>
        /// "HP Drain"
        /// </summary>
        public static LocalisableString DrainRate => new TranslatableString(getKey(@"drain_rate"), "HP Drain");

        /// <summary>
        /// "Override a beatmap's set HP."
        /// </summary>
        public static LocalisableString DrainRateDescription =>
            new TranslatableString(getKey(@"drain_rate_description"), "Override a beatmap's set HP.");

        /// <summary>
        /// "Accuracy"
        /// </summary>
        public static LocalisableString OverallDifficulty => new TranslatableString(getKey(@"overall_difficulty"), "Accuracy");

        /// <summary>
        /// "Override a beatmap's set OD."
        /// </summary>
        public static LocalisableString OverallDifficultyDescription =>
            new TranslatableString(getKey(@"overall_difficulty_description"), "Override a beatmap's set OD.");

        /// <summary>
        /// "Extended limits"
        /// </summary>
        public static LocalisableString ExtendedLimits => new TranslatableString(getKey(@"extended_limits"), "Extended limits");

        /// <summary>
        /// "Adjust difficulty beyond sane limits."
        /// </summary>
        public static LocalisableString ExtendedLimitsDescription =>
            new TranslatableString(getKey(@"extended_limits_description"), "Adjust difficulty beyond sane limits.");

        /// <summary>
        /// "Circle Size"
        /// </summary>
        public static LocalisableString CircleSize => new TranslatableString(getKey(@"circle_size"), "Circle Size");

        /// <summary>
        /// "Override a beatmap's set CS."
        /// </summary>
        public static LocalisableString CircleSizeDescription =>
            new TranslatableString(getKey(@"circle_size_description"), "Override a beatmap's set CS.");

        /// <summary>
        /// "Approach Rate"
        /// </summary>
        public static LocalisableString ApproachRate => new TranslatableString(getKey(@"approach_rate"), "Approach Rate");

        /// <summary>
        /// "Override a beatmap's set AR"
        /// </summary>
        public static LocalisableString ApproachRateDescription =>
            new TranslatableString(getKey(@"approach_rate_description"), "Override a beatmap's set AR");

        /// <summary>
        /// "Spicy Patterns"
        /// </summary>
        public static LocalisableString HardRockOffsets => new TranslatableString(getKey(@"hard_rock_offsets"), "Spicy Patterns");

        /// <summary>
        /// "Adjust the patterns as if Hard Rock is enabled."
        /// </summary>
        public static LocalisableString HardRockOffsetsDescription => new TranslatableString(getKey(@"hard_rock_offsets_description"), "Adjust the patterns as if Hard Rock is enabled.");

        /// <summary>
        /// "Scroll Speed"
        /// </summary>
        public static LocalisableString ScrollSpeed => new TranslatableString(getKey(@"scroll_speed"), "Scroll Speed");

        /// <summary>
        /// "Adjust a beatmap's set scroll speed"
        /// </summary>
        public static LocalisableString ScrollSpeedDescription => new TranslatableString(getKey(@"scroll_speed_description"), "Adjust a beatmap's set scroll speed");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
