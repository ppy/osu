// Copyright (c) 2007-2017 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

namespace osu.Game.Overlays.Options.Sections.Audio
{
    public class AudioDevicesOptions : OptionsSubsection
    {
        protected override string Header => "Devices";

        public AudioDevicesOptions()
        {
            Children = new[]
            {
                new OptionLabel { Text = "Output device: TODO dropdown" }
            };
        }
    }
}