// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Android.OS;
using osu.Framework.Allocation;
using osu.Framework.Android.Input;
using osu.Framework.Bindables;
using osu.Framework.Extensions.IEnumerableExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Overlays.Settings.Sections.Input;

namespace osu.Android
{
    public partial class AndroidMouseSettings : SettingsSubsection
    {
        private readonly AndroidMouseHandler mouseHandler;

        protected override LocalisableString Header => MouseSettingsStrings.Mouse;

        private Bindable<Vector2d> handlerSensitivity = null!;

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

        private Bindable<bool> relativeMode = null!;

        private Bindable<bool> separateMode = null!;

        public AndroidMouseSettings(AndroidMouseHandler mouseHandler)
        {
            this.mouseHandler = mouseHandler;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig)
        {
            // use local bindable to avoid changing enabled state of game host's bindable.
            handlerSensitivity = mouseHandler.Sensitivity.GetBoundCopy();
            localSensitivityX.Value = handlerSensitivity.Value.X;
            localSensitivityY.Value = handlerSensitivity.Value.Y;

            separateMode = osuConfig.GetBindable<bool>(OsuSetting.UseSeparateSensitivity);
            relativeMode = mouseHandler.UseRelativeMode.GetBoundCopy();

            // High precision/pointer capture is only available on Android 8.0 and up
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                AddRange(new Drawable[]
                {
                    new SettingsCheckbox
                    {
                        LabelText = MouseSettingsStrings.HighPrecisionMouse,
                        TooltipText = MouseSettingsStrings.HighPrecisionMouseTooltip,
                        Current = relativeMode,
                        Keywords = new[] { @"raw", @"input", @"relative", @"cursor", @"captured", @"pointer" },
                    },
                    separateSensitivity = new SettingsCheckbox
                    {
                        LabelText = MouseSettingsStrings.SeparateSensitivity,
                        TooltipText = MouseSettingsStrings.SeparateSensitivityTooltip,
                        Current = separateMode,
                        Keywords = new[] { @"sensitivity", @"mouse", @"separate", @"cursor" }
                    },
                    horizontalSensitivity = new MouseSettings.SensitivitySettingX
                    {
                        LabelText = separateMode.Value ? MouseSettingsStrings.CursorHorizontalSensitivity : MouseSettingsStrings.CursorSensitivity,
                        Current = localSensitivityX
                    },
                    verticalSensitivitySettings = new FillFlowContainer<MouseSettings.SensitivitySettingY>
                    {
                       Direction = FillDirection.Vertical,
                       RelativeSizeAxes = Axes.X,
                       AutoSizeAxes = Axes.Y,
                       Masking = true,
                       Children = new[]
                       {
                           new MouseSettings.SensitivitySettingY
                           {
                               LabelText = MouseSettingsStrings.CursorVerticalSensitivity,
                               Current = localSensitivityY,
                           }
                       }
                    },
                });
            }

            AddRange(new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = MouseSettingsStrings.DisableMouseWheelVolumeAdjust,
                    TooltipText = MouseSettingsStrings.DisableMouseWheelVolumeAdjustTooltip,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableWheel),
                },
                new SettingsCheckbox
                {
                    LabelText = MouseSettingsStrings.DisableMouseButtons,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableButtons),
                },
            });
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
    }
}
