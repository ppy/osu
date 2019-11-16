// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class KeyboardSettings : SettingsSubsection
    {
        protected override string Header => "键盘";

        public KeyboardSettings(KeyBindingPanel keyConfig)
        {
            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = "键盘配置",
                    TooltipText = "更改全局快捷键和游戏键位",
                    Action = keyConfig.ToggleVisibility
                },
            };
        }
    }
}
