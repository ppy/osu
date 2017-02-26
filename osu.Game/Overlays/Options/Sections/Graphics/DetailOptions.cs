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
                    Bindable = config.GetWeldedBindable<bool>(OsuConfig.SnakingInSliders)
                },
                new OsuCheckbox
                {
                    LabelText = "Snaking out sliders",
                    Bindable = config.GetWeldedBindable<bool>(OsuConfig.SnakingOutSliders)
                },
                new OsuCheckbox
                {
                    LabelText = "Background video",
                    Bindable = config.GetWeldedBindable<bool>(OsuConfig.Video)
                },
                new OsuCheckbox
                {
                    LabelText = "Storyboards",
                    Bindable = config.GetWeldedBindable<bool>(OsuConfig.ShowStoryboard)
                },
                new OsuCheckbox
                {
                    LabelText = "Combo bursts",
                    Bindable = config.GetWeldedBindable<bool>(OsuConfig.ComboBurst)
                },
                new OsuCheckbox
                {
                    LabelText = "Hit lighting",
                    Bindable = config.GetWeldedBindable<bool>(OsuConfig.HitLighting)
                },
                new OsuCheckbox
                {
                    LabelText = "Shaders",
                    Bindable = config.GetWeldedBindable<bool>(OsuConfig.Bloom)
                },
                new OsuCheckbox
                {
                    LabelText = "Softening filter",
                    Bindable = config.GetWeldedBindable<bool>(OsuConfig.BloomSoftening)
                },
                new OptionEnumDropDown<ScreenshotFormat>
                {
                    LabelText = "Screenshot",
                    Bindable = config.GetWeldedBindable<ScreenshotFormat>(OsuConfig.ScreenshotFormat)
                }
            };
        }
    }
}