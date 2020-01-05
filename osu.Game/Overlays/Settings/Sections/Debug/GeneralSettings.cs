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

        [BackgroundDependencyLoader]
        private void load(FrameworkDebugConfigManager config, FrameworkConfigManager frameworkConfig)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "显示log overlay",
                    Bindable = frameworkConfig.GetBindable<bool>(FrameworkSetting.ShowLogOverlay)
                },
                new SettingsCheckbox
                {
                    LabelText = "记录性能",
                    Bindable = config.GetBindable<bool>(DebugSetting.PerformanceLogging)
                },
                new SettingsCheckbox
                {
                    LabelText = "绕过 front-to-back 渲染检查",
                    Bindable = config.GetBindable<bool>(DebugSetting.BypassFrontToBackPass)
                }
            };
        }
    }
}
