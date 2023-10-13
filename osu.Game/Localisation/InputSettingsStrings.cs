// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class InputSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.InputSettings";

        /// <summary>
        /// "Input"
        /// </summary>
        public static LocalisableString InputSectionHeader => new TranslatableString(getKey(@"input_section_header"), @"Input");

        /// <summary>
        /// "Global"
        /// </summary>
        public static LocalisableString GlobalKeyBindingHeader => new TranslatableString(getKey(@"global_key_binding_header"), @"Global");

        /// <summary>
        /// "Overlays"
        /// </summary>
        public static LocalisableString OverlaysSection => new TranslatableString(getKey(@"overlays_section"), @"Overlays");

        /// <summary>
        /// "Song Select"
        /// </summary>
        public static LocalisableString SongSelectSection => new TranslatableString(getKey(@"song_select_section"), @"Song Select");

        /// <summary>
        /// "In Game"
        /// </summary>
        public static LocalisableString InGameSection => new TranslatableString(getKey(@"in_game_section"), @"In Game");

        /// <summary>
        /// "Replay"
        /// </summary>
        public static LocalisableString ReplaySection => new TranslatableString(getKey(@"replay_section"), @"Replay");

        /// <summary>
        /// "Audio"
        /// </summary>
        public static LocalisableString AudioSection => new TranslatableString(getKey(@"audio_section"), @"Audio");

        /// <summary>
        /// "Editor"
        /// </summary>
        public static LocalisableString EditorSection => new TranslatableString(getKey(@"editor_section"), @"Editor");

        /// <summary>
        /// "Reset all bindings in section"
        /// </summary>
        public static LocalisableString ResetSectionButton => new TranslatableString(getKey(@"reset_section_button"), @"Reset all bindings in section");

        /// <summary>
        /// "key configuration"
        /// </summary>
        public static LocalisableString KeyBindingPanelHeader => new TranslatableString(getKey(@"key_binding_panel_header"), @"key configuration");

        /// <summary>
        /// "Customise your keys!"
        /// </summary>
        public static LocalisableString KeyBindingPanelDescription => new TranslatableString(getKey(@"key_binding_panel_description"), @"Customise your keys!");

        /// <summary>
        /// "The binding you&#39;ve selected conflicts with another existing binding."
        /// </summary>
        public static LocalisableString KeyBindingConflictDetected => new TranslatableString(getKey(@"key_binding_conflict_detected"), @"The binding you've selected conflicts with another existing binding.");

        /// <summary>
        /// "Keep existing"
        /// </summary>
        public static LocalisableString KeepExistingBinding => new TranslatableString(getKey(@"keep_existing_binding"), @"Keep existing");

        /// <summary>
        /// "Apply new"
        /// </summary>
        public static LocalisableString ApplyNewBinding => new TranslatableString(getKey(@"apply_new_binding"), @"Apply new");

        /// <summary>
        /// "(none)"
        /// </summary>
        public static LocalisableString ActionHasNoKeyBinding => new TranslatableString(getKey(@"action_has_no_key_binding"), @"(none)");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
