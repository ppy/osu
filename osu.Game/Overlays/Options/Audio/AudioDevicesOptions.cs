//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Options.Audio
{
    public class AudioDevicesOptions : OptionsSubsection
    {
        protected override string Header => "Devices";

        public AudioDevicesOptions()
        {
            Children = new[]
            {
                new SpriteText { Text = "Output device: TODO dropdown" }
            };
        }
    }
}