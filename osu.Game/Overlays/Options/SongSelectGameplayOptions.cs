using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Overlays.Options
{
    public class SongSelectGameplayOptions : OptionsSubsection
    {
        public SongSelectGameplayOptions()
        {
            Header = "Song Select";
            Children = new Drawable[]
            {
                new SpriteText { Text = "Display beatmaps from: TODO slider" },
                new SpriteText { Text = "up to: TODO slider" },
            };
        }
    }
}

