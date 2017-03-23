// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Runtime;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.Debug
{
    public class GCOptions : OptionsSubsection
    {
        protected override string Header => "Garbage Collector";

        [BackgroundDependencyLoader]
        private void load(FrameworkDebugConfigManager config)
        {
            Children = new Drawable[]
            {
                new OptionEnumDropdown<GCLatencyMode>
                {
                    LabelText = "Active mode",
                    Bindable = config.GetBindable<GCLatencyMode>(FrameworkDebugConfig.ActiveGCMode)
                },
                new OsuButton
                {
                    RelativeSizeAxes = Axes.X,
                    Text = "Force garbage collection",
                    Action = () => GC.Collect()
                },
            };
        }
    }
}
