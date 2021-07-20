using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.Profile.Header.Components
{
    public class PlayerStatBox : ComponentContainer
    {
        protected override float cornerRadius => 0f;
        protected override bool masking => false;

        private readonly OsuSpriteText title;
        private readonly OsuSpriteText content;
        private readonly SpriteIcon icon;
        private readonly OsuSpriteText iconDescription;
        public string ContentText { set => content.Text = value; }
        public LocalisableString Title { set => title.Text = value; }
        public IconUsage Icon { set => icon.Icon = value; }
        public string IconDescription { set => iconDescription.Text = value; }

        public PlayerStatBox(float iconSize = 25)
        {
            Masking = true;
            Anchor = Anchor.Centre;
            Origin = Anchor.Centre;
            RelativeSizeAxes = Axes.Both;
            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Padding = new MarginPadding { Horizontal = 15 },
                Child = new FillFlowContainer
                {
                    Direction = FillDirection.Vertical,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Spacing = new Vector2(2.5f),
                    Children = new Drawable[]
                    {
                        icon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(iconSize)
                        },
                        iconDescription = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.Numeric.With(size: 25)
                        },
                        title = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Bold)
                        },
                        content = new OsuSpriteText
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Font = OsuFont.GetFont(size: 20, weight: FontWeight.Light)
                        },
                    }
                },
            };
        }
    }
}
