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
            Colour = Colour4.Black.Opacity(0.35f),
            Radius = 18,
        };

        private Container baseContainer;
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
                        backgroundContainer = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = 25,
                            Masking = true,
                            BorderColour = colourProvider.Light1,
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
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(25),
                            Spacing = new Vector2(15),
                            Masking = true,
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
                contentFillFlow.Add(d);

            contentFillFlow.LayoutEasing = Easing.OutQuint;
            contentFillFlow.LayoutDuration = 750;

            base.LoadComplete();
        }

        //我已经不知道要怎么处理光标悬浮时的动画了就这样吧
        protected override bool OnHover(HoverEvent e)
        {
            backgroundContainer.BorderThickness = 2;
            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            backgroundContainer.BorderThickness = 0;
            base.OnHoverLost(e);
        }
    }
}