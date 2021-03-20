// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class BindingSettings : SettingsSubsection
    {
        protected override string Header => "快捷键和键位设定";

        public BindingSettings(KeyBindingPanel keyConfig)
        {
            Children = new Drawable[]
            {
                new SettingsButton
                {
                    Text = "配置",
                    TooltipText = "更改全局快捷键和键位设定",
                    Action = keyConfig.ToggleVisibility
                },
            };
        }
    }
}
