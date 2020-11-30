// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings.Sections.UserInterface
{
    public class GeneralSettings : SettingsSubsection
    {
        protected override string Header => "General";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "Rotate cursor when dragging",
                    Current = config.GetBindable<bool>(OsuSetting.CursorRotation)
                },
                new SettingsSlider<float, SizeSlider>
                {
                    LabelText = "Menu cursor size",
                    Current = config.GetBindable<float>(OsuSetting.MenuCursorSize),
                    KeyboardStep = 0.01f
                },
                new SettingsCheckbox
                {
                    LabelText = "Parallax",
                    Current = config.GetBindable<bool>(OsuSetting.MenuParallax)
                },
                new SettingsSlider<float, TimeSlider>
                {
                    LabelText = "Hold-to-confirm activation time",
                    Current = config.GetBindable<float>(OsuSetting.UIHoldActivationDelay),
                    KeyboardStep = 50
                },
            };
        }

        private class TimeSlider : OsuSliderBar<float>
        {
            public override string TooltipText => Current.Value.ToString("N0") + "ms";
        }
    }
}
