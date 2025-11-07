// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class RulesetBeatmapAttributesStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.RulesetBeatmapAttributes";

        /// <summary>
        /// "Circle Size"
        /// </summary>
        public static LocalisableString CircleSize => new TranslatableString(getKey(@"circle_size"), @"Circle Size");

        /// <summary>
        /// "Affects the size of hit circles and sliders."
        /// </summary>
        public static LocalisableString OsuCircleSizeDescription => new TranslatableString(getKey(@"osu_circle_size_description"), @"Affects the size of hit circles and sliders.");

        /// <summary>
        /// "Affects the size of fruits."
        /// </summary>
        public static LocalisableString CatchCircleSizeDescription => new TranslatableString(getKey(@"catch_circle_size_description"), @"Affects the size of fruits.");

        /// <summary>
        /// "Approach Rate"
        /// </summary>
        public static LocalisableString ApproachRate => new TranslatableString(getKey(@"approach_rate"), @"Approach Rate");

        /// <summary>
        /// "Affects how early objects appear on screen relative to their hit time."
        /// </summary>
        public static LocalisableString OsuApproachRateDescription =>
            new TranslatableString(getKey(@"osu_approach_rate_description"), @"Affects how early objects appear on screen relative to their hit time.");

        /// <summary>
        /// "Affects how early fruits fade in on the screen."
        /// </summary>
        public static LocalisableString CatchApproachRateDescription =>
            new TranslatableString(getKey(@"catch_approach_rate_description"), @"Affects how early fruits fade in on the screen.");

        /// <summary>
        /// "Accuracy"
        /// </summary>
        public static LocalisableString Accuracy => new TranslatableString(getKey(@"accuracy"), @"Accuracy");

        /// <summary>
        /// "Affects timing requirements for hit circles and spin speed requirements for spinners."
        /// </summary>
        public static LocalisableString OsuAccuracyDescription =>
            new TranslatableString(getKey(@"osu_accuracy_description"), @"Affects timing requirements for hit circles and spin speed requirements for spinners.");

        /// <summary>
        /// "Affects timing requirements for hits and mash rate requirements for swells."
        /// </summary>
        public static LocalisableString TaikoAccuracyDescription =>
            new TranslatableString(getKey(@"taiko_accuracy_description"), @"Affects timing requirements for hits and mash rate requirements for swells.");

        /// <summary>
        /// "Affects timing requirements for notes."
        /// </summary>
        public static LocalisableString ManiaAccuracyDescription =>
            new TranslatableString(getKey(@"mania_accuracy_description"), @"Affects timing requirements for notes.");

        /// <summary>
        /// "HP Drain"
        /// </summary>
        public static LocalisableString HPDrain => new TranslatableString(getKey(@"hp_drain"), @"HP Drain");

        /// <summary>
        /// "Affects the harshness of health drain and the health penalties for missing."
        /// </summary>
        public static LocalisableString HPDrainDescription => new TranslatableString(getKey(@"hp_drain_description"), @"Affects the harshness of health drain and the health penalties for missing.");

        /// <summary>
        /// "Scroll Speed"
        /// </summary>
        public static LocalisableString ScrollSpeed => new TranslatableString(getKey(@"scroll_speed"), @"Scroll Speed");

        /// <summary>
        /// "Multiplier applied to the baseline scroll speed of the playfield when no mods are active."
        /// </summary>
        public static LocalisableString ScrollSpeedDescription =>
            new TranslatableString(getKey(@"scroll_speed_description"), @"Multiplier applied to the baseline scroll speed of the playfield when no mods are active.");

        /// <summary>
        /// "Key Count"
        /// </summary>
        public static LocalisableString KeyCount => new TranslatableString(getKey(@"key_count"), @"Key Count");

        /// <summary>
        /// "Affects the number of key columns on the playfield."
        /// </summary>
        public static LocalisableString KeyCountDescription => new TranslatableString(getKey(@"key_count_description"), @"Affects the number of key columns on the playfield.");

        /// <summary>
        /// "Hit circle radius"
        /// </summary>
        public static LocalisableString HitCircleRadiusMetric => new TranslatableString(getKey(@"hit_circle_radius_metric"), @"Hit circle radius");

        /// <summary>
        /// "Approach time"
        /// </summary>
        public static LocalisableString ApproachTimeMetric => new TranslatableString(getKey(@"approach_time_metric"), @"Approach time");

        /// <summary>
        /// "{0} hit window"
        /// </summary>
        public static LocalisableString HitWindowMetric(LocalisableString windowName) => new TranslatableString(getKey(@"hit_window_metric"), @"{0} hit window", windowName);

        /// <summary>
        /// "RPM required to clear spinners"
        /// </summary>
        public static LocalisableString ClearSpinnersMetric => new TranslatableString(getKey(@"clear_spinners_metric"), @"RPM required to clear spinners");

        /// <summary>
        /// "RPM required to get full spinner bonus"
        /// </summary>
        public static LocalisableString FullSpinnerBonusMetric => new TranslatableString(getKey(@"full_spinner_bonus_metric"), @"RPM required to get full spinner bonus");

        /// <summary>
        /// "Hits per second required to clear swells"
        /// </summary>
        public static LocalisableString ClearSwellsMetric => new TranslatableString(getKey(@"clear_swells_metric"), @"Hits per second required to clear swells");

        /// <summary>
        /// "Fade-in time"
        /// </summary>
        public static LocalisableString FadeInTimeMetric => new TranslatableString(getKey(@"fade_in_time_metric"), @"Fade-in time");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
