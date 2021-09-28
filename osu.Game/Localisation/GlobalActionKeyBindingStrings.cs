// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class GlobalActionKeyBindingStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.GlobalActionKeyBinding";

        /// <summary>
        /// "Toggle chat overlay"
        /// </summary>
        public static LocalisableString ToggleChat => new TranslatableString(getKey(@"toggle_chat"), @"切换聊天");

        /// <summary>
        /// "Toggle social overlay"
        /// </summary>
        public static LocalisableString ToggleSocial => new TranslatableString(getKey(@"toggle_social"), @"切换主页");

        /// <summary>
        /// "Reset input settings"
        /// </summary>
        public static LocalisableString ResetInputSettings => new TranslatableString(getKey(@"reset_input_settings"), @"重置输入设置");

        /// <summary>
        /// "Toggle toolbar"
        /// </summary>
        public static LocalisableString ToggleToolbar => new TranslatableString(getKey(@"toggle_toolbar"), @"切换工具栏");

        /// <summary>
        /// "Toggle settings"
        /// </summary>
        public static LocalisableString ToggleSettings => new TranslatableString(getKey(@"toggle_settings"), @"切换设置");

        /// <summary>
        /// "Toggle beatmap listing"
        /// </summary>
        public static LocalisableString ToggleBeatmapListing => new TranslatableString(getKey(@"toggle_beatmap_listing"), @"切换谱面列表");

        /// <summary>
        /// "Increase volume"
        /// </summary>
        public static LocalisableString IncreaseVolume => new TranslatableString(getKey(@"increase_volume"), @"增加音量");

        /// <summary>
        /// "Decrease volume"
        /// </summary>
        public static LocalisableString DecreaseVolume => new TranslatableString(getKey(@"decrease_volume"), @"降低音量");

        /// <summary>
        /// "Toggle mute"
        /// </summary>
        public static LocalisableString ToggleMute => new TranslatableString(getKey(@"toggle_mute"), @"切换静音");

        /// <summary>
        /// "Skip cutscene"
        /// </summary>
        public static LocalisableString SkipCutscene => new TranslatableString(getKey(@"skip_cutscene"), @"跳过过场");

        /// <summary>
        /// "Quick retry (hold)"
        /// </summary>
        public static LocalisableString QuickRetry => new TranslatableString(getKey(@"quick_retry"), @"快速重试 (按住)");

        /// <summary>
        /// "Take screenshot"
        /// </summary>
        public static LocalisableString TakeScreenshot => new TranslatableString(getKey(@"take_screenshot"), @"截图");

        /// <summary>
        /// "Toggle gameplay mouse buttons"
        /// </summary>
        public static LocalisableString ToggleGameplayMouseButtons => new TranslatableString(getKey(@"toggle_gameplay_mouse_buttons"), @"切换游戏内鼠标按键");

        /// <summary>
        /// "Back"
        /// </summary>
        public static LocalisableString Back => new TranslatableString(getKey(@"back"), @"返回");

        /// <summary>
        /// "Increase scroll speed"
        /// </summary>
        public static LocalisableString IncreaseScrollSpeed => new TranslatableString(getKey(@"increase_scroll_speed"), @"增加滚动速度");

        /// <summary>
        /// "Decrease scroll speed"
        /// </summary>
        public static LocalisableString DecreaseScrollSpeed => new TranslatableString(getKey(@"decrease_scroll_speed"), @"降低滚动速度");

        /// <summary>
        /// "Select"
        /// </summary>
        public static LocalisableString Select => new TranslatableString(getKey(@"select"), @"选择");

        /// <summary>
        /// "Quick exit (hold)"
        /// </summary>
        public static LocalisableString QuickExit => new TranslatableString(getKey(@"quick_exit"), @"快速退出 (按住)");

        /// <summary>
        /// "Next track"
        /// </summary>
        public static LocalisableString MusicNext => new TranslatableString(getKey(@"music_next"), @"下一首");

        /// <summary>
        /// "Previous track"
        /// </summary>
        public static LocalisableString MusicPrev => new TranslatableString(getKey(@"music_prev"), @"上一首");

        /// <summary>
        /// "Play / pause"
        /// </summary>
        public static LocalisableString MusicPlay => new TranslatableString(getKey(@"music_play"), @"切换暂停");

        /// <summary>
        /// "Toggle now playing overlay"
        /// </summary>
        public static LocalisableString ToggleNowPlaying => new TranslatableString(getKey(@"toggle_now_playing"), @"切换正在播放");

        /// <summary>
        /// "Previous selection"
        /// </summary>
        public static LocalisableString SelectPrevious => new TranslatableString(getKey(@"select_previous"), @"选择上一个");

        /// <summary>
        /// "Next selection"
        /// </summary>
        public static LocalisableString SelectNext => new TranslatableString(getKey(@"select_next"), @"选择下一个");

        /// <summary>
        /// "Home"
        /// </summary>
        public static LocalisableString Home => new TranslatableString(getKey(@"home"), @"主页");

        /// <summary>
        /// "Toggle notifications"
        /// </summary>
        public static LocalisableString ToggleNotifications => new TranslatableString(getKey(@"toggle_notifications"), @"切换通知");

        /// <summary>
        /// "Pause gameplay"
        /// </summary>
        public static LocalisableString PauseGameplay => new TranslatableString(getKey(@"pause_gameplay"), @"暂停游戏");

        /// <summary>
        /// "Setup mode"
        /// </summary>
        public static LocalisableString EditorSetupMode => new TranslatableString(getKey(@"editor_setup_mode"), @"谱面设置模式");

        /// <summary>
        /// "Compose mode"
        /// </summary>
        public static LocalisableString EditorComposeMode => new TranslatableString(getKey(@"editor_compose_mode"), @"物件排布模式");

        /// <summary>
        /// "Design mode"
        /// </summary>
        public static LocalisableString EditorDesignMode => new TranslatableString(getKey(@"editor_design_mode"), @"谱面设计模式");

        /// <summary>
        /// "Timing mode"
        /// </summary>
        public static LocalisableString EditorTimingMode => new TranslatableString(getKey(@"editor_timing_mode"), @"Timing模式");

        /// <summary>
        /// "Cycle grid display mode"
        /// </summary>
        public static LocalisableString EditorCycleGridDisplayMode => new TranslatableString(getKey(@"editor_cycle_grid_display_mode"), @"Cycle grid display mode");

        /// <summary>
        /// "Hold for HUD"
        /// </summary>
        public static LocalisableString HoldForHUD => new TranslatableString(getKey(@"hold_for_hud"), @"按住显示HUD");

        /// <summary>
        /// "Random skin"
        /// </summary>
        public static LocalisableString RandomSkin => new TranslatableString(getKey(@"random_skin"), @"随机皮肤");

        /// <summary>
        /// "Pause / resume replay"
        /// </summary>
        public static LocalisableString TogglePauseReplay => new TranslatableString(getKey(@"toggle_pause_replay"), @"暂停/播放回放");

        /// <summary>
        /// "Toggle in-game interface"
        /// </summary>
        public static LocalisableString ToggleInGameInterface => new TranslatableString(getKey(@"toggle_in_game_interface"), @"切换游戏界面");

        /// <summary>
        /// "Toggle Mod Select"
        /// </summary>
        public static LocalisableString ToggleModSelection => new TranslatableString(getKey(@"toggle_mod_selection"), @"切换Mod选择");

        /// <summary>
        /// "Random"
        /// </summary>
        public static LocalisableString SelectNextRandom => new TranslatableString(getKey(@"select_next_random"), @"随机选择");

        /// <summary>
        /// "Rewind"
        /// </summary>
        public static LocalisableString SelectPreviousRandom => new TranslatableString(getKey(@"select_previous_random"), @"撤销随机");

        /// <summary>
        /// "Beatmap Options"
        /// </summary>
        public static LocalisableString ToggleBeatmapOptions => new TranslatableString(getKey(@"toggle_beatmap_options"), @"谱面选项");

        /// <summary>
        /// "Verify mode"
        /// </summary>
        public static LocalisableString EditorVerifyMode => new TranslatableString(getKey(@"editor_verify_mode"), @"验证模式");

        /// <summary>
        /// "Nudge selection left"
        /// </summary>
        public static LocalisableString EditorNudgeLeft => new TranslatableString(getKey(@"editor_nudge_left"), @"在时间线上左移物件");

        /// <summary>
        /// "Nudge selection right"
        /// </summary>
        public static LocalisableString EditorNudgeRight => new TranslatableString(getKey(@"editor_nudge_right"), @"在时间线上右移物件");

        /// <summary>
        /// "Toggle skin editor"
        /// </summary>
        public static LocalisableString ToggleSkinEditor => new TranslatableString(getKey(@"toggle_skin_editor"), @"切换皮肤编辑器");

        /// <summary>
        /// "Previous volume meter"
        /// </summary>
        public static LocalisableString PreviousVolumeMeter => new TranslatableString(getKey(@"previous_volume_meter"), @"上一个音量条");

        /// <summary>
        /// "Next volume meter"
        /// </summary>
        public static LocalisableString NextVolumeMeter => new TranslatableString(getKey(@"next_volume_meter"), @"下一个音量条");

        /// <summary>
        /// "Seek replay forward"
        /// </summary>
        public static LocalisableString SeekReplayForward => new TranslatableString(getKey(@"seek_replay_forward"), @"向前快进回放");

        /// <summary>
        /// "Seek replay backward"
        /// </summary>
        public static LocalisableString SeekReplayBackward => new TranslatableString(getKey(@"seek_replay_backward"), @"向后快退回放");

        /// <summary>
        /// "Toggle chat focus"
        /// </summary>
        public static LocalisableString ToggleChatFocus => new TranslatableString(getKey(@"toggle_chat_focus"), @"切换聊天");

        private static string getKey(string key) => $"{prefix}:{key}";
    }
}
