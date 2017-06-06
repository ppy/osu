﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
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
        private SensitivitySetting sensitivity;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig, FrameworkConfigManager config)
        {
            activeInputHandlers = config.GetBindable<string>(FrameworkSetting.ActiveInputHandlers);
            rawInputToggle.Value = activeInputHandlers.Value.Contains("Raw");

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
            };

            rawInputToggle.ValueChanged += enabled =>
            {
                // this is temporary until we support per-handler settings.
                const string raw_mouse_handler = @"OpenTKRawMouseHandler";
                const string standard_mouse_handler = @"OpenTKMouseHandler";

                activeInputHandlers.Value = enabled ?
                    activeInputHandlers.Value.Replace(standard_mouse_handler, raw_mouse_handler) :
                    activeInputHandlers.Value.Replace(raw_mouse_handler, standard_mouse_handler);

                sensitivity.Bindable.Disabled = !enabled;
            };

            rawInputToggle.TriggerChange();
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
                        MinValue = doubleValue.MinValue,
                        MaxValue = doubleValue.MaxValue
                    };

                    // one-way binding to update the sliderbar with changes from external actions.
                    doubleValue.DisabledChanged += disabled => base.Bindable.Disabled = disabled;
                    doubleValue.ValueChanged += newValue => base.Bindable.Value = newValue;
                }
            }
        }

        private class SensitivitySlider : OsuSliderBar<double>
        {
            public Bindable<double> Sensitivity;

            public SensitivitySlider()
            {
                KeyboardStep = 0.01f;

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