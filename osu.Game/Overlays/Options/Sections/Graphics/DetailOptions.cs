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
                    Bindable = config.GetBindable<bool>(OsuConfig.SnakingInSliders),
                    TooltipText = "Sliders gradually snake in from their starting point. This should run fine unless you have a low-end PC."
                },
                new OsuCheckbox
                {
                    LabelText = "Snaking out sliders",
                    Bindable = config.GetBindable<bool>(OsuConfig.SnakingOutSliders),
                    TooltipText = "Sliders gradually snake out from their starting point. This should run fine unless you have a low-end PC."
                },
                new OsuCheckbox
                {
                    LabelText = "Background video",
                    Bindable = config.GetBindable<bool>(OsuConfig.Video),
                    TooltipText = "Enables background video playback. If you get a large amount of lag on beatmaps with video, try disabling this feature."
                },
                new OsuCheckbox
                {
                    LabelText = "Storyboards",
                    Bindable = config.GetBindable<bool>(OsuConfig.ShowStoryboard),
                    TooltipText = "Show storyboards in the background of beatmaps. These usually contain story elements, lyrics or special effects."
                },
                new OsuCheckbox
                {
                    LabelText = "Combo bursts",
                    Bindable = config.GetBindable<bool>(OsuConfig.ComboBurst),
                    TooltipText = "A character image burst from the side of the screen at combo milestones."
                },
                new OsuCheckbox
                {
                    LabelText = "Hit lighting",
                    Bindable = config.GetBindable<bool>(OsuConfig.HitLighting),
                    TooltipText = "Adds a subtle glow behind hit explosions which lights the playfield."
                },
                new OsuCheckbox
                {
                    LabelText = "Shaders",
                    Bindable = config.GetBindable<bool>(OsuConfig.Bloom),
                    TooltipText = "Enables shader special effects in gameplay (epic flashes, blurring, tinting and more!). Highly recommended, but requires Pixel Shader 2.0 support and a relatively powerful graphics card. If your card is unsupported, this will be automatically disabled."
                },
                new OsuCheckbox
                {
                    LabelText = "Softening filter",
                    Bindable = config.GetBindable<bool>(OsuConfig.BloomSoftening),
                    TooltipText = "Adds a softening touch to visual game-wide. Some people like this, others hate it ;)"
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