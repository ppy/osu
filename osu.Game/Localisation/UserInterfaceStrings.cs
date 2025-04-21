// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class UserInterfaceStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.UserInterface";

        /// <summary>
        /// "User Interface"
        /// </summary>
        public static LocalisableString UserInterfaceSectionHeader => new TranslatableString(getKey(@"user_interface_section_header"), @"User Interface");

        /// <summary>
        /// "Rotate cursor when dragging"
        /// </summary>
        public static LocalisableString CursorRotation => new TranslatableString(getKey(@"cursor_rotation"), @"Rotate cursor when dragging");

        /// <summary>
        /// "Always use gameplay cursor"
        /// </summary>
        public static LocalisableString GameplayCursorInMenus => new TranslatableString(getKey(@"gameplay_cursor_in_menus"), @"Always use gameplay cursor");

        /// <summary>
        /// "Menu cursor size"
        /// </summary>
        public static LocalisableString MenuCursorSize => new TranslatableString(getKey(@"menu_cursor_size"), @"Menu cursor size");

        /// <summary>
        /// "Menu tips"
        /// </summary>
        public static LocalisableString ShowMenuTips => new TranslatableString(getKey(@"show_menu_tips"), @"Menu tips");

        /// <summary>
        /// "Menu news"
        /// </summary>
        public static LocalisableString ShowMenuNews => new TranslatableString(getKey(@"show_menu_news"), @"Menu news");

        /// <summary>
        /// "Parallax"
        /// </summary>
        public static LocalisableString Parallax => new TranslatableString(getKey(@"parallax"), @"Parallax");

        /// <summary>
        /// "Hold-to-confirm activation time"
        /// </summary>
        public static LocalisableString HoldToConfirmActivationTime => new TranslatableString(getKey(@"hold_to_confirm_activation_time"), @"Hold-to-confirm activation time");

        /// <summary>
        /// "Main Menu"
        /// </summary>
        public static LocalisableString MainMenuHeader => new TranslatableString(getKey(@"main_menu_header"), @"Main Menu");

        /// <summary>
        /// "Interface voices"
        /// </summary>
        public static LocalisableString InterfaceVoices => new TranslatableString(getKey(@"interface_voices"), @"Interface voices");

        /// <summary>
        /// "Female interface voices"
        /// </summary>
        public static LocalisableString MenuVoiceFemale => new TranslatableString(getKey(@"menu_voice_female"), @"Female interface voices");

        /// <summary>
        /// "osu! music theme"
        /// </summary>
        public static LocalisableString OsuMusicTheme => new TranslatableString(getKey(@"osu_music_theme"), @"osu! music theme");

        /// <summary>
        /// "Intro sequence"
        /// </summary>
        public static LocalisableString IntroSequence => new TranslatableString(getKey(@"intro_sequence"), @"Intro sequence");

        /// <summary>
        /// "Background source"
        /// </summary>
        public static LocalisableString BackgroundSource => new TranslatableString(getKey(@"background_source"), @"Background source");

        /// <summary>
        /// "Seasonal backgrounds"
        /// </summary>
        public static LocalisableString SeasonalBackgrounds => new TranslatableString(getKey(@"seasonal_backgrounds"), @"Seasonal backgrounds");

        /// <summary>
        /// "Changes to this setting will only apply with an active osu!supporter tag."
        /// </summary>
        public static LocalisableString NotSupporterNote => new TranslatableString(getKey(@"not_supporter_note"), @"Changes to this setting will only apply with an active osu!supporter tag.");

        /// <summary>
        /// "Song Select"
        /// </summary>
        public static LocalisableString SongSelectHeader => new TranslatableString(getKey(@"song_select_header"), @"Song Select");

        /// <summary>
        /// "Right mouse drag to absolute scroll"
        /// </summary>
        public static LocalisableString RightMouseScroll => new TranslatableString(getKey(@"right_mouse_scroll"), @"Right mouse drag to absolute scroll");

        /// <summary>
        /// "Show converted beatmaps"
        /// </summary>
        public static LocalisableString ShowConvertedBeatmaps => new TranslatableString(getKey(@"show_converted_beatmaps"), @"Show converted beatmaps");

        /// <summary>
        /// "Show all possible rulesets"
        /// </summary>
        public static LocalisableString ClassicRulesetFiltering => new TranslatableString(getKey(@"classic_ruleset_filtering"), @"Show all possible rulesets");

        /// <summary>
        /// "Display beatmaps from"
        /// </summary>
        public static LocalisableString StarsMinimum => new TranslatableString(getKey(@"stars_minimum"), @"Display beatmaps from");

        /// <summary>
        /// "up to"
        /// </summary>
        public static LocalisableString StarsMaximum => new TranslatableString(getKey(@"stars_maximum"), @"up to");

        /// <summary>
        /// "Random selection algorithm"
        /// </summary>
        public static LocalisableString RandomSelectionAlgorithm => new TranslatableString(getKey(@"random_selection_algorithm"), @"Random selection algorithm");

        /// <summary>
        /// "Mod select hotkey style"
        /// </summary>
        public static LocalisableString ModSelectHotkeyStyle => new TranslatableString(getKey(@"mod_select_hotkey_style"), @"Mod select hotkey style");

        /// <summary>
        /// "Automatically focus search text box in mod select"
        /// </summary>
        public static LocalisableString ModSelectTextSearchStartsActive => new TranslatableString(getKey(@"mod_select_text_search_starts_active"), @"Automatically focus search text box in mod select");

        /// <summary>
        /// "no limit"
        /// </summary>
        public static LocalisableString NoLimit => new TranslatableString(getKey(@"no_limit"), @"no limit");

        /// <summary>
        /// "Beatmap (with storyboard / video)"
        /// </summary>
        public static LocalisableString BeatmapWithStoryboard => new TranslatableString(getKey(@"beatmap_with_storyboard"), @"Beatmap (with storyboard / video)");

        /// <summary>
        /// "Always"
        /// </summary>
        public static LocalisableString AlwaysSeasonalBackground => new TranslatableString(getKey(@"always_seasonal_backgrounds"), @"Always");

        /// <summary>
        /// "Never"
        /// </summary>
        public static LocalisableString NeverSeasonalBackground => new TranslatableString(getKey(@"never_seasonal_backgrounds"), @"Never");

        /// <summary>
        /// "Sometimes"
        /// </summary>
        public static LocalisableString SometimesSeasonalBackground => new TranslatableString(getKey(@"sometimes_seasonal_backgrounds"), @"Sometimes");

        /// <summary>
        /// "Sequential"
        /// </summary>
        public static LocalisableString SequentialHotkeyStyle => new TranslatableString(getKey(@"mods_sequential_hotkeys"), @"Sequential");

        /// <summary>
        /// "Classic"
        /// </summary>
        public static LocalisableString ClassicHotkeyStyle => new TranslatableString(getKey(@"mods_classic_hotkeys"), @"Classic");

        /// <summary>
        /// "Never repeat"
        /// </summary>
        public static LocalisableString NeverRepeat => new TranslatableString(getKey(@"never_repeat_random"), @"Never repeat");

        /// <summary>
        /// "True Random"
        /// </summary>
        public static LocalisableString TrueRandom => new TranslatableString(getKey(@"true_random"), @"True Random");

        /// <summary>
        /// "Apply audio filter when window is inactive"
        /// </summary>
        public static LocalisableString AudioInactiveFilter => new TranslatableString(getKey(@"audio_inactive_filter"), @"Apply audio filter when window is inactive");

        /// <summary>
        /// "Override beatmap hitsounds with default"
        /// </summary>
        public static LocalisableString OverrideBeatmapHitsounds => new TranslatableString(getKey(@"override_beatmap_hitsounds"), @"Override beatmap hitsounds with default");

        /// <summary>
        /// "Dimmed background color"
        /// </summary>
        public static LocalisableString BackgroundColour => new TranslatableString(getKey(@"background_colour"), @"Dimmed background color");

        /// <summary>
        /// "Use classic layout on key overlay"
        /// </summary>
        public static LocalisableString KeyOverlayLayoutClassic => new TranslatableString(getKey(@"key_overlay_layout_classic"), @"Use classic layout on key overlay");

        /// <summary>
        /// "Cursor trail length"
        /// </summary>
        public static LocalisableString CursorTrailLength => new TranslatableString(getKey(@"cursor_trail_length"), @"Cursor trail length");

        /// <summary>
        /// "Use trail on menu cursor"
        /// </summary>
        public static LocalisableString ShowLazerCursorTrail => new TranslatableString(getKey(@"show_lazer_cursor_trail"), @"Use trail on menu cursor");

        /// <summary>
        /// "Use trail on gameplay cursor"
        /// </summary>
        public static LocalisableString ShowCursorTrail => new TranslatableString(getKey(@"show_cursor_trail"), @"Use trail on gameplay cursor");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
