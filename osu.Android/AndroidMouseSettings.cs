// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Android.OS;
using osu.Framework.Allocation;
using osu.Framework.Android.Input;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
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

        private Bindable<double> handlerSensitivity = null!;

        private Bindable<double> localSensitivity = null!;

        private Bindable<bool> relativeMode = null!;

        public AndroidMouseSettings(AndroidMouseHandler mouseHandler)
        {
            this.mouseHandler = mouseHandler;
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager osuConfig)
        {
            // use local bindable to avoid changing enabled state of game host's bindable.
            handlerSensitivity = mouseHandler.Sensitivity.GetBoundCopy();
            localSensitivity = handlerSensitivity.GetUnboundCopy();

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
                    new MouseSettings.SensitivitySetting
                    {
                        LabelText = MouseSettingsStrings.CursorSensitivity,
                        Current = localSensitivity,
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
                    LabelText = MouseSettingsStrings.DisableClicksDuringGameplay,
                    Current = osuConfig.GetBindable<bool>(OsuSetting.MouseDisableButtons),
                },
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
        }
    }
}
