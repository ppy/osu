// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Localisation
{
    public static class BindingSettingsStrings
    {
        private const string prefix = @"osu.Game.Resources.Localisation.BindingSettings";

        /// <summary>
        /// "快捷键和键位绑定"
        /// </summary>
        public static LocalisableString ShortcutAndGameplayBindings => new TranslatableString(getKey(@"shortcut_and_gameplay_bindings"), @"快捷键和键位绑定");

        /// <summary>
        /// "配置"
        /// </summary>
        public static LocalisableString Configure => new TranslatableString(getKey(@"configure"), @"配置");

        /// <summary>
        /// "更改全局快捷键和键位绑定"
        /// </summary>
        public static LocalisableString ChangeBindingsButton => new TranslatableString(getKey(@"change_bindings_button"), @"更改全局快捷键和键位绑定");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}
