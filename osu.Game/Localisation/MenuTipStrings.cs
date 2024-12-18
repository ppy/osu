﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class MenuTipStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.MenuTip";

        /// <summary>
        /// "Press Ctrl-T anywhere in the game to toggle the toolbar!"
        /// </summary>
        public static LocalisableString ToggleToolbarShortcut => new TranslatableString(getKey(@"toggle_toolbar_shortcut"), @"Press Ctrl-T anywhere in the game to toggle the toolbar!");

        /// <summary>
        /// "Press Ctrl-O anywhere in the game to access settings!"
        /// </summary>
        public static LocalisableString GameSettingsShortcut => new TranslatableString(getKey(@"game_settings_shortcut"), @"Press Ctrl-O anywhere in the game to access settings!");

        /// <summary>
        /// "All settings are dynamic and take effect in real-time. Try changing the skin while watching autoplay!"
        /// </summary>
        public static LocalisableString DynamicSettings => new TranslatableString(getKey(@"dynamic_settings"), @"All settings are dynamic and take effect in real-time. Try changing the skin while watching autoplay!");

        /// <summary>
        /// "New features are coming online every update. Make sure to stay up-to-date!"
        /// </summary>
        public static LocalisableString NewFeaturesAreComingOnline => new TranslatableString(getKey(@"new_features_are_coming_online"), @"New features are coming online every update. Make sure to stay up-to-date!");

        /// <summary>
        /// "If you find the UI too large or small, try adjusting UI scale in settings!"
        /// </summary>
        public static LocalisableString UIScalingSettings => new TranslatableString(getKey(@"ui_scaling_settings"), @"If you find the UI too large or small, try adjusting UI scale in settings!");

        /// <summary>
        /// "Try adjusting the &quot;Screen Scaling&quot; mode to change your gameplay or UI area, even in fullscreen!"
        /// </summary>
        public static LocalisableString ScreenScalingSettings => new TranslatableString(getKey(@"screen_scaling_settings"), @"Try adjusting the ""Screen Scaling"" mode to change your gameplay or UI area, even in fullscreen!");

        /// <summary>
        /// "What used to be &quot;osu!direct&quot; is available to all users just like on the website. You can access it anywhere using Ctrl-B!"
        /// </summary>
        public static LocalisableString FreeOsuDirect => new TranslatableString(getKey(@"free_osu_direct"), @"What used to be ""osu!direct"" is available to all users just like on the website. You can access it anywhere using Ctrl-B!");

        /// <summary>
        /// "Seeking in replays is available by dragging on the progress bar at the bottom of the screen or by using the left and right arrow keys!"
        /// </summary>
        public static LocalisableString ReplaySeeking => new TranslatableString(getKey(@"replay_seeking"), @"Seeking in replays is available by dragging on the progress bar at the bottom of the screen or by using the left and right arrow keys!");

        /// <summary>
        /// "Try scrolling right in mod select to find a bunch of new fun mods!"
        /// </summary>
        public static LocalisableString TryNewMods => new TranslatableString(getKey(@"try_new_mods"), @"Try scrolling right in mod select to find a bunch of new fun mods!");

        /// <summary>
        /// "Most of the web content (profiles, rankings, etc.) are available natively in-game from the icons on the toolbar!"
        /// </summary>
        public static LocalisableString EmbeddedWebContent => new TranslatableString(getKey(@"embedded_web_content"), @"Most of the web content (profiles, rankings, etc.) are available natively in-game from the icons on the toolbar!");

        /// <summary>
        /// "Get more details, hide or delete a beatmap by right-clicking on its panel at song select!"
        /// </summary>
        public static LocalisableString BeatmapRightClick => new TranslatableString(getKey(@"beatmap_right_click"), @"Get more details, hide or delete a beatmap by right-clicking on its panel at song select!");

        /// <summary>
        /// "Check out the &quot;playlists&quot; system, which lets users create their own custom and permanent leaderboards!"
        /// </summary>
        public static LocalisableString DiscoverPlaylists => new TranslatableString(getKey(@"discover_playlists"), @"Check out the ""playlists"" system, which lets users create their own custom and permanent leaderboards!");

        /// <summary>
        /// "Toggle advanced frame / thread statistics with Ctrl-F11!"
        /// </summary>
        public static LocalisableString ToggleAdvancedFPSCounter => new TranslatableString(getKey(@"toggle_advanced_fps_counter"), @"Toggle advanced frame / thread statistics with Ctrl-F11!");

        /// <summary>
        /// "You can pause during a replay by pressing Space!"
        /// </summary>
        public static LocalisableString ReplayPausing => new TranslatableString(getKey(@"replay_pausing"), @"You can pause during a replay by pressing Space!");

        /// <summary>
        /// "Most of the hotkeys in the game are configurable and can be changed to anything you want. Check the bindings panel under input settings!"
        /// </summary>
        public static LocalisableString ConfigurableHotkeys => new TranslatableString(getKey(@"configurable_hotkeys"), @"Most of the hotkeys in the game are configurable and can be changed to anything you want. Check the bindings panel under input settings!");

        /// <summary>
        /// "Your gameplay HUD can be customised by using the skin layout editor. Open it at any time via Ctrl-Shift-S!"
        /// </summary>
        public static LocalisableString SkinEditor => new TranslatableString(getKey(@"skin_editor"), @"Your gameplay HUD can be customised by using the skin layout editor. Open it at any time via Ctrl-Shift-S!");

        /// <summary>
        /// "You can create mod presets to make toggling your favourite mod combinations easier!"
        /// </summary>
        public static LocalisableString ModPresets => new TranslatableString(getKey(@"mod_presets"), @"You can create mod presets to make toggling your favourite mod combinations easier!");

        /// <summary>
        /// "Many mods have customisation settings that drastically change how they function. Click the Customise button in mod select to view settings!"
        /// </summary>
        public static LocalisableString ModCustomisationSettings => new TranslatableString(getKey(@"mod_customisation_settings"), @"Many mods have customisation settings that drastically change how they function. Click the Customise button in mod select to view settings!");

        /// <summary>
        /// "Press Ctrl-Shift-R to switch to a random skin!"
        /// </summary>
        public static LocalisableString RandomSkinShortcut => new TranslatableString(getKey(@"random_skin_shortcut"), @"Press Ctrl-Shift-R to switch to a random skin!");

        /// <summary>
        /// "While watching a replay, press Ctrl-H to toggle replay settings!"
        /// </summary>
        public static LocalisableString ToggleReplaySettingsShortcut => new TranslatableString(getKey(@"toggle_replay_settings_shortcut"), @"While watching a replay, press Ctrl-H to toggle replay settings!");

        /// <summary>
        /// "You can easily copy the mods from scores on a leaderboard by right-clicking on them!"
        /// </summary>
        public static LocalisableString CopyModsFromScore => new TranslatableString(getKey(@"copy_mods_from_score"), @"You can easily copy the mods from scores on a leaderboard by right-clicking on them!");

        /// <summary>
        /// "Ctrl-Enter at song select will start a beatmap in autoplay mode!"
        /// </summary>
        public static LocalisableString AutoplayBeatmapShortcut => new TranslatableString(getKey(@"autoplay_beatmap_shortcut"), @"Ctrl-Enter at song select will start a beatmap in autoplay mode!");

        /// <summary>
        /// "Multithreading support means that even with low &quot;FPS&quot; your input and judgements will be accurate!"
        /// </summary>
        public static LocalisableString MultithreadingSupport => new TranslatableString(getKey(@"multithreading_support"), @"Multithreading support means that even with low ""FPS"" your input and judgements will be accurate!");

        /// <summary>
        /// "All delete operations are temporary until exiting. Restore accidentally deleted content from the maintenance settings!"
        /// </summary>
        public static LocalisableString TemporaryDeleteOperations => new TranslatableString(getKey(@"temporary_delete_operations"), @"All delete operations are temporary until exiting. Restore accidentally deleted content from the maintenance settings!");

        /// <summary>
        /// "Take a look under the hood at performance counters and enable verbose performance logging with Ctrl-F2!"
        /// </summary>
        public static LocalisableString GlobalStatisticsShortcut => new TranslatableString(getKey(@"global_statistics_shortcut"), @"Take a look under the hood at performance counters and enable verbose performance logging with Ctrl-F2!");

        /// <summary>
        /// "When your gameplay HUD is hidden, you can press and hold Ctrl to view it temporarily!"
        /// </summary>
        public static LocalisableString PeekHUDWhenHidden => new TranslatableString(getKey(@"peek_hud_when_hidden"), @"When your gameplay HUD is hidden, you can press and hold Ctrl to view it temporarily!");

        /// <summary>
        /// "Drag and drop any image into the skin editor to load it in quickly!"
        /// </summary>
        public static LocalisableString DragAndDropImageInSkinEditor => new TranslatableString(getKey(@"drag_and_drop_image_in_skin_editor"), @"Drag and drop any image into the skin editor to load it in quickly!");

        /// <summary>
        /// "a tip for you:"
        /// </summary>
        public static LocalisableString MenuTipTitle => new TranslatableString(getKey(@"menu_tip_title"), @"a tip for you:");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
