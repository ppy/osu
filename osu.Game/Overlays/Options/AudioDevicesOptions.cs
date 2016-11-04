using System;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Options
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