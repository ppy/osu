// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
        private Bindable<string> ignoredInputHandler;
        private SensitivitySetting sensitivity;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig, FrameworkConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Raw input",
                    Bindable = rawInputToggle
                },
                sensitivity = new SensitivitySetting
                {
                    LabelText = "Cursor sensitivity",
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
            };

            rawInputToggle.ValueChanged += enabled =>
            {
                // this is temporary until we support per-handler settings.
                const string raw_mouse_handler = @"OsuTKRawMouseHandler";
                const string standard_mouse_handler = @"OsuTKMouseHandler";

                ignoredInputHandler.Value = enabled.NewValue ? standard_mouse_handler : raw_mouse_handler;
            };

            ignoredInputHandler = config.GetBindable<string>(FrameworkSetting.IgnoredInputHandlers);
            ignoredInputHandler.ValueChanged += handler =>
            {
                bool raw = !handler.NewValue.Contains("Raw");
                rawInputToggle.Value = raw;
                sensitivity.Bindable.Disabled = !raw;
            };

            ignoredInputHandler.TriggerChange();
        }

        private class SensitivitySetting : SettingsSlider<double, SensitivitySlider>
        {
            public SensitivitySetting()
            {
                KeyboardStep = 0.01f;
                TransferValueOnCommit = true;
            }
        }

        private class SensitivitySlider : OsuSliderBar<double>
        {
            public override string TooltipText => Current.Disabled ? "Enable raw input to adjust sensitivity" : $"{base.TooltipText}x";
        }
    }
}
