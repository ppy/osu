using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Modes.Vitaru.Objects.Characters;

namespace osu.Game.Modes.Vitaru.Objects.Drawables.Pieces
{
    public class EnemySprite : Container
    {
        public Sprite sprite;

        public EnemySprite()
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
            sprite.Texture = textures.Get(@"Play/Vitaru/enemy");
        }
    }
}