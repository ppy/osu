// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public class VolumeSettings : SettingsSubsection
    {
        protected override string Header => "Volume";

        [BackgroundDependencyLoader]
        private void load(AudioManager audio, OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double> { LabelText = "Master", Bindable = audio.Volume, KeyboardStep = 0.1f },
                new SettingsSlider<double> { LabelText = "Master (Window Inactive)", Bindable = config.GetBindable<double>(OsuSetting.VolumeInactive), KeyboardStep = 0.1f },
                new SettingsSlider<double> { LabelText = "Effect", Bindable = audio.VolumeSample, KeyboardStep = 0.1f },
                new SettingsSlider<double> { LabelText = "Music", Bindable = audio.VolumeTrack, KeyboardStep = 0.1f },
            };
        }
    }
}
