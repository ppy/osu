// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Debug
{
    public class GeneralSettings : SettingsSubsection
    {
        protected override string Header => "整体";

        [BackgroundDependencyLoader(true)]
        private void load(FrameworkDebugConfigManager config, FrameworkConfigManager frameworkConfig, OsuGame game)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "在左下角显示调试日志",
                    Current = frameworkConfig.GetBindable<bool>(FrameworkSetting.ShowLogOverlay)
                },
                new SettingsCheckbox
                {
                    LabelText = "绕过front-to-back渲染检查",
                    Current = config.GetBindable<bool>(DebugSetting.BypassFrontToBackPass)
                }
            };
        }
    }
}
