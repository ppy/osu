// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

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
                    LabelText = "Bypass caching",
                    Bindable = config.GetBindable<bool>(DebugSetting.BypassCaching)
                },
                new SettingsCheckbox
                {
                    LabelText = "Debug logs",
                    Bindable = frameworkConfig.GetBindable<bool>(FrameworkSetting.ShowLogOverlay)
                }
            };
        }
    }
}
