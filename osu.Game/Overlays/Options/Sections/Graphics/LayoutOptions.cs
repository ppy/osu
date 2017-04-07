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

        private OptionSlider<int> letterboxPositionX;
        private OptionSlider<int> letterboxPositionY;

        private Bindable<bool> letterboxing;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config)
        {
            letterboxing = config.GetBindable<bool>(FrameworkConfig.Letterboxing);

            Children = new Drawable[]
            {
                new OptionLabel { Text = "Resolution: TODO dropdown" },
                new OptionEnumDropdown<WindowMode>
                {
                    LabelText = "Screen mode",
                    Bindable = config.GetBindable<WindowMode>(FrameworkConfig.WindowMode),
                },
                new OsuCheckbox
                {
                    LabelText = "Letterboxing",
                    Bindable = letterboxing,
                },
                letterboxPositionX = new OptionSlider<int>
                {
                    LabelText = "Horizontal position",
                    Bindable = (BindableInt)config.GetBindable<int>(FrameworkConfig.LetterboxPositionX)
                },
                letterboxPositionY = new OptionSlider<int>
                {
                    LabelText = "Vertical position",
                    Bindable = (BindableInt)config.GetBindable<int>(FrameworkConfig.LetterboxPositionY)
                },
            };

            letterboxing.ValueChanged += visibilityChanged;
            letterboxing.TriggerChange();
        }

        private void visibilityChanged(bool newVisibility)
        {
            if (newVisibility)
            {
                letterboxPositionX.Show();
                letterboxPositionY.Show();
            }
            else
            {
                letterboxPositionX.Hide();
                letterboxPositionY.Hide();
            }
        }
    }
}