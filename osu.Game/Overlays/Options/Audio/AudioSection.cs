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
                new AudioDevicesOptions(),
                new VolumeOptions(),
                new OffsetAdjustmentOptions(),
            };
        }
    }
}