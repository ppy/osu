// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation.Mods
{
    public static class DifficultyIncreaseStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.Mods.DifficultyIncrease";

        /// <summary>
        /// "Everything just got a bit harder..."
        /// </summary>
        public static LocalisableString HardRockDescription => new TranslatableString(getKey(@"hard_rock_description"), "Everything just got a bit harder...");

        /// <summary>
        /// "Restatrt on fail"
        /// </summary>
        public static LocalisableString FailConditionRestart => new TranslatableString(getKey(@"fail_condition_restart"), "Restatrt on fail");

        /// <summary>
        /// "Automatically restarts when failed."
        /// </summary>
        public static LocalisableString FailConditionRestartDescription => new TranslatableString(getKey(@"fail_condition_restart_description"), "Automatically restarts when failed.");

        /// <summary>
        /// "Miss and fail."
        /// </summary>
        public static LocalisableString SuddenDeathDescription => new TranslatableString(getKey(@"sudden_death_description"), "Miss and fail.");

        /// <summary>
        /// "SS or quit."
        /// </summary>
        public static LocalisableString PerfectDescription => new TranslatableString(getKey(@"perfect_description"), "SS or quit.");

        /// <summary>
        /// "Zooooooooooom..."
        /// </summary>
        public static LocalisableString DoubleTimeDescription => new TranslatableString(getKey(@"double_time_description"), "Zooooooooooom...");

        /// <summary>
        /// "Speed increase"
        /// </summary>
        public static LocalisableString DoubleTimeSpeedChange => new TranslatableString(getKey(@"double_time_speed_change"), "Speed increase");

        /// <summary>
        /// "The actual increase to apply"
        /// </summary>
        public static LocalisableString DoubleTimeSpeedChangeDescription => new TranslatableString(getKey(@"double_time_speed_change_description"), "The actual increase to apply");

        /// <summary>
        /// "Uguuuuuuuu..."
        /// </summary>
        public static LocalisableString NightcoreDescription => new TranslatableString(getKey(@"nightcore_description"), "Uguuuuuuuu...");

        /// <summary>
        /// "Keys appear out of nowhere!"
        /// </summary>
        public static LocalisableString FadeInDescription => new TranslatableString(getKey(@"fade_in_description"), "Keys appear out of nowhere!");

        /// <summary>
        /// "Coverage"
        /// </summary>
        public static LocalisableString PlayfieldCoverCoverage => new TranslatableString(getKey(@"playfield_cover_coverage"), "Coverage");

        /// <summary>
        /// "The proportion of playfield height that notes will be hidden for."
        /// </summary>
        public static LocalisableString PlayfieldCoverCoverageDescription => new TranslatableString(getKey(@"playfield_cover_coverage_description"), "The proportion of playfield height that notes will be hidden for.");

        /// <summary>
        /// "Play with no approach circles and fading circles/sliders."
        /// </summary>
        public static LocalisableString OsuHiddenDescription => new TranslatableString(getKey(@"osu_hidden_description"), "Play with no approach circles and fading circles/sliders.");

        /// <summary>
        /// "Only fade approach circles"
        /// </summary>
        public static LocalisableString OsuHiddenOnlyFade => new TranslatableString(getKey(@"osu_hidden_only_fade"), "Only fade approach circles");

        /// <summary>
        /// "The main body will not fade when enabled."
        /// </summary>
        public static LocalisableString OsuHiddenOnlyFadeDescription => new TranslatableString(getKey(@"osu_hidden_only_fade_description"), "The main body will not fade when enabled.");

        /// <summary>
        /// "Beats fade out before you hit them!"
        /// </summary>
        public static LocalisableString TaikoHiddenDescription => new TranslatableString(getKey(@"taiko_hidden_description"), "Beats fade out before you hit them!");

        /// <summary>
        /// "Keys fade out before you hit them!"
        /// </summary>
        public static LocalisableString ManiaHiddenDescription => new TranslatableString(getKey(@"mania_hidden_description"), "Keys fade out before you hit them!");

        /// <summary>
        /// "Play with fading fruits."
        /// </summary>
        public static LocalisableString CatchHiddenDescription => new TranslatableString(getKey(@"catch_hidden_description"), "Play with fading fruits.");

        /// <summary>
        /// "Restricted view area."
        /// </summary>
        public static LocalisableString FlashlightDescription => new TranslatableString(getKey(@"flashlight_descriptionn"), "Restricted view area.");

        /// <summary>
        /// "Flashlight size"
        /// </summary>
        public static LocalisableString FlashlightSizeMultiplier => new TranslatableString(getKey(@"flashlight_size_multiplier"), "Flashlight size");

        /// <summary>
        /// "Multiplier applied to the default flashlight size."
        /// </summary>
        public static LocalisableString FlashlightSizeMultiplierDescription => new TranslatableString(getKey(@"flashlight_size_multiplier_description"), "Multiplier applied to the default flashlight size.");

        /// <summary>
        /// "Changed size based on combo"
        /// </summary>
        public static LocalisableString FlashlightComboBasedSize => new TranslatableString(getKey(@"flashlight_combo_based_size"), "Changed size based on combo");

        /// <summary>
        /// "Decrease the flashlight size as combo increases."
        /// </summary>
        public static LocalisableString FlashlightComboBasedSizeDescription => new TranslatableString(getKey(@"flashlight_combo_based_size_description"), "Decrease the flashlight size as combo increases.");

        /// <summary>
        /// "Play with blinds on your screen."
        /// </summary>
        public static LocalisableString OsuBlindsDescription => new TranslatableString(getKey(@"osu_blinds_description"), "Play with blinds on your screen.");

        /// <summary>
        /// "Once you start a slider, follow precisely or get a miss."
        /// </summary>
        public static LocalisableString OsuStrictTrackingDescription => new TranslatableString(getKey(@"osu_strict_tracking_description"), "Once you start a slider, follow precisely or get a miss.");

        /// <summary>
        /// "Fail if your accuracy drops too low!"
        /// </summary>
        public static LocalisableString AccuracyChallengeDescription => new TranslatableString(getKey(@"accuracy_challenge_description"), "Fail if your accuracy drops too low!");

        /// <summary>
        /// "Minimum accuracy"
        /// </summary>
        public static LocalisableString AccuracyChallengeMinAcc => new TranslatableString(getKey(@"accuracy_challenge_min_acc"), "Minimum accuracy");

        /// <summary>
        /// "Trigger a failure if your accuracy foes below this value."
        /// </summary>
        public static LocalisableString AccuracyChallengeMinAccDescription => new TranslatableString(getKey(@"accuracy_challenge_min_acc_description"), "Trigger a failure if your accuracy foes below this value.");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
