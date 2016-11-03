using System;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Options
{
    public class AudioOptions : OptionsSection
    {
        public AudioOptions()
        {
            Header = "Audio";
            Children = new Drawable[]
            {
                new AudioDevicesOptions(),
                new VolumeOptions(),
                new OffsetAdjustmentOptions(),
            };
        }
    }
}