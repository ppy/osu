using System;
using osu.Framework.Graphics;

namespace osu.Game.Overlays.Options
{
    public class AudioOptions : OptionsSection
    {
        protected override string Header => "Audio";
    
        public AudioOptions()
        {
            Children = new Drawable[]
            {
                new AudioDevicesOptions(),
                new VolumeOptions(),
                new OffsetAdjustmentOptions(),
            };
        }
    }
}