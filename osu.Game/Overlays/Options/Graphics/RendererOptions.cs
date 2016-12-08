//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Graphics
{
    public class RendererOptions : OptionsSubsection
    {
        protected override string Header => "Renderer";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            // NOTE: Compatability mode omitted
            Children = new Drawable[]
            {
                // TODO: this needs to be a custom dropdown at some point
                new DropdownOption<FrameSync>
                {
                    LabelText = "Frame limiter",
                    Bindable = config.GetBindable<FrameSync>(OsuConfig.FrameSync)
                },
                new CheckBoxOption
                {
                    LabelText = "Show FPS counter",
                    Bindable = config.GetBindable<bool>(OsuConfig.FpsCounter),
                },
                new CheckBoxOption
                {
                    LabelText = "Reduce dropped frames",
                    Bindable = config.GetBindable<bool>(OsuConfig.ForceFrameFlush),
                },
                new CheckBoxOption
                {
                    LabelText = "Detect performance issues",
                    Bindable = config.GetBindable<bool>(OsuConfig.DetectPerformanceIssues),
                },
            };
        }
    }
}