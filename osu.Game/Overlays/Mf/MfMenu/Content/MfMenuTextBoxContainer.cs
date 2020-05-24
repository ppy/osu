using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osuTK;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuTextBoxContainer : Container
    {
        public Drawable d;
        public float HoverScale = 1.025f;
        public string Title { get; set; }

        private OverlayColourProvider colourProvider  = new OverlayColourProvider(OverlayColourScheme.Orange);

        private EdgeEffectParameters edgeEffect = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Colour = Colour4.Black.Opacity(0.3f),
            Radius = 18,
        };

        private Container hover;
        private Container content;
        private FillFlowContainer textFillFlow;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                content = new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        hover = new Container
                        {
                            CornerRadius = 25,
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            EdgeEffect = edgeEffect,
                            Alpha = 0
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = 25,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background6,
                                },
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Alpha = 0.3f,
                                    Colour = Colour4.Black,
                                },
                            }
                        },
                        textFillFlow = new FillFlowContainer
                        {
                            Padding = new MarginPadding(25),
                            Spacing = new Vector2(15),
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        },
                    }
                },
            };

            textFillFlow.Add(new OsuSpriteText
            {
                Text = Title,
                Font = OsuFont.GetFont(size: 30, weight: FontWeight.SemiBold),
            });
            if ( d != null )
            {
                textFillFlow.Add(d);
            }
        }

        //我已经不知道要怎么处理光标悬浮时的动画了就这样吧
        protected override bool OnHover(HoverEvent e)
        {
            content.MoveToY(-5, 500, Easing.OutQuint);
            hover.FadeIn(500, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            content.MoveToY(0, 500, Easing.OutQuint);
            d.MoveToY(0, 500, Easing.OutQuint);
            hover.FadeOut(500, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}