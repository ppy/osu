using System;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Options
{
    public class AudioDevicesOptions : OptionsSubsection
    {
        public AudioDevicesOptions()
        {
            Header = "Devices";
            Children = new[]
            {
                new SpriteText { Text = "Output device: TODO dropdown" }
            };
        }
    }
}