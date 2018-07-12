// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System.Collections.Generic;
using System.Linq;
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
        private readonly BindableBool disableJoystickInputToggle = new BindableBool();
        private Bindable<string> ignoredInputHandler;
        private SensitivitySetting sensitivity;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig, FrameworkConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Raw Input",
                    Bindable = rawInputToggle
                },
                sensitivity = new SensitivitySetting
                {
                    LabelText = "Cursor Sensitivity",
                    Bindable = config.GetBindable<double>(FrameworkSetting.CursorSensitivity)
                },
                new SettingsCheckbox
                {
                    LabelText = "Map absolute input to window",
                    Bindable = config.GetBindable<bool>(FrameworkSetting.MapAbsoluteInputToWindow)
                },
                new SettingsEnumDropdown<ConfineMouseMode>
                {
                    LabelText = "Confine mouse cursor to window",
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
                    LabelText = "Disable all joystick input",
                    Bindable = disableJoystickInputToggle
                },
            };

            ignoredInputHandler = config.GetBindable<string>(FrameworkSetting.IgnoredInputHandlers);

            setBindablesFromIgnoreInputHandler();
            setIgnoreInputHandler();

            rawInputToggle.BindValueChanged(_ => setIgnoreInputHandler());
            disableJoystickInputToggle.BindValueChanged(_ => setIgnoreInputHandler());

            ignoredInputHandler.BindValueChanged(_ => setBindablesFromIgnoreInputHandler());
        }

        // this is temporary until we support per-handler settings.
        private const string standard_mouse_handler = @"OpenTKMouseHandler";
        private const string raw_mouse_handler = @"OpenTKRawMouseHandler";
        private const string joystick_handler = @"OpenTKJoystickHandler";

        private void setIgnoreInputHandler()
        {
            var disableInputHandlerNames = new List<string>
            {
                rawInputToggle.Value ? standard_mouse_handler : raw_mouse_handler
            };

            if (disableJoystickInputToggle.Value)
                disableInputHandlerNames.Add(joystick_handler);

            ignoredInputHandler.Value = string.Join(" ", disableInputHandlerNames);
        }

        private void setBindablesFromIgnoreInputHandler()
        {
            var disableInputHandlerNames = ignoredInputHandler.Value.Split(' ');

            rawInputToggle.Value = disableInputHandlerNames.Contains(standard_mouse_handler);
            sensitivity.Bindable.Disabled = !rawInputToggle.Value;

            disableJoystickInputToggle.Value = disableInputHandlerNames.Contains(joystick_handler);
        }

        private class SensitivitySetting : SettingsSlider<double, SensitivitySlider>
        {
            public override Bindable<double> Bindable
            {
                get { return ((SensitivitySlider)Control).Sensitivity; }

                set
                {
                    BindableDouble doubleValue = (BindableDouble)value;

                    // create a second layer of bindable so we can only handle state changes when not being dragged.
                    ((SensitivitySlider)Control).Sensitivity = doubleValue;

                    // this bindable will still act as the "interactive" bindable displayed during a drag.
                    base.Bindable = new BindableDouble(doubleValue.Value)
                    {
                        Default = doubleValue.Default,
                        MinValue = doubleValue.MinValue,
                        MaxValue = doubleValue.MaxValue
                    };

                    // one-way binding to update the sliderbar with changes from external actions.
                    doubleValue.DisabledChanged += disabled => base.Bindable.Disabled = disabled;
                    doubleValue.ValueChanged += newValue => base.Bindable.Value = newValue;
                }
            }

            public SensitivitySetting()
            {
                KeyboardStep = 0.01f;
            }
        }

        private class SensitivitySlider : OsuSliderBar<double>
        {
            public Bindable<double> Sensitivity;

            public SensitivitySlider()
            {
                Current.ValueChanged += newValue =>
                {
                    if (!isDragging && Sensitivity != null)
                        Sensitivity.Value = newValue;
                };
            }

            private bool isDragging;

            protected override bool OnDragStart(InputState state)
            {
                isDragging = true;
                return base.OnDragStart(state);
            }

            protected override bool OnDragEnd(InputState state)
            {
                isDragging = false;
                Current.TriggerChange();

                return base.OnDragEnd(state);
            }

            public override string TooltipText => Current.Disabled ? "Enable raw input to adjust sensitivity" : Current.Value.ToString(@"0.##x");
        }
    }
}
