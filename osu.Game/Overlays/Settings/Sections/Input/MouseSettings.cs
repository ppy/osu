// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class MouseSettings : InputSubsection
    {
        private readonly MouseHandler mouseHandler;

        protected override LocalisableString Header => MouseSettingsStrings.Mouse;

        private Bindable<double> handlerSensitivity = null!;
        private Bindable<double> localSensitivity = null!;
        private Bindable<WindowMode> windowMode = null!;
        private Bindable<bool> minimiseOnFocusLoss = null!;
        private FormEnumDropdown<OsuConfineMouseMode> confineMouseModeSetting = null!;
        private Bindable<bool> relativeMode = null!;

        private FormCheckBox highPrecisionMouse = null!;

        private readonly Bindable<SettingsNote.Data?> highPrecisionMouseNote = new Bindable<SettingsNote.Data?>();

        protected override bool IsToggleable => false;

        public MouseSettings(MouseHandler mouseHandler)
            : base(mouseHandler)
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

            AddRange(new Drawable[]
            {
                new SettingsItemV2(highPrecisionMouse = new FormCheckBox
                {
                    Caption = MouseSettingsStrings.HighPrecisionMouse,
                    HintText = MouseSettingsStrings.HighPrecisionMouseTooltip,
                    Current = relativeMode,
                })
                {
                    Keywords = new[] { @"raw", @"input", @"relative", @"cursor", "sensitivity", "speed", "velocity" },
                    Note = { BindTarget = highPrecisionMouseNote },
                },
                new SettingsItemV2(new FormSliderBar<double>
                {
                    Caption = MouseSettingsStrings.CursorSensitivity,
                    Current = localSensitivity,
                    KeyboardStep = 0.01f,
                    TransferValueOnCommit = true,
                    LabelFormat = v => $@"{v:0.##}x",
                    TooltipFormat = v => localSensitivity.Disabled ? MouseSettingsStrings.EnableHighPrecisionForSensitivityAdjust : $@"{v:0.##}x",
                })
                {
                    Keywords = new[] { "speed", "velocity" },
                },
                new SettingsItemV2(confineMouseModeSetting = new FormEnumDropdown<OsuConfineMouseMode>
                {
                    Caption = MouseSettingsStrings.ConfineMouseMode,
                    Current = osuConfig.GetBindable<OsuConfineMouseMode>(OsuSetting.ConfineMouseMode)
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = MouseSettingsStrings.DisableMouseWheelVolumeAdjust,
                    HintText = MouseSettingsStrings.DisableMouseWheelVolumeAdjustTooltip,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableWheel)
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = MouseSettingsStrings.DisableClicksDuringGameplay,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableButtons)
                }),
            });
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
                            highPrecisionMouseNote.Value = new SettingsNote.Data(MouseSettingsStrings.HighPrecisionPlatformWarning, SettingsNote.Type.Warning);
                        else
                            highPrecisionMouseNote.Value = null;

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
                confineMouseModeSetting.HintText = MouseSettingsStrings.NotApplicableFullscreen;
            }
            else
            {
                confineMouseModeSetting.Current.Disabled = false;
                confineMouseModeSetting.HintText = default;
            }
        }
    }
}
