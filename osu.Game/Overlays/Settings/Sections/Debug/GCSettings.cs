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

        private readonly Bindable<LatencyMode> latencyMode = new Bindable<LatencyMode>();
        private Bindable<GCLatencyMode> configLatencyMode;

        [BackgroundDependencyLoader]
        private void load(FrameworkDebugConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsEnumDropdown<LatencyMode>
                {
                    LabelText = "Active mode",
                    Bindable = latencyMode
                },
                new SettingsButton
                {
                    Text = "Force garbage collection",
                    Action = GC.Collect
                },
            };

            configLatencyMode = config.GetBindable<GCLatencyMode>(DebugSetting.ActiveGCMode);
            configLatencyMode.BindValueChanged(v => latencyMode.Value = (LatencyMode)v, true);
            latencyMode.BindValueChanged(v => configLatencyMode.Value = (GCLatencyMode)v);
        }

        private enum LatencyMode
        {
            Batch = GCLatencyMode.Batch,
            Interactive = GCLatencyMode.Interactive,
            LowLatency = GCLatencyMode.LowLatency,
            SustainedLowLatency = GCLatencyMode.SustainedLowLatency
        }
    }
}
