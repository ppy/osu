using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class RingPiece : Container
    {
        private Sprite ring;

        public RingPiece()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;

            Children = new Drawable[]
            {
                ring = new Sprite
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre
                }
            };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            ring.Texture = textures.Get(@"Play/osu/ring@2x");
        }
    }
}