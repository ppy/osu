﻿// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Graphics;
using osu.Game.Configuration;
using osu.Game.Graphics.UserInterface;

namespace osu.Game.Overlays.Options.Sections.Audio
{
    public class VolumeOptions : OptionsSubsection
    {
        protected override string Header => "Volume";

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config, AudioManager audio)
        {
            Children = new Drawable[]
            {
                new OptionSlider<double> { LabelText = "Master", Bindable = audio.Volume },
                new OptionSlider<double> { LabelText = "Effect", Bindable = audio.VolumeSample },
                new OptionSlider<double> { LabelText = "Music", Bindable = audio.VolumeTrack },
                new OsuCheckbox
                {
                    LabelText = "Ignore beatmap hitsounds",
                    Bindable = config.GetBindable<bool>(OsuConfig.IgnoreBeatmapSamples),
                    TooltipText = "Custom hitsounds bundled with beatmaps will be ignored."
                }
            };
        }
    }
}
