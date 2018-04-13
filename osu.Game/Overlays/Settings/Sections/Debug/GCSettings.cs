// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Runtime;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Debug
{
    public class GCSettings : SettingsSubsection
    {
        protected override string Header => "Garbage Collector";

        [BackgroundDependencyLoader]
        private void load(FrameworkDebugConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsEnumDropdown<GCLatencyMode>
                {
                    LabelText = "Active mode",
                    Bindable = config.GetBindable<GCLatencyMode>(DebugSetting.ActiveGCMode)
                },
                new SettingsButton
                {
                    Text = "Force garbage collection",
                    Action = GC.Collect
                },
            };
        }
    }
}
