using M.Resources.Fonts;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Graphics.Mf
{
    public class FontInfoLabel : CompositeDrawable, IHasCustomTooltip
    {
        private readonly Font font;
        private readonly Box bg;

        public FontInfoLabel(Font font)
        {
            this.font = font;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            InternalChildren = new Drawable[]
            {
                bg = new Box
                {
                    Colour = Color4Extensions.FromHex("#18171c"),
                    RelativeSizeAxes = Axes.Both
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 28,
                    Spacing = new Vector2(5),
                    Direction = FillDirection.Horizontal,
                    Children = new Drawable[]
                    {
                        new FontPreviewSpriteText(font)
                        {
                            Text = font.Name,
                            Margin = new MarginPadding { Left = 5 },
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft
                        },
                        new Circle
                        {
                            RelativeSizeAxes = Axes.Y,
                            Width = 3,
                            Colour = Color4Extensions.FromHex("#777"),
                            Padding = new MarginPadding { Vertical = 5 }
                        },
                        new OsuSpriteText
                        {
                            Text = $"作者: {font.Author}",
                            Font = new FontUsage("Noto-CJK-Basic"),
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            NoFontAutoUpdate = true
                        }
                    }
                }
            };
        }

        protected override bool OnHover(HoverEvent e)
        {
            bg.FadeColour(Color4Extensions.FromHex("#333"), 300);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            bg.FadeColour(Color4Extensions.FromHex("#18171c"), 300);
            base.OnHoverLost(e);
        }

        public ITooltip GetCustomTooltip() => new FontInfoTooltip();

        public object TooltipContent => font;
    }
}
