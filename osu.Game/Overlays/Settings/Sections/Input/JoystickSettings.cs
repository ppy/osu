// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Joystick;
using osu.Framework.Localisation;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class JoystickSettings : SettingsSubsection
    {
        protected override LocalisableString Header => JoystickSettingsStrings.JoystickGamepad;

        private readonly JoystickHandler joystickHandler;

        private readonly Bindable<bool> enabled = new BindableBool(true);

        private SettingsSlider<float> deadzoneSlider;

        public JoystickSettings(JoystickHandler joystickHandler)
        {
            this.joystickHandler = joystickHandler;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
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
                    Current = joystickHandler.DeadzoneThreshold,
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            enabled.BindTo(joystickHandler.Enabled);
            enabled.BindValueChanged(e => deadzoneSlider.Current.Disabled = !e.NewValue, true);
        }
    }
}
