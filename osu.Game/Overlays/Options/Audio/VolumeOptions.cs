//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Audio
{
    public class VolumeOptions : OptionsSubsection
    {
        protected override string Header => "Volume";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, AudioManager audio)
        {
            Children = new Drawable[]
            {
                new OptionsSlider<double> { Label = "Master", Bindable = audio.Volume },
                new OptionsSlider<double> { Label = "Effect", Bindable = audio.VolumeSample },
                new OptionsSlider<double> { Label = "Music", Bindable = audio.VolumeTrack },
                new CheckBoxOption
                {
                    LabelText = "Ignore beatmap hitsounds",
                    Bindable = config.GetBindable<bool>(OsuConfig.IgnoreBeatmapSamples)
                }
            };
        }
    }
}
