//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Graphics
{
    public class DetailOptions : OptionsSubsection
    {
        protected override string Header => "Detail Settings";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new CheckBoxOption
                {
                    LabelText = "Snaking in sliders",
                    Bindable = config.GetBindable<bool>(OsuConfig.SnakingInSliders)
                },
                new CheckBoxOption
                {
                    LabelText = "Snaking out sliders",
                    Bindable = config.GetBindable<bool>(OsuConfig.SnakingOutSliders)
                },
                new CheckBoxOption
                {
                    LabelText = "Background video",
                    Bindable = config.GetBindable<bool>(OsuConfig.Video)
                },
                new CheckBoxOption
                {
                    LabelText = "Storyboards",
                    Bindable = config.GetBindable<bool>(OsuConfig.ShowStoryboard)
                },
                new CheckBoxOption
                {
                    LabelText = "Combo bursts",
                    Bindable = config.GetBindable<bool>(OsuConfig.ComboBurst)
                },
                new CheckBoxOption
                {
                    LabelText = "Hit lighting",
                    Bindable = config.GetBindable<bool>(OsuConfig.HitLighting)
                },
                new CheckBoxOption
                {
                    LabelText = "Shaders",
                    Bindable = config.GetBindable<bool>(OsuConfig.Bloom)
                },
                new CheckBoxOption
                {
                    LabelText = "Softening filter",
                    Bindable = config.GetBindable<bool>(OsuConfig.BloomSoftening)
                },
                new DropdownOption<ScreenshotFormat>
                {
                    LabelText = "Screenshot",
                    Bindable = config.GetBindable<ScreenshotFormat>(OsuConfig.ScreenshotFormat)
                }
            };
        }
    }
}