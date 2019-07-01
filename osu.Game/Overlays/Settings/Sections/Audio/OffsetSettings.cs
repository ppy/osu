// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public class OffsetSettings : SettingsSubsection
    {
        protected override string Header => "Offset Adjustment";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double, OffsetSlider>
                {
                    LabelText = "Audio offset",
                    Bindable = config.GetBindable<double>(OsuSetting.AudioOffset),
                    KeyboardStep = 1f
                },
                new SettingsButton
                {
                    Text = "Offset wizard"
                }
            };
        }

        private class OffsetSlider : OsuSliderBar<double>
        {
            public override string TooltipText => Current.Value.ToString(@"0ms");
        }
    }
}
