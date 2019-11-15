// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Debug
{
    public class GeneralSettings : SettingsSubsection
    {
        protected override string Header => "General";

        [BackgroundDependencyLoader]
        private void load(FrameworkDebugConfigManager config, FrameworkConfigManager frameworkConfig)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Show log overlay",
                    Bindable = frameworkConfig.GetBindable<bool>(FrameworkSetting.ShowLogOverlay)
                },
                new SettingsCheckbox
                {
                    LabelText = "Performance logging",
                    Bindable = frameworkConfig.GetBindable<bool>(FrameworkSetting.PerformanceLogging)
                },
                new SettingsCheckbox
                {
                    LabelText = "Bypass front-to-back render pass",
                    Bindable = config.GetBindable<bool>(DebugSetting.BypassFrontToBackPass)
                }
            };
        }
    }
}
