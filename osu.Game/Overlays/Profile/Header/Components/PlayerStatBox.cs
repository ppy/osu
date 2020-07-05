using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class PlayerStatBox : ComponentContainer
    {
        protected override float cornerRadius => 0f;
        protected override bool masking => false;

        private OsuSpriteText title;
        private OsuSpriteText content;
        public string ContentText{ set => content.Text = value; }
        public string Title{ set => title.Text = value; }

        public PlayerStatBox()
        {
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Padding = new MarginPadding{ Horizontal = 15 },
                Children = new Drawable[]
                {
                    title = new OsuSpriteText
                    {
                        Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold)
                    },
                    content = new OsuSpriteText
                    {
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                        Font = OsuFont.GetFont(size: 20, weight: FontWeight.Light)
                    },
                }
            };
        }
    }
}