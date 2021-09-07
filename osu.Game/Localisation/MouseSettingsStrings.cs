// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class MouseSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.MouseSettings";

        /// <summary>
        /// "Mouse"
        /// </summary>
        public static LocalisableString Mouse => new TranslatableString(getKey(@"mouse"), @"Mouse");

        /// <summary>
        /// "Not applicable in full screen mode"
        /// </summary>
        public static LocalisableString NotApplicableFullscreen => new TranslatableString(getKey(@"not_applicable_full_screen"), @"Not applicable in full screen mode");

        /// <summary>
        /// "High precision mouse"
        /// </summary>
        public static LocalisableString HighPrecisionMouse => new TranslatableString(getKey(@"high_precision_mouse"), @"高精度鼠标");

        /// <summary>
        /// "Attempts to bypass any operation system mouse acceleration. On windows, this is equivalent to what used to be known as &quot;Raw Input&quot;."
        /// </summary>
        public static LocalisableString HighPrecisionMouseTooltip => new TranslatableString(getKey(@"high_precision_mouse_tooltip"), @"Attempts to bypass any operation system mouse acceleration. On windows, this is equivalent to what used to be known as ""Raw Input"".");

        /// <summary>
        /// "Confine mouse cursor to window"
        /// </summary>
        public static LocalisableString ConfineMouseMode => new TranslatableString(getKey(@"confine_mouse_mode"), @"Confine mouse cursor to window");

        /// <summary>
        /// "Disable mouse wheel during gameplay"
        /// </summary>
        public static LocalisableString DisableMouseWheel => new TranslatableString(getKey(@"disable_mouse_wheel"), @"在游戏中禁用鼠标滚轮");

        /// <summary>
        /// "Disable mouse buttons during gameplay"
        /// </summary>
        public static LocalisableString DisableMouseButtons => new TranslatableString(getKey(@"disable_mouse_buttons"), @"在游戏中禁用鼠标按钮");

        /// <summary>
        /// "Enable high precision mouse to adjust sensitivity"
        /// </summary>
        public static LocalisableString EnableHighPrecisionForSensitivityAdjust => new TranslatableString(getKey(@"enable_high_precision_for_sensitivity_adjust"), @"Enable high precision mouse to adjust sensitivity");

        /// <summary>
        /// "Cursor sensitivity"
        /// </summary>
        public static LocalisableString CursorSensitivity => new TranslatableString(getKey(@"cursor_sensitivity"), @"Cursor sensitivity");

        /// <summary>
        /// "This setting has known issues on your platform. If you encounter problems, it is recommended to adjust sensitivity externally and keep this disabled for now."
        /// </summary>
        public static LocalisableString HighPrecisionPlatformWarning => new TranslatableString(getKey(@"high_precision_platform_warning"), @"此设置在您的平台上有已知问题。如果您遇到问题，建议您在游戏外调整灵敏度，并暂时禁用此项。");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
