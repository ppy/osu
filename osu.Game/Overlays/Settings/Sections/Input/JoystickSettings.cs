// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Joystick;
using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class JoystickSettings : SettingsSubsection
    {
        protected override LocalisableString Header => JoystickSettingsStrings.JoystickGamepad;

        private readonly JoystickHandler joystickHandler;

        private readonly Bindable<bool> enabled = new BindableBool(true);

        private SettingsSlider<float> deadzoneSlider;

        private Bindable<float> handlerDeadzone;

        private Bindable<float> localDeadzone;

        public JoystickSettings(JoystickHandler joystickHandler)
        {
            this.joystickHandler = joystickHandler;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // use local bindable to avoid changing enabled state of game host's bindable.
            handlerDeadzone = joystickHandler.DeadzoneThreshold.GetBoundCopy();
            localDeadzone = handlerDeadzone.GetUnboundCopy();

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = CommonStrings.Enabled,
                    Current = enabled
                },
                deadzoneSlider = new SettingsSlider<float>
                {
                    LabelText = JoystickSettingsStrings.DeadzoneThreshold,
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true,
                    Current = localDeadzone,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            enabled.BindTo(joystickHandler.Enabled);
            enabled.BindValueChanged(e => deadzoneSlider.Current.Disabled = !e.NewValue, true);

            handlerDeadzone.BindValueChanged(val =>
            {
                bool disabled = localDeadzone.Disabled;

                localDeadzone.Disabled = false;
                localDeadzone.Value = val.NewValue;
                localDeadzone.Disabled = disabled;
            }, true);

            localDeadzone.BindValueChanged(val => handlerDeadzone.Value = val.NewValue);
        }
    }
}
