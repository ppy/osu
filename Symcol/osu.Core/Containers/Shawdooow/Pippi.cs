using osu.Framework.Allocation;
using osu.Framework.Graphics.Textures;
using osu.Game.Graphics;

namespace osu.Core.Containers.Shawdooow
{
    public class Pippi : Mascot
    {
        public Pippi()
        {

        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures, OsuColour colors)
        {
            SpeechBubbleBackground.Colour = colors.Pink;
            Idle.Texture = textures.Get("Menu/comboburst@2x");
        }
    }
}
