using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;

namespace osu.Game.Modes.Vitaru.Objects.Drawables.Pieces
{
    class Glow : Container
    {
        public Sprite sprite;
        public string thisSprite = "null";

        public Glow()
        {
            sprite = new Sprite()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
            };
            Add(sprite);

        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            sprite.Texture = textures.Get(@"Play/Vitaru/" + thisSprite + "glow");
        }
    }
}