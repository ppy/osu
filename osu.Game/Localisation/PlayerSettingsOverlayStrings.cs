// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class PlayerSettingsOverlayStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.PlaybackSettings";

        /// <summary>
        /// "Step backward one frame"
        /// </summary>
        public static LocalisableString StepBackward => new TranslatableString(getKey(@"step_backward_frame"), @"Step backward one frame");

        /// <summary>
        /// "Step forward one frame"
        /// </summary>
        public static LocalisableString StepForward => new TranslatableString(getKey(@"step_forward_frame"), @"Step forward one frame");

        /// <summary>
        /// "Seek backward {0} seconds"
        /// </summary>
        public static LocalisableString SeekBackwardSeconds(double arg0) => new TranslatableString(getKey(@"seek_backward_seconds"), @"Seek backward {0} seconds", arg0);

        /// <summary>
        /// "Seek forward {0} seconds"
        /// </summary>
        public static LocalisableString SeekForwardSeconds(double arg0) => new TranslatableString(getKey(@"seek_forward_seconds"), @"Seek forward {0} seconds", arg0);

        /// <summary>
        /// "Playback speed"
        /// </summary>
        public static LocalisableString PlaybackSpeed => new TranslatableString(getKey(@"playback_speed"), @"Playback speed");

        /// <summary>
        /// "Show click markers"
        /// </summary>
        public static LocalisableString ShowClickMarkers => new TranslatableString(getKey(@"show_click_markers"), @"Show click markers");

        /// <summary>
        /// "Show frame markers"
        /// </summary>
        public static LocalisableString ShowFrameMarkers => new TranslatableString(getKey(@"show_frame_markers"), @"Show frame markers");

        /// <summary>
        /// "Show cursor path"
        /// </summary>
        public static LocalisableString ShowCursorPath => new TranslatableString(getKey(@"show_cursor_path"), @"Show cursor path");

        /// <summary>
        /// "Hide gameplay cursor"
        /// </summary>
        public static LocalisableString HideGameplayCursor => new TranslatableString(getKey(@"hide_gameplay_cursor"), @"Hide gameplay cursor");

        /// <summary>
        /// "Display length"
        /// </summary>
        public static LocalisableString DisplayLength => new TranslatableString(getKey(@"display_length"), @"Display length");

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
