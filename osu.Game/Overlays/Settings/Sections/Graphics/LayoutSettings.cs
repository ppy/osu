// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public class LayoutSettings : SettingsSubsection
    {
        protected override string Header => "Layout";

        private FillFlowContainer letterboxSettings;

        private Bindable<bool> letterboxing;

        private const int letterbox_container_height = 75;
        private const int duration = 300;

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
                letterboxSettings = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    Masking = true,

                    Children = new Drawable[]
                    {
                        new SettingsSlider<double>
                        {
                            LabelText = "Horizontal position",
                            Bindable = config.GetBindable<double>(FrameworkSetting.LetterboxPositionX)
                        },
                        new SettingsSlider<double>
                        {
                            LabelText = "Vertical position",
                            Bindable = config.GetBindable<double>(FrameworkSetting.LetterboxPositionY)
                        },
                    }
                },
            };

            letterboxing.ValueChanged += isVisible => letterboxSettings.ResizeHeightTo(isVisible ? letterbox_container_height : 0, duration, EasingTypes.OutQuint);
            letterboxing.TriggerChange();
        }
    }
}
