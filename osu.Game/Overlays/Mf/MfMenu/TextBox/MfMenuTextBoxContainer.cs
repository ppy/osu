using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osuTK;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuTextBoxContainer : Container
    {
        private static void Titlefont(SpriteText t) => t.Font = OsuFont.GetFont(size: 30, weight: FontWeight.SemiBold);

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

        private Container baseContainer;
        private Container hoverEffectContainer;
        protected Container backgroundContainer;
        private FillFlowContainer contentFillFlow;


        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                baseContainer = new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        hoverEffectContainer = new Container
                        {
                            CornerRadius = 25,
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            EdgeEffect = edgeEffect,
                            Alpha = 0
                        },
                        backgroundContainer = new Container
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
                        contentFillFlow = new FillFlowContainer
                        {
                            Padding = new MarginPadding(25),
                            Spacing = new Vector2(15),
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y
                        },
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            if ( Title != null )
            {
                var titleTextFlow = new LinkFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    TextAnchor = Anchor.TopLeft,
                };

                var title = Title.ToCharArray();

                foreach(var c in title)
                {
                    titleTextFlow.AddText(c.ToString(), Titlefont);
                }

                contentFillFlow.Add(titleTextFlow);
            }

            if ( d != null )
            {
                contentFillFlow.Add(d);
                contentFillFlow.LayoutEasing = Easing.OutQuint;
                contentFillFlow.LayoutDuration = 750;
            }

            base.LoadComplete();
        }

        //我已经不知道要怎么处理光标悬浮时的动画了就这样吧
        protected override bool OnHover(HoverEvent e)
        {
            baseContainer.MoveToY(-5, 500, Easing.OutQuint);
            hoverEffectContainer.FadeIn(500, Easing.OutQuint);
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            baseContainer.MoveToY(0, 500, Easing.OutQuint);
            hoverEffectContainer.FadeOut(500, Easing.OutQuint);
            base.OnHoverLost(e);
        }
    }
}