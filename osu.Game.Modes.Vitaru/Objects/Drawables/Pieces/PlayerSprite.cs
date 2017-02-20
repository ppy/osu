using OpenTK;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Game.Modes.Vitaru.Objects.Characters;

namespace osu.Game.Modes.Vitaru.Objects.Drawables.Pieces
{
    public class PlayerSprite : Container
    {
        public Sprite sprite;

        public PlayerSprite()
        {
            sprite = new Sprite()
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                //Scale = new Vector2 (0.5f,0.5f)
            };
            Add(sprite);

        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            sprite.Texture = textures.Get(@"Play/Vitaru/player");
        }
    }
}