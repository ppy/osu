// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.Graphics
{
    public class LayoutOptions : OptionsSubsection
    {
        protected override string Header => "Layout";

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config)
        {
            Children = new Drawable[]
            {
                new OptionLabel { Text = "Resolution: TODO dropdown" },
                new OptionEnumDropDown<WindowMode>
                {
                    LabelText = "Screen mode",
                    Bindable = config.GetBindable<WindowMode>(FrameworkConfig.WindowMode),
                },
                new OsuCheckbox
                {
                    LabelText = "Letterboxing",
                    Bindable = config.GetBindable<bool>(FrameworkConfig.Letterboxing),
                },
                new OptionSlider<int>
                {
                    LabelText = "Horizontal position",
                    Bindable = (BindableInt)config.GetBindable<int>(FrameworkConfig.LetterboxPositionX)
                },
                new OptionSlider<int>
                {
                    LabelText = "Vertical position",
                    Bindable = (BindableInt)config.GetBindable<int>(FrameworkConfig.LetterboxPositionY)
                },
            };
        }
    }
}