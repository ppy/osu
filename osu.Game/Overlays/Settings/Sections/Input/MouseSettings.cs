﻿// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
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
    public partial class MouseSettings : SettingsSubsection
    {
        private readonly MouseHandler mouseHandler;

        protected override LocalisableString Header => MouseSettingsStrings.Mouse;

        private Bindable<double> handlerSensitivity;

        private Bindable<double> localSensitivity;

        private Bindable<WindowMode> windowMode;
        private Bindable<bool> minimiseOnFocusLoss;
        private SettingsEnumDropdown<OsuConfineMouseMode> confineMouseModeSetting;
        private Bindable<bool> relativeMode;

        private SettingsCheckbox highPrecisionMouse;

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

            relativeMode = mouseHandler.UseRelativeMode.GetBoundCopy();
            windowMode = config.GetBindable<WindowMode>(FrameworkSetting.WindowMode);
            minimiseOnFocusLoss = config.GetBindable<bool>(FrameworkSetting.MinimiseOnFocusLossInFullscreen);

            Children = new Drawable[]
            {
                highPrecisionMouse = new SettingsCheckbox
                {
                    LabelText = MouseSettingsStrings.HighPrecisionMouse,
                    TooltipText = MouseSettingsStrings.HighPrecisionMouseTooltip,
                    Current = relativeMode,
                    Keywords = new[] { @"raw", @"input", @"relative", @"cursor" }
                },
                new SensitivitySetting
                {
                    LabelText = MouseSettingsStrings.CursorSensitivity,
                    Current = localSensitivity
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
                    LabelText = MouseSettingsStrings.DisableClicksDuringGameplay,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableButtons)
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            relativeMode.BindValueChanged(relative => localSensitivity.Disabled = !relative.NewValue, true);

            handlerSensitivity.BindValueChanged(val =>
            {
                bool disabled = localSensitivity.Disabled;

                localSensitivity.Disabled = false;
                localSensitivity.Value = val.NewValue;
                localSensitivity.Disabled = disabled;
            }, true);

            localSensitivity.BindValueChanged(val => handlerSensitivity.Value = val.NewValue);

            windowMode.BindValueChanged(_ => updateConfineMouseModeSettingVisibility());
            minimiseOnFocusLoss.BindValueChanged(_ => updateConfineMouseModeSettingVisibility(), true);

            highPrecisionMouse.Current.BindValueChanged(highPrecision =>
            {
                switch (RuntimeInfo.OS)
                {
                    case RuntimeInfo.Platform.Linux:
                    case RuntimeInfo.Platform.macOS:
                    case RuntimeInfo.Platform.iOS:
                        if (highPrecision.NewValue)
                            highPrecisionMouse.SetNoticeText(MouseSettingsStrings.HighPrecisionPlatformWarning, true);
                        else
                            highPrecisionMouse.ClearNoticeText();

                        break;
                }
            }, true);
        }

        /// <summary>
        /// Updates disabled state and tooltip of <see cref="confineMouseModeSetting"/> to match when <see cref="ConfineMouseTracker"/> is overriding the confine mode.
        /// </summary>
        private void updateConfineMouseModeSettingVisibility()
        {
            bool confineModeOverriden = windowMode.Value == WindowMode.Fullscreen && minimiseOnFocusLoss.Value;

            if (confineModeOverriden)
            {
                confineMouseModeSetting.Current.Disabled = true;
                confineMouseModeSetting.TooltipText = MouseSettingsStrings.NotApplicableFullscreen;
            }
            else
            {
                confineMouseModeSetting.Current.Disabled = false;
                confineMouseModeSetting.TooltipText = string.Empty;
            }
        }

        public partial class SensitivitySetting : SettingsSlider<double, SensitivitySlider>
        {
            public SensitivitySetting()
            {
                KeyboardStep = 0.01f;
                TransferValueOnCommit = true;
            }
        }

        public partial class SensitivitySlider : RoundedSliderBar<double>
        {
            public override LocalisableString TooltipText => Current.Disabled ? MouseSettingsStrings.EnableHighPrecisionForSensitivityAdjust : $"{base.TooltipText}x";
        }
    }
}
