// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public class PlayerSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.PlayerSettings";

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
