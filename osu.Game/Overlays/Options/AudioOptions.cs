using System;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options
{
    public class AudioOptions : OptionsSection
    {
        protected override string Header => "Audio";
        public override FontAwesome Icon => FontAwesome.fa_headphones;
    
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