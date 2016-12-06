//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.UserInterface;
using osu.Game.Configuration;

namespace osu.Game.Overlays.Options.Audio
{
    public class VolumeOptions : OptionsSubsection
    {
        protected override string Header => "Volume";

        private CheckBoxOption ignoreHitsounds;

        public VolumeOptions()
        {
        }

        [BackgroundDependencyLoader]
        private void load(OsuConfigManager config)
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "Master: TODO slider" },
                new SpriteText { Text = "Music: TODO slider" },
                new SpriteText { Text = "Effect: TODO slider" },
                ignoreHitsounds = new CheckBoxOption
                {
                    LabelText = "Ignore beatmap hitsounds",
                    Bindable = config.GetBindable<bool>(OsuConfig.IgnoreBeatmapSamples)
                }
            };
        }
    }
}
