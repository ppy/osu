// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class BeatmapOffsetControlStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.BeatmapOffsetControl";

        /// <summary>
        /// "Audio offset (this beatmap)"
        /// </summary>
        public static LocalisableString AudioOffsetThisBeatmap => new TranslatableString(getKey(@"beatmap_offset"), @"Audio offset (this beatmap)");

        /// <summary>
        /// "Previous play:"
        /// </summary>
        public static LocalisableString PreviousPlay => new TranslatableString(getKey(@"previous_play"), @"Previous play:");

        /// <summary>
        /// "Previous play too short to use for calibration"
        /// </summary>
        public static LocalisableString PreviousPlayTooShortToUseForCalibration => new TranslatableString(getKey(@"previous_play_too_short_to_use_for_calibration"), @"Previous play too short to use for calibration");

        /// <summary>
        /// "Calibrate using last play"
        /// </summary>
        public static LocalisableString CalibrateUsingLastPlay => new TranslatableString(getKey(@"calibrate_using_last_play"), @"Calibrate using last play");

        /// <summary>
        /// "(hit objects appear later)"
        /// </summary>
        public static LocalisableString HitObjectsAppearLater => new TranslatableString(getKey(@"hit_objects_appear_later"), @"(hit objects appear later)");

        /// <summary>
        /// "(hit objects appear earlier)"
        /// </summary>
        public static LocalisableString HitObjectsAppearEarlier => new TranslatableString(getKey(@"hit_objects_appear_earlier"), @"(hit objects appear earlier)");

        /// <summary>
        /// "Beatmap offset was adjusted to {0} ms."
        /// </summary>
        public static LocalisableString BeatmapOffsetWasAdjustedTo(string offset) => new TranslatableString(getKey(@"beatmap_offset_was_adjusted_to"), @"Beatmap offset was adjusted to {0} ms.", offset);

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
