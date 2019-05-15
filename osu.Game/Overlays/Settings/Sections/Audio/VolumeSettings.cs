// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
                new SettingsSlider<double> { LabelText = "Master", Bindable = audio.Volume, KeyboardStep = 0.01f },
                new SettingsSlider<double> { LabelText = "Master (window inactive)", Bindable = config.GetBindable<double>(OsuSetting.VolumeInactive), KeyboardStep = 0.01f },
                new SettingsSlider<double> { LabelText = "Effect", Bindable = audio.VolumeSample, KeyboardStep = 0.01f },
                new SettingsSlider<double> { LabelText = "Music", Bindable = audio.VolumeTrack, KeyboardStep = 0.01f },
            };
        }
    }
}
