// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class MouseSettings : SettingsSubsection
    {
        protected override string Header => "Mouse";

        private readonly BindableBool rawInputToggle = new BindableBool();

        private Bindable<double> configSensitivity;

        private Bindable<double> localSensitivity;

        private Bindable<string> ignoredInputHandlers;

        private Bindable<WindowMode> windowMode;
        private SettingsEnumDropdown<OsuConfineMouseMode> confineMouseModeSetting;

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig, FrameworkConfigManager config)
        {
            // use local bindable to avoid changing enabled state of game host's bindable.
            configSensitivity = config.GetBindable<double>(FrameworkSetting.CursorSensitivity);
            localSensitivity = configSensitivity.GetUnboundCopy();

            windowMode = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode);
            ignoredInputHandlers = config.GetBindable<string>(FrameworkSetting.IgnoredInputHandlers);

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Raw input",
                    Current = rawInputToggle
                },
                new SensitivitySetting
                {
                    LabelText = "Cursor sensitivity",
                    Current = localSensitivity
                },
                new SettingsCheckbox
                {
                    LabelText = "Map absolute input to window",
                    Current = config.GetBindable<bool>(FrameworkSetting.MapAbsoluteInputToWindow)
                },
                confineMouseModeSetting = new SettingsEnumDropdown<OsuConfineMouseMode>
                {
                    LabelText = "Confine mouse cursor to window",
                    Current = osuConfig.GetBindable<OsuConfineMouseMode>(OsuSetting.ConfineMouseMode)
                },
                new SettingsCheckbox
                {
                    LabelText = "Disable mouse wheel during gameplay",
                    Current = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableWheel)
                },
                new SettingsCheckbox
                {
                    LabelText = "Disable mouse buttons during gameplay",
                    Current = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableButtons)
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            configSensitivity.BindValueChanged(val =>
            {
                var disabled = localSensitivity.Disabled;

                localSensitivity.Disabled = false;
                localSensitivity.Value = val.NewValue;
                localSensitivity.Disabled = disabled;
            }, true);

            localSensitivity.BindValueChanged(val => configSensitivity.Value = val.NewValue);

            windowMode.BindValueChanged(mode =>
            {
                var isFullscreen = mode.NewValue == WindowMode.Fullscreen;

                if (isFullscreen)
                {
                    confineMouseModeSetting.Current.Disabled = true;
                    confineMouseModeSetting.TooltipText = "Not applicable in full screen mode";
                }
                else
                {
                    confineMouseModeSetting.Current.Disabled = false;
                    confineMouseModeSetting.TooltipText = string.Empty;
                }
            }, true);

            if (RuntimeInfo.OS != RuntimeInfo.Platform.Windows)
            {
                rawInputToggle.Disabled = true;
                localSensitivity.Disabled = true;
            }
            else
            {
                rawInputToggle.ValueChanged += enabled =>
                {
                    // this is temporary until we support per-handler settings.
                    const string raw_mouse_handler = @"OsuTKRawMouseHandler";
                    const string standard_mouse_handlers = @"OsuTKMouseHandler MouseHandler";

                    ignoredInputHandlers.Value = enabled.NewValue ? standard_mouse_handlers : raw_mouse_handler;
                };

                ignoredInputHandlers.ValueChanged += handler =>
                {
                    bool raw = !handler.NewValue.Contains("Raw");
                    rawInputToggle.Value = raw;
                    localSensitivity.Disabled = !raw;
                };

                ignoredInputHandlers.TriggerChange();
            }
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
            public override string TooltipText => Current.Disabled ? "enable raw input to adjust sensitivity" : $"{base.TooltipText}x";
        }
    }
}
