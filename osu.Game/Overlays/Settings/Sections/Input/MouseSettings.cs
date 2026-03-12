// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        private Bindable<bool> enabledPlayfieldRelative = null!;

        private FormCheckBox highPrecisionMouse = null!;

        private FormCheckBox enableSensitivityRelativeToPlayfield = null!;

        private readonly Bindable<SettingsNote.Data?> highPrecisionMouseNote = new Bindable<SettingsNote.Data?>();

        protected override bool IsToggleable => false;
        private Bindable<float> scalingSizeX = null!;
        private Bindable<float> scalingSizeY = null!;
        private float playfieldScale = 0.8f;
        private float defaultScale = 0.8f;

        // A entry guard that prevents the two callbacks from ping-ponging off each other.
        private bool isSyncing;

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

            enabledPlayfieldRelative = osuConfig.GetBindable<bool>(OsuSetting.SensitivityScaleWithPlayfieldSize);
            scalingSizeX = osuConfig.GetBindable<float>(OsuSetting.ScalingSizeX);
            scalingSizeY = osuConfig.GetBindable<float>(OsuSetting.ScalingSizeY);
            playfieldScale = Math.Min(scalingSizeX.Value, scalingSizeY.Value);
            float defaultScale = Math.Min(scalingSizeX.Default, scalingSizeY.Default);

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
                new SettingsItemV2(enableSensitivityRelativeToPlayfield = new FormCheckBox
                {
                    Caption = MouseSettingsStrings.EnableCursorSensitivityRelativeToPlayfield,
                    HintText = MouseSettingsStrings.EnableCursorSensitivityRelativeToPlayfieldToolTip,
                    Current = enabledPlayfieldRelative
                })
                {
                    Keywords = new[] { "speed", "velocity", "cursor" },
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

            relativeMode.BindValueChanged(relative =>
            {
                localSensitivity.Disabled = !relative.NewValue;
                enabledPlayfieldRelative.Disabled = !relative.NewValue;
            }, true);

            handlerSensitivity.BindValueChanged(val =>
            {
                if (isSyncing) return;

                isSyncing = true;

                bool disabled = localSensitivity.Disabled;

                localSensitivity.Disabled = false;

                if (enabledPlayfieldRelative.Value)
                    localSensitivity.Value = val.NewValue / (playfieldScale / defaultScale);
                else
                    localSensitivity.Value = val.NewValue;

                localSensitivity.Disabled = disabled;

                isSyncing = false;
            }, true);

            localSensitivity.BindValueChanged(val =>
            {
                if (isSyncing) return;

                isSyncing = true;

                if (enabledPlayfieldRelative.Value)
                    handlerSensitivity.Value = val.NewValue * (playfieldScale / defaultScale);
                else
                    handlerSensitivity.Value = val.NewValue;

                isSyncing = false;
            });

            enabledPlayfieldRelative.BindValueChanged(_ => syncToHandler());

            windowMode.BindValueChanged(_ => updateConfineMouseModeSettingVisibility());
            minimiseOnFocusLoss.BindValueChanged(_ => updateConfineMouseModeSettingVisibility(), true);

            scalingSizeX.BindValueChanged(_ => updatePlayfieldScale());
            scalingSizeY.BindValueChanged(_ => updatePlayfieldScale(), true);

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
        /// Pushes the current slider value (<see cref="localSensitivity"/>) to the actual cursor sensitivity
        /// (<see cref="handlerSensitivity"/>) used by the framework.
        /// When playfield-relative scaling is enabled, the handler value is adjusted by the ratio of current
        /// playfield scale to the default, so cursor speed stays proportional to the play area.
        /// Called when the playfield-relative toggle changes or when playfield scale sliders are adjusted.
        /// </summary>
        private void syncToHandler()
        {
            if (isSyncing) return;

            isSyncing = true;

            if (enabledPlayfieldRelative.Value)
                handlerSensitivity.Value = localSensitivity.Value * (playfieldScale / defaultScale);
            else
                handlerSensitivity.Value = localSensitivity.Value;

            isSyncing = false;
        }

        private void updatePlayfieldScale()
        {
            playfieldScale = Math.Min(scalingSizeX.Value, scalingSizeY.Value);
            syncToHandler();
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
