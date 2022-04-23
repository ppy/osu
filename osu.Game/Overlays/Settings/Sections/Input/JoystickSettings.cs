// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Handlers.Joystick;
using osu.Framework.Localisation;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Input
{
    public class JoystickSettings : SettingsSubsection
    {
        private readonly JoystickHandler joystickHandler;
        protected override LocalisableString Header => JoystickSettingsStrings.JoystickGamepad;
        private readonly BindableNumber<float> deadzoneThreshold = new BindableNumber<float>();
        private readonly Bindable<bool> enabled = new BindableBool(true);
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
                new DeadzoneSetting
                {
                    LabelText = JoystickSettingsStrings.DeadzoneThreshold,
                    Current = deadzoneThreshold
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            enabled.BindTo(joystickHandler.Enabled);
            deadzoneThreshold.BindTo(joystickHandler.DeadzoneThreshold);
            enabled.BindValueChanged(e => deadzoneThreshold.Disabled = !e.NewValue, true);
        }

        private class DeadzoneSetting : SettingsSlider<float, DeadzoneSlider>
        {
            public DeadzoneSetting()
            {
                KeyboardStep = 0.005f;
                TransferValueOnCommit = true;
            }
        }
        private class DeadzoneSlider : OsuSliderBar<float>
        {
            public override LocalisableString TooltipText => Current.Disabled ? "" : base.TooltipText;
        }
    }
}