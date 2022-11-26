// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class MouseSettings : SettingsSubsection
    {
        private readonly MouseHandler mouseHandler;

        protected override LocalisableString Header => MouseSettingsStrings.Mouse;

        private Bindable<double> handlerSensitivity;

        private Bindable<double> localSensitivity;

        private Bindable<double> handlerSensitivityY;

        private Bindable<double> localSensitivityY;

        private Bindable<WindowMode> windowMode;
        private SettingsEnumDropdown<OsuConfineMouseMode> confineMouseModeSetting;
        private Bindable<bool> relativeMode;

        private Bindable<bool> separateMode;

        private SettingsCheckbox highPrecisionMouse;

        private SettingsCheckbox separateSensitivity;

        private SensitivitySetting horizontalSensitivity;

        public MouseSettings(MouseHandler mouseHandler)
        {
            this.mouseHandler = mouseHandler;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig, FrameworkConfigManager config)
        {
            // use local bindable to avoid changing enabled state of game host's bindable.
            handlerSensitivity = mouseHandler.Sensitivity.GetBoundCopy();
            localSensitivity = handlerSensitivity.GetUnboundCopy();
            handlerSensitivityY = mouseHandler.SensitivityY.GetBoundCopy();
            localSensitivityY = handlerSensitivityY.GetUnboundCopy();

            relativeMode = mouseHandler.UseRelativeMode.GetBoundCopy();
            separateMode = mouseHandler.UseSeparateSensitivity.GetBoundCopy();
            windowMode = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode);

            Children = new Drawable[]
            {
                highPrecisionMouse = new SettingsCheckbox
                {
                    LabelText = MouseSettingsStrings.HighPrecisionMouse,
                    TooltipText = MouseSettingsStrings.HighPrecisionMouseTooltip,
                    Current = relativeMode,
                    Keywords = new[] { @"raw", @"input", @"relative", @"cursor" }
                },
                separateSensitivity = new SettingsCheckbox
                {
                    LabelText = MouseSettingsStrings.SeparateSensitivity,
                    TooltipText = MouseSettingsStrings.SeparateSensitivityTooltip,
                    Current = separateMode,
                    Keywords = new[] { @"sensitivity", @"mouse", @"separate", @"cursor" }
                },
                horizontalSensitivity = new SensitivitySetting
                {
                    LabelText = separateMode.Value ? MouseSettingsStrings.CursorHorizontalSensitivity : MouseSettingsStrings.CursorSensitivity,
                    Current = localSensitivity
                },
                new SensitivitySettingY
                {
                    LabelText = MouseSettingsStrings.CursorVerticalSensitivity,
                    Current = localSensitivityY,
                },
                confineMouseModeSetting = new SettingsEnumDropdown<OsuConfineMouseMode>
                {
                    LabelText = MouseSettingsStrings.ConfineMouseMode,
                    Current = osuConfig.GetBindable<OsuConfineMouseMode>(OsuSetting.ConfineMouseMode)
                },
                new SettingsCheckbox
                {
                    LabelText = MouseSettingsStrings.DisableMouseWheelVolumeAdjust,
                    TooltipText = MouseSettingsStrings.DisableMouseWheelVolumeAdjustTooltip,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableWheel)
                },
                new SettingsCheckbox
                {
                    LabelText = MouseSettingsStrings.DisableMouseButtons,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableButtons)
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            relativeMode.BindValueChanged(relative => 
            {
                localSensitivity.Disabled = !relative.NewValue;
                localSensitivityY.Disabled = !relative.NewValue || !separateMode.Value;
            }, true);

            handlerSensitivity.BindValueChanged(val =>
            {
                bool disabled = localSensitivity.Disabled;

                localSensitivity.Disabled = false;
                localSensitivity.Value = val.NewValue;
                localSensitivity.Disabled = disabled;
            }, true);

            localSensitivity.BindValueChanged(val =>
            {
                handlerSensitivity.Value = val.NewValue;

                if (!separateMode.Value)
                {
                    bool disabled = localSensitivityY.Disabled;

                    localSensitivityY.Disabled = false;
                    localSensitivityY.Value = val.NewValue;
                    localSensitivityY.Disabled = disabled;
                }
            });

            handlerSensitivityY.BindValueChanged(val =>
            {
                bool disabled = localSensitivityY.Disabled;

                localSensitivityY.Disabled = false;
                localSensitivityY.Value = val.NewValue;
                localSensitivityY.Disabled = disabled;
            }, true);

            localSensitivityY.BindValueChanged(val => handlerSensitivityY.Value = val.NewValue);

            windowMode.BindValueChanged(mode =>
            {
                bool isFullscreen = mode.NewValue == WindowMode.Fullscreen;

                if (isFullscreen)
                {
                    confineMouseModeSetting.Current.Disabled = true;
                    confineMouseModeSetting.TooltipText = MouseSettingsStrings.NotApplicableFullscreen;
                }
                else
                {
                    confineMouseModeSetting.Current.Disabled = false;
                    confineMouseModeSetting.TooltipText = string.Empty;
                }
            }, true);

            highPrecisionMouse.Current.BindValueChanged(highPrecision =>
            {
                if (RuntimeInfo.OS != RuntimeInfo.Platform.Windows)
                {
                    if (highPrecision.NewValue)
                        highPrecisionMouse.SetNoticeText(MouseSettingsStrings.HighPrecisionPlatformWarning, true);
                    else
                        highPrecisionMouse.ClearNoticeText();
                }
            }, true);

            separateSensitivity.Current.BindValueChanged(separate =>
            {
                localSensitivityY.Disabled = !separate.NewValue || !relativeMode.Value;
                horizontalSensitivity.LabelText = separate.NewValue ? MouseSettingsStrings.CursorHorizontalSensitivity : MouseSettingsStrings.CursorSensitivity;
            }, true);
        }

        public class SensitivitySetting : SettingsSlider<double, SensitivitySlider>
        {
            public SensitivitySetting()
            {
                KeyboardStep = 0.01f;
                TransferValueOnCommit = true;
            }
        }

        public class SensitivitySettingY : SettingsSlider<double, SensitivitySliderY>
        {
            public SensitivitySettingY()
            {
                KeyboardStep = 0.01f;
                TransferValueOnCommit = true;
            }
        }

        public class SensitivitySlider : OsuSliderBar<double>
        {
            public override LocalisableString TooltipText => Current.Disabled ? MouseSettingsStrings.EnableHighPrecisionForSensitivityAdjust : $"{base.TooltipText}x";
        }

        public class SensitivitySliderY : OsuSliderBar<double>
        {
            public override LocalisableString TooltipText => Current.Disabled ? MouseSettingsStrings.EnableSeparateSensitivity : $"{base.TooltipText}x";
        }
    }
}
