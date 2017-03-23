// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.Graphics
{
    public class DetailOptions : OptionsSubsection
    {
        protected override string Header => "Detail Settings";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new OsuCheckbox
                {
                    LabelText = "Snaking in sliders",
                    Bindable = config.GetBindable<bool>(OsuConfig.SnakingInSliders)
                },
                new OsuCheckbox
                {
                    LabelText = "Snaking out sliders",
                    Bindable = config.GetBindable<bool>(OsuConfig.SnakingOutSliders)
                },
                new OsuCheckbox
                {
                    LabelText = "Background video",
                    Bindable = config.GetBindable<bool>(OsuConfig.Video)
                },
                new OsuCheckbox
                {
                    LabelText = "Storyboards",
                    Bindable = config.GetBindable<bool>(OsuConfig.ShowStoryboard)
                },
                new OsuCheckbox
                {
                    LabelText = "Combo bursts",
                    Bindable = config.GetBindable<bool>(OsuConfig.ComboBurst)
                },
                new OsuCheckbox
                {
                    LabelText = "Hit lighting",
                    Bindable = config.GetBindable<bool>(OsuConfig.HitLighting)
                },
                new OsuCheckbox
                {
                    LabelText = "Shaders",
                    Bindable = config.GetBindable<bool>(OsuConfig.Bloom)
                },
                new OsuCheckbox
                {
                    LabelText = "Softening filter",
                    Bindable = config.GetBindable<bool>(OsuConfig.BloomSoftening)
                },
                new OptionEnumDropdown<ScreenshotFormat>
                {
                    LabelText = "Screenshot",
                    Bindable = config.GetBindable<ScreenshotFormat>(OsuConfig.ScreenshotFormat)
                }
            };
        }
    }
}