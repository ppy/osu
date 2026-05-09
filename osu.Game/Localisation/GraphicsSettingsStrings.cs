// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class GraphicsSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.GraphicsSettings";

        /// <summary>
        /// "Graphics"
        /// </summary>
        public static LocalisableString GraphicsSectionHeader => new TranslatableString(getKey(@"graphics_section_header"), @"Graphics");

        /// <summary>
        /// "Renderer"
        /// </summary>
        public static LocalisableString RendererHeader => new TranslatableString(getKey(@"renderer_header"), @"Renderer");

        /// <summary>
        /// "Renderer"
        /// </summary>
        public static LocalisableString Renderer => new TranslatableString(getKey(@"renderer"), @"Renderer");

        /// <summary>
        /// "Automatic"
        /// </summary>
        public static LocalisableString RendererAutomatic => new TranslatableString(getKey(@"renderer_automatic"), @"Automatic");

        /// <summary>
        /// "Automatic ({0})"
        /// </summary>
        public static LocalisableString RendererAutomaticInUse(LocalisableString rendererName) => new TranslatableString(getKey(@"renderer_automatic_in_use"), @"Automatic ({0})", rendererName);

        /// <summary>
        /// "{0} (Experimental)"
        /// </summary>
        public static LocalisableString RendererExperimental(LocalisableString rendererName) => new TranslatableString(getKey(@"renderer_experimental"), @"{0} (Experimental)", rendererName);

        /// <summary>
        /// "Frame limiter"
        /// </summary>
        public static LocalisableString FrameLimiter => new TranslatableString(getKey(@"frame_limiter"), @"Frame limiter");

        /// <summary>
        /// "VSync"
        /// </summary>
        public static LocalisableString FrameLimiterVSync => new TranslatableString(getKey(@"frame_limiter_vsync"), @"VSync");

        /// <summary>
        /// "{0}x refresh rate"
        /// </summary>
        public static LocalisableString FrameLimiterMultiplier(int multiplier) => new TranslatableString(getKey(@"frame_limiter_multiplier"), @"{0}x refresh rate", multiplier);

        /// <summary>
        /// "Basically unlimited"
        /// </summary>
        public static LocalisableString FrameLimiterUnlimited => new TranslatableString(getKey(@"frame_limiter_unlimited"), @"Basically unlimited");

        /// <summary>
        /// "Threading mode"
        /// </summary>
        public static LocalisableString ThreadingMode => new TranslatableString(getKey(@"threading_mode"), @"Threading mode");

        /// <summary>
        /// "Show FPS"
        /// </summary>
        public static LocalisableString ShowFPS => new TranslatableString(getKey(@"show_fps"), @"Show FPS");

        /// <summary>
        /// "Layout"
        /// </summary>
        public static LocalisableString LayoutHeader => new TranslatableString(getKey(@"layout_header"), @"Layout");

        /// <summary>
        /// "Screen mode"
        /// </summary>
        public static LocalisableString ScreenMode => new TranslatableString(getKey(@"screen_mode"), @"Screen mode");

        /// <summary>
        /// "Windowed"
        /// </summary>
        public static LocalisableString ScreenModeWindowed => new TranslatableString(getKey(@"screen_mode_windowed"), @"Windowed");

        /// <summary>
        /// "Borderless"
        /// </summary>
        public static LocalisableString ScreenModeBorderless => new TranslatableString(getKey(@"screen_mode_borderless"), @"Borderless");

        /// <summary>
        /// "Fullscreen"
        /// </summary>
        public static LocalisableString ScreenModeFullscreen => new TranslatableString(getKey(@"screen_mode_fullscreen"), @"Fullscreen");

        /// <summary>
        /// "Resolution"
        /// </summary>
        public static LocalisableString Resolution => new TranslatableString(getKey(@"resolution"), @"Resolution");

        /// <summary>
        /// "Display"
        /// </summary>
        public static LocalisableString Display => new TranslatableString(getKey(@"display"), @"Display");

        /// <summary>
        /// "UI scaling"
        /// </summary>
        public static LocalisableString UIScaling => new TranslatableString(getKey(@"ui_scaling"), @"UI scaling");

        /// <summary>
        /// "Screen scaling"
        /// </summary>
        public static LocalisableString ScreenScaling => new TranslatableString(getKey(@"screen_scaling"), @"Screen scaling");

        /// <summary>
        /// "Horizontal position"
        /// </summary>
        public static LocalisableString HorizontalPosition => new TranslatableString(getKey(@"horizontal_position"), @"Horizontal position");

        /// <summary>
        /// "Vertical position"
        /// </summary>
        public static LocalisableString VerticalPosition => new TranslatableString(getKey(@"vertical_position"), @"Vertical position");

        /// <summary>
        /// "Horizontal scale"
        /// </summary>
        public static LocalisableString HorizontalScale => new TranslatableString(getKey(@"horizontal_scale"), @"Horizontal scale");

        /// <summary>
        /// "Vertical scale"
        /// </summary>
        public static LocalisableString VerticalScale => new TranslatableString(getKey(@"vertical_scale"), @"Vertical scale");

        /// <summary>
        /// "Running without fullscreen mode will increase your input latency!"
        /// </summary>
        public static LocalisableString NotFullscreenNote => new TranslatableString(getKey(@"not_fullscreen_note"), @"Running without fullscreen mode will increase your input latency!");

        /// <summary>
        /// "Detail Settings"
        /// </summary>
        public static LocalisableString DetailSettingsHeader => new TranslatableString(getKey(@"detail_settings_header"), @"Detail Settings");

        /// <summary>
        /// "Storyboard / video"
        /// </summary>
        public static LocalisableString StoryboardVideo => new TranslatableString(getKey(@"storyboard_video"), @"Storyboard / video");

        /// <summary>
        /// "Combo colour normalisation"
        /// </summary>
        public static LocalisableString ComboColourNormalisation => new TranslatableString(getKey(@"combo_colour_normalisation"), @"Combo colour normalisation");

        /// <summary>
        /// "Hit lighting"
        /// </summary>
        public static LocalisableString HitLighting => new TranslatableString(getKey(@"hit_lighting"), @"Hit lighting");

        /// <summary>
        /// "Screenshots"
        /// </summary>
        public static LocalisableString Screenshots => new TranslatableString(getKey(@"screenshots"), @"Screenshots");

        /// <summary>
        /// "Screenshot format"
        /// </summary>
        public static LocalisableString ScreenshotFormat => new TranslatableString(getKey(@"screenshot_format"), @"Screenshot format");

        /// <summary>
        /// "Show menu cursor in screenshots"
        /// </summary>
        public static LocalisableString ShowCursorInScreenshots => new TranslatableString(getKey(@"show_cursor_in_screenshots"), @"Show menu cursor in screenshots");

        /// <summary>
        /// "Video"
        /// </summary>
        public static LocalisableString VideoHeader => new TranslatableString(getKey(@"video_header"), @"Video");

        /// <summary>
        /// "Use hardware acceleration"
        /// </summary>
        public static LocalisableString UseHardwareAcceleration => new TranslatableString(getKey(@"use_hardware_acceleration"), @"Use hardware acceleration");

        /// <summary>
        /// "JPG (web-friendly)"
        /// </summary>
        public static LocalisableString Jpg => new TranslatableString(getKey(@"jpg_web_friendly"), @"JPG (web-friendly)");

        /// <summary>
        /// "PNG (lossless)"
        /// </summary>
        public static LocalisableString Png => new TranslatableString(getKey(@"png_lossless"), @"PNG (lossless)");

        /// <summary>
        /// "In order to change the renderer, the game will close. Please open it again."
        /// </summary>
        public static LocalisableString ChangeRendererConfirmation => new TranslatableString(getKey(@"change_renderer_configuration"), @"In order to change the renderer, the game will close. Please open it again.");

        /// <summary>
        /// "Minimise osu! when switching to another app"
        /// </summary>
        public static LocalisableString MinimiseOnFocusLoss => new TranslatableString(getKey(@"minimise_on_focus_loss"), @"Minimise osu! when switching to another app");

        /// <summary>
        /// "Shrink game to avoid cameras and notches"
        /// </summary>
        public static LocalisableString ShrinkGameToSafeArea => new TranslatableString(getKey(@"shrink_game_to_safe_area"), @"Shrink game to avoid cameras and notches");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
