// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Input.Handlers.Mouse;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Input;
using osu.Game.Localisation;
using osuTK;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class MouseSettings : SettingsSubsection
    {
        private readonly MouseHandler mouseHandler;

        protected override LocalisableString Header => MouseSettingsStrings.Mouse;

        private Bindable<Vector2d> handlerSensitivity;

        private BindableDouble localSensitivityX = new BindableDouble(1)
        {
            MinValue = 0.1,
            MaxValue = 10,
            Precision = 0.01
        };

        private BindableDouble localSensitivityY = new BindableDouble(1)
        {
            MinValue = 0.1,
            MaxValue = 10,
            Precision = 0.01
        };

        private Bindable<WindowMode> windowMode;
        private SettingsEnumDropdown<OsuConfineMouseMode> confineMouseModeSetting;
        private Bindable<bool> relativeMode;

        private Bindable<bool> separateMode;

        private SettingsCheckbox highPrecisionMouse;

        private SettingsCheckbox separateSensitivity;

        private SensitivitySettingX horizontalSensitivity;

        private FillFlowContainer<SensitivitySettingY> verticalSensitivitySettings = null!;

        private const int transition_duration = 200;

        public MouseSettings(MouseHandler mouseHandler)
        {
            this.mouseHandler = mouseHandler;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig, FrameworkConfigManager config)
        {
            // use local bindable to avoid changing enabled state of game host's bindable.
            handlerSensitivity = mouseHandler.Sensitivity.GetBoundCopy();
            localSensitivityX.Value = handlerSensitivity.Value.X;
            localSensitivityY.Value = handlerSensitivity.Value.Y;

            relativeMode = mouseHandler.UseRelativeMode.GetBoundCopy();
            separateMode = osuConfig.GetBindable<bool>(OsuSetting.UseSeparateSensitivity);
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
                horizontalSensitivity = new SensitivitySettingX
                {
                    LabelText = separateMode.Value ? MouseSettingsStrings.CursorHorizontalSensitivity : MouseSettingsStrings.CursorSensitivity,
                    Current = localSensitivityX
                },
                verticalSensitivitySettings = new FillFlowContainer<SensitivitySettingY>
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Masking = true,
                    Children = new[]
                    {
                        new SensitivitySettingY
                        {
                            LabelText = MouseSettingsStrings.CursorVerticalSensitivity,
                            Current = localSensitivityY,
                        }
                    }
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
                localSensitivityX.Disabled = !relative.NewValue;
                localSensitivityY.Disabled = !relative.NewValue || !separateMode.Value;
            }, true);

            handlerSensitivity.BindValueChanged(val =>
            {
                bool disabledX = localSensitivityX.Disabled;
                bool disabledY = localSensitivityY.Disabled;

                localSensitivityX.Disabled = false;
                localSensitivityY.Disabled = false;
                localSensitivityX.Value = val.NewValue.X;
                localSensitivityY.Value = val.NewValue.Y;
                localSensitivityX.Disabled = disabledX;
                localSensitivityY.Disabled = disabledY;
            }, true);

            localSensitivityX.BindValueChanged(val =>
            {
                handlerSensitivity.Value = new Vector2d(val.NewValue, localSensitivityY.Value);

                if (!separateMode.Value)
                {
                    bool disabled = localSensitivityY.Disabled;

                    localSensitivityY.Disabled = false;
                    localSensitivityY.Value = val.NewValue;
                    localSensitivityY.Disabled = disabled;
                }
            });

            localSensitivityY.BindValueChanged(val => handlerSensitivity.Value = new Vector2d(localSensitivityX.Value, val.NewValue));

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
                if (separate.NewValue)
                    handlerSensitivity.Value = new Vector2d(localSensitivityX.Value, localSensitivityY.Value);
                else
                    handlerSensitivity.Value = new Vector2d(localSensitivityX.Value, localSensitivityX.Value);

                localSensitivityY.Disabled = !separate.NewValue || !relativeMode.Value;
                verticalSensitivitySettings.ClearTransforms();
                verticalSensitivitySettings.AutoSizeDuration = transition_duration;
                verticalSensitivitySettings.AutoSizeEasing = Easing.OutQuint;

                updateScalingModeVisibility();
                horizontalSensitivity.LabelText = separate.NewValue ? MouseSettingsStrings.CursorHorizontalSensitivity : MouseSettingsStrings.CursorSensitivity;
            }, true);

            // initial update bypasses transforms
            updateScalingModeVisibility();

            void updateScalingModeVisibility()
            {
                if (!separateMode.Value)
                    verticalSensitivitySettings.ResizeHeightTo(0, transition_duration, Easing.OutQuint);

                verticalSensitivitySettings.AutoSizeAxes = separateMode.Value ? Axes.Y : Axes.None;
                verticalSensitivitySettings.ForEach(s =>
                {
                    s.TransferValueOnCommit = separateMode.Value;
                    s.CanBeShown.Value = separateMode.Value;
                });
            }
        }

        public partial class SensitivitySettingX : SettingsSlider<double, SensitivitySliderX>
        {
            public SensitivitySettingX()
            {
                KeyboardStep = 0.01f;
                TransferValueOnCommit = true;
            }
        }

        public partial class SensitivitySettingY : SettingsSlider<double, SensitivitySliderY>
        {
            public SensitivitySettingY()
            {
                KeyboardStep = 0.01f;
                TransferValueOnCommit = true;
            }
        }

        public partial class SensitivitySliderX : OsuSliderBar<double>
        {
            public override LocalisableString TooltipText => Current.Disabled ? MouseSettingsStrings.EnableHighPrecisionForSensitivityAdjust : $"{base.TooltipText}x";
        }

        public partial class SensitivitySliderY : OsuSliderBar<double>
        {
            public override LocalisableString TooltipText => Current.Disabled ? MouseSettingsStrings.EnableSeparateSensitivity : $"{base.TooltipText}x";
        }
    }
}
