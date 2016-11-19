using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.MathUtils;
using OpenTK;

namespace osu.Game.Modes.Osu.Objects.Drawables.Pieces
{
    public class Triangles : Container
    {
        private Texture triangle;

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            triangle = textures.Get(@"Play/osu/triangle@2x");
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();
            for (int i = 0; i < 10; i++)
            {
                Add(new Sprite
                {
                    Texture = triangle,
                    Origin = Anchor.Centre,
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2(RNG.NextSingle(), RNG.NextSingle()),
                    Scale = new Vector2(RNG.NextSingle() * 0.4f + 0.2f),
                    Alpha = RNG.NextSingle() * 0.3f
                });
            }
        }

        protected override void Update()
        {
            base.Update();

            foreach (Drawable d in Children)
                d.Position -= new Vector2(0, (float)(d.Scale.X * (Time.Elapsed / 2880)));
        }
    }
}