// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Sections.Graphics
{
    public class RendererOptions : OptionsSubsection
    {
        protected override string Header => "Renderer";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig, FrameworkConfigManager config)
        {
            // NOTE: Compatability mode omitted
            Children = new Drawable[]
            {
                // TODO: this needs to be a custom dropdown at some point
                new OptionEnumDropDown<FrameSync>
                {
                    LabelText = "Frame limiter",
                    Bindable = config.GetBindable<FrameSync>(FrameworkConfig.FrameSync)
                },
                new OptionCheckbox
                {
                    LabelText = "Show FPS counter",
                    Bindable = osuConfig.GetBindable<bool>(OsuConfig.FpsCounter),
                },
                new OptionCheckbox
                {
                    LabelText = "Reduce dropped frames",
                    Bindable = osuConfig.GetBindable<bool>(OsuConfig.ForceFrameFlush),
                },
                new OptionCheckbox
                {
                    LabelText = "Detect performance issues",
                    Bindable = osuConfig.GetBindable<bool>(OsuConfig.DetectPerformanceIssues),
                },
            };
        }
    }
}