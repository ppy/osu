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

