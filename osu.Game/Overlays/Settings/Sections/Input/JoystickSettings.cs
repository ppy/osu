// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Joystick;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public partial class JoystickSettings : InputSubsection
    {
        protected override LocalisableString Header => JoystickSettingsStrings.JoystickGamepad;

        private readonly JoystickHandler joystickHandler;

        private Bindable<float> handlerDeadzone;

        private Bindable<float> localDeadzone;

        public JoystickSettings(JoystickHandler joystickHandler)
            : base(joystickHandler)
        {
            this.joystickHandler = joystickHandler;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            // use local bindable to avoid changing enabled state of game host's bindable.
            handlerDeadzone = joystickHandler.DeadzoneThreshold.GetBoundCopy();
            localDeadzone = handlerDeadzone.GetUnboundCopy();

            AddRange(new Drawable[]
            {
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = JoystickSettingsStrings.DeadzoneThreshold,
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true,
                    Current = localDeadzone,
                })
            });
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

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
