//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Graphics;
using osu.Game.Graphics;

namespace osu.Game.Overlays.Options.Audio
{
    public class AudioSection : OptionsSection
    {
        public override string Header => "Audio";
        public override FontAwesome Icon => FontAwesome.fa_headphones;

        public AudioSection()
        {
            Children = new Drawable[]
            {
                new AudioDevicesOptions { Alpha = RuntimeInfo.IsWindows ? 1 : 0 },
                new VolumeOptions(),
                new OffsetAdjustmentOptions(),
            };
        }
    }
}