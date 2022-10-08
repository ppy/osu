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
        /// "General"
        /// </summary>
        public static LocalisableString GeneralHeader => new TranslatableString(getKey(@"general_header"), @"General");

        /// <summary>
        /// "拖动时旋转光标"
        /// </summary>
        public static LocalisableString CursorRotation => new TranslatableString(getKey(@"cursor_rotation"), @"拖动时旋转光标");

        /// <summary>
        /// "Menu cursor size"
        /// </summary>
        public static LocalisableString MenuCursorSize => new TranslatableString(getKey(@"menu_cursor_size"), @"Menu cursor size");

        /// <summary>
        /// "视差"
        /// </summary>
        public static LocalisableString Parallax => new TranslatableString(getKey(@"parallax"), @"视差");

        /// <summary>
        /// "Hold-to-confirm activation time"
        /// </summary>
        public static LocalisableString HoldToConfirmActivationTime => new TranslatableString(getKey(@"hold_to_confirm_activation_time"), @"Hold-to-confirm activation time");

        /// <summary>
        /// "Main Menu"
        /// </summary>
        public static LocalisableString MainMenuHeader => new TranslatableString(getKey(@"main_menu_header"), @"Main Menu");

        /// <summary>
        /// "界面声音"
        /// </summary>
        public static LocalisableString InterfaceVoices => new TranslatableString(getKey(@"interface_voices"), @"界面声音");

        /// <summary>
        /// "osu! 主题音乐"
        /// </summary>
        public static LocalisableString OsuMusicTheme => new TranslatableString(getKey(@"osu_music_theme"), @"osu! 主题音乐");

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
        /// "该设置仅对 osu! supporter 有效"
        /// </summary>
        public static LocalisableString NotSupporterNote => new TranslatableString(getKey(@"not_supporter_note"), @"该设置仅对 osu! supporter 有效");

        /// <summary>
        /// "Song Select"
        /// </summary>
        public static LocalisableString SongSelectHeader => new TranslatableString(getKey(@"song_select_header"), @"Song Select");

        /// <summary>
        /// "按住鼠标右键快速滚动"
        /// </summary>
        public static LocalisableString RightMouseScroll => new TranslatableString(getKey(@"right_mouse_scroll"), @"按住鼠标右键快速滚动");

        /// <summary>
        /// "显示转谱"
        /// </summary>
        public static LocalisableString ShowConvertedBeatmaps => new TranslatableString(getKey(@"show_converted_beatmaps"), @"显示转谱");

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

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
