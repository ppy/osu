// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Localisation;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;

namespace osu.Game.Overlays.Settings.Sections.Graphics
{
    public partial class CalibrationSettings : SettingsSubsection
    {
        protected override LocalisableString Header => "Calibration";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double, AudioOffsetSlider>
                {
                    LabelText = AudioSettingsStrings.AudioOffset,
                    Current = config.GetBindable<double>(OsuSetting.AudioOffset),
                    KeyboardStep = 1,
                },
            };
        }

        private partial class AudioOffsetSlider : RoundedSliderBar<double>
        {
            public override LocalisableString TooltipText => Current.Value.ToString(@"0 ms");
        }
    }
}
