using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using OpenTK;

namespace osu.Game.Tournament.Screens.Showcase
{
    public class TournamentLogo : CompositeDrawable
    {
        public TournamentLogo()
        {
            RelativeSizeAxes = Axes.X;
            Height = 100;
            Margin = new MarginPadding { Vertical = 5 };
        }

        [BackgroundDependencyLoader]
        private void load(TextureStore textures)
        {
            InternalChild = new Sprite
            {
                Texture = textures.Get("game-screen-logo"),
                Anchor = Anchor.TopCentre,
                Origin = Anchor.TopCentre,
                FillMode = FillMode.Fit,
                RelativeSizeAxes = Axes.Both,
                Size = Vector2.One
            };
        }
    }
}
