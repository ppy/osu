// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class MouseSettings : SettingsSubsection
    {
        protected override string Header => "Mouse";

        private readonly BindableBool rawInputToggle = new BindableBool();
        private Bindable<string> activeInputHandlers;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig, FrameworkConfigManager config)
        {
            activeInputHandlers = config.GetBindable<string>(FrameworkSetting.ActiveInputHandlers);
            rawInputToggle.Value = activeInputHandlers.Value.Contains("Raw");
            rawInputToggle.ValueChanged += enabled =>
            {
                const string raw_mouse_handler = @"OpenTKRawMouseHandler";
                const string standard_mouse_handler = @"OpenTKMouseHandler";

                activeInputHandlers.Value = enabled ?
                    activeInputHandlers.Value.Replace(standard_mouse_handler, raw_mouse_handler) :
                    activeInputHandlers.Value.Replace(raw_mouse_handler, standard_mouse_handler);
            };

            Children = new Drawable[]
            {
                new SettingsEnumDropdown<ConfineMouseMode>
                {
                    LabelText = "Confine mouse cursor",
                    Bindable = config.GetBindable<ConfineMouseMode>(FrameworkSetting.ConfineMouseMode),
                },
                new SettingsCheckbox
                {
                    LabelText = "Disable mouse wheel during gameplay",
                    Bindable = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableWheel)
                },
                new SettingsCheckbox
                {
                    LabelText = "Disable mouse buttons during gameplay",
                    Bindable = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableButtons)
                },
                new SettingsCheckbox
                {
                    LabelText = "Raw Input",
                    Bindable = rawInputToggle
                },
                new SettingsSlider<double, SensitivitySlider>
                {
                    LabelText = "Cursor Sensitivity",
                    Bindable = config.GetBindable<double>(FrameworkSetting.CursorSensitivity)
                }
            };
        }

        private class SensitivitySlider : OsuSliderBar<double>
        {
            public override string TooltipText => Current.Value.ToString(@"0.##x");
        }
    }
}