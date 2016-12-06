//Copyright (c) 2007-2016 ppy Pty Ltd <contact@ppy.sh>.
//Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Options.Gameplay
{
    public class SongSelectGameplayOptions : OptionsSubsection
    {
        protected override string Header => "Song Select";

        public SongSelectGameplayOptions()
        {
            Children = new Drawable[]
            {
                new SpriteText { Text = "Display beatmaps from: TODO slider" },
                new SpriteText { Text = "up to: TODO slider" },
            };
        }
    }
}

