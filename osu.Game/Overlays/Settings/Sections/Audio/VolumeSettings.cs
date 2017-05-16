// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Settings.Sections.Audio
{
    public class VolumeSettings : SettingsSubsection
    {
        protected override string Header => "Volume";

        [BackgroundDependencyLoader]
        private void load(AudioManager audio)
        {
            Children = new Drawable[]
            {
                new SettingsSlider<double> { LabelText = "Master", Bindable = audio.Volume },
                new SettingsSlider<double> { LabelText = "Effect", Bindable = audio.VolumeSample },
                new SettingsSlider<double> { LabelText = "Music", Bindable = audio.VolumeTrack },
            };
        }
    }
}
