// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class PlayerLoaderStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.PlayerLoader";

        /// <summary>
        /// "This beatmap contains scenes with rapidly flashing colours"
        /// </summary>
        public static LocalisableString EpilepsyWarningTitle => new TranslatableString(getKey(@"epilepsy_warning_title"), @"This beatmap contains scenes with rapidly flashing colours");

        /// <summary>
        /// "Please take caution if you are affected by epilepsy."
        /// </summary>
        public static LocalisableString EpilepsyWarningContent => new TranslatableString(getKey(@"epilepsy_warning_content"), @"Please take caution if you are affected by epilepsy.");

        /// <summary>
        /// "This beatmap is loved"
        /// </summary>
        public static LocalisableString LovedBeatmapDisclaimerTitle => new TranslatableString(getKey(@"loved_beatmap_disclaimer_title"), @"This beatmap is loved");

        /// <summary>
        /// "No performance points will be awarded.
        /// Leaderboards may be reset."
        /// </summary>
        public static LocalisableString LovedBeatmapDisclaimerContent => new TranslatableString(getKey(@"loved_beatmap_disclaimer_content"), @"No performance points will be awarded.
Leaderboards may be reset.");

        /// <summary>
        /// "This beatmap is qualified"
        /// </summary>
        public static LocalisableString QualifiedBeatmapDisclaimerTitle => new TranslatableString(getKey(@"qualified_beatmap_disclaimer_title"), @"This beatmap is qualified");

        /// <summary>
        /// "No performance points will be awarded.
        /// Leaderboards will be reset when the beatmap is ranked."
        /// </summary>
        public static LocalisableString QualifiedBeatmapDisclaimerContent => new TranslatableString(getKey(@"qualified_beatmap_disclaimer_content"), @"No performance points will be awarded.
Leaderboards will be reset when the beatmap is ranked.");

        /// <summary>
        /// "Mapper"
        /// </summary>
        public static LocalisableString ShowInfoMapper => new TranslatableString(getKey(@"show_info_mapper"), @"Mapper");

        /// <summary>
        /// "Playback"
        /// </summary>
        public static LocalisableString PlaybackTitle => new TranslatableString(getKey(@"playback_title"), @"Playback");

        /// <summary>
        /// "Visual Settings"
        /// </summary>
        public static LocalisableString VisualSettingsTitle => new TranslatableString(getKey(@"visual_settings_title"), @"Visual Settings");

        /// <summary>
        /// "Audio Settings"
        /// </summary>
        public static LocalisableString AudioSettingsTitle => new TranslatableString(getKey(@"audio_settings_title"), @"Audio Settings");

        /// <summary>
        /// "Input Settings"
        /// </summary>
        public static LocalisableString InputSettingsTitle => new TranslatableString(getKey(@"input_settings_title"), @"Input Settings");

        /// <summary>
        /// "Analysis Settings"
        /// </summary>
        public static LocalisableString AnalysisSettingsTitle => new TranslatableString(getKey(@"analysis_settings_title"), @"Analysis Settings");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
