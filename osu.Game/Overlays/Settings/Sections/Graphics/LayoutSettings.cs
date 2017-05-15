// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class LayoutSettings : SettingsSubsection
    {
        protected override string Header => "Layout";

        private SettingsSlider<double> letterboxPositionX;
        private SettingsSlider<double> letterboxPositionY;

        private Bindable<bool> letterboxing;

        [BackgroundDependencyLoader]
        private void load(FrameworkConfigManager config)
        {
            letterboxing = config.GetBindable<bool>(FrameworkSetting.Letterboxing);

            Children = new Drawable[]
            {
                new SettingsEnumDropdown<WindowMode>
                {
                    LabelText = "Screen mode",
                    Bindable = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode),
                },
                new SettingsCheckbox
                {
                    LabelText = "Letterboxing",
                    Bindable = letterboxing,
                },
                letterboxPositionX = new SettingsSlider<double>
                {
                    LabelText = "Horizontal position",
                    Bindable = config.GetBindable<double>(FrameworkSetting.LetterboxPositionX)
                },
                letterboxPositionY = new SettingsSlider<double>
                {
                    LabelText = "Vertical position",
                    Bindable = config.GetBindable<double>(FrameworkSetting.LetterboxPositionY)
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
