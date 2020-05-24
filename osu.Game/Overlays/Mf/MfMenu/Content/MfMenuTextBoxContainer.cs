using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuTextBoxContainer : OsuClickableContainer
    {
        public Drawable d;

        public float HoverScale = 1.05f;

        private OverlayColourProvider colourProvider  = new OverlayColourProvider(OverlayColourScheme.Orange);
        
        private EdgeEffectParameters edgeEffect = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Colour = Colour4.Black.Opacity(0.3f),
            Radius = 12,
        };

        private Container hover;
        private Container content;

        public MfMenuTextBoxContainer()
        {
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            Children = new Drawable[]
            {
                content = new Container
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new[]
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
                        }
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            if ( d != null )
            {
                content.Add(d);
            }
            base.LoadComplete();
        }

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