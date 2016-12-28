//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Graphics
{
    public class LayoutOptions : OptionsSubsection
    {
        protected override string Header => "Layout";

        private CheckBoxOption letterboxing;

        private SliderOption<int> letterboxPositionX;
        private SliderOption<int> letterboxPositionY;

        private void eventLetterboxing()
        {
            if (letterboxing.State == CheckBoxState.Unchecked)
            {
                letterboxPositionX.FadeOut(150);
                letterboxPositionY.FadeOut(150);
            }
            else
            {
                letterboxPositionX.FadeIn(150);
                letterboxPositionY.FadeIn(150);
            }
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "Resolution: TODO dropdown" },
                new CheckBoxOption
                {
                    LabelText = "Fullscreen mode",
                    Bindable = config.GetBindable<bool>(OsuConfig.Fullscreen),
                },
                letterboxing = new CheckBoxOption
                {
                    LabelText = "Letterboxing",
                    Bindable = config.GetBindable<bool>(OsuConfig.Letterboxing),
                },
                letterboxPositionX = new SliderOption<int>
                {
                    LabelText = "Horizontal position",
                    Bindable = (BindableInt)config.GetBindable<int>(OsuConfig.LetterboxPositionX)
                },
                letterboxPositionY = new SliderOption<int>
                {
                    LabelText = "Vertical position",
                    Bindable = (BindableInt)config.GetBindable<int>(OsuConfig.LetterboxPositionY)
                },
            };

            if (!config.GetBindable<bool>(OsuConfig.Letterboxing))
            {
                letterboxPositionX.Hide();
                letterboxPositionY.Hide();
            }

            config.GetBindable<bool>(OsuConfig.Letterboxing).ValueChanged += delegate { eventLetterboxing(); };
        }
    }
}