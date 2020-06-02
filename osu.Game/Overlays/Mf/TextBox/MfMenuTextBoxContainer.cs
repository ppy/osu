using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuTextBoxContainer : Container
    {
        private static void Titlefont(SpriteText t) => t.Font = OsuFont.GetFont(size: 30, weight: FontWeight.SemiBold);

        public Drawable d;
        public float HoverScale = 1.025f;
        public string Title { get; set; }

        private FillFlowContainer contentFillFlow;

        protected virtual bool Clickable => false;
        protected BindableBool AllowTransformBasicEffects = new BindableBool();
        protected BindableFloat borderThickness = new BindableFloat();
        protected float cornerRadius = 12.5f;
        protected float DURATION = 500;
        protected Easing EASING = Easing.OutQuint;

        protected EdgeEffectParameters edgeEffectNormal = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Radius = 0,
            Colour = Color4.Black.Opacity(0),
        };
        protected EdgeEffectParameters edgeEffectHover = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Radius = 7,
            Offset = new Vector2(0, 3.5f),
            Colour = Color4.Black.Opacity(0.35f),
        };

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = true;
            EdgeEffect = edgeEffectNormal;
            BorderColour = colourProvider.Light1;
            CornerRadius = cornerRadius;
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Children = new Drawable[]
            {
                new Container
                {
                    Anchor = Anchor.TopCentre,
                    Origin = Anchor.TopCentre,
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background5,
                        },
                        new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.3f,
                            Colour = Colour4.Black,
                        },
                        contentFillFlow = new FillFlowContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding(25),
                            Spacing = new Vector2(15),
                            Masking = true,
                            LayoutEasing = EASING,
                            LayoutDuration = DURATION + 250,
                        },
                        SelectSounds()
                    }
                },
            };

            AllowTransformBasicEffects.Value = true;
        }

        protected override void LoadComplete()
        {
            borderThickness.BindValueChanged(OnborderThicknessChanged);

            if ( Title != null )
            {
                var titleTextFlow = new LinkFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    TextAnchor = Anchor.TopLeft,
                };

                //将传入的标题转化为charArray以避免ui缩放导致文字伸出容器
                var title = Title.ToCharArray();

                foreach(var c in title)
                {
                    titleTextFlow.AddText(c.ToString(), Titlefont);
                }

                contentFillFlow.Add(titleTextFlow);
            }

            if ( d != null )
                contentFillFlow.Add(d);

            base.LoadComplete();
        }

        private Drawable SelectSounds()
        {
            Drawable s;

            if ( Clickable )
                s = new HoverClickSounds();
            else
                s = new HoverSounds();

            return s;
        }

        private void OnborderThicknessChanged(ValueChangedEvent<float> v)
        {
            this.BorderThickness = v.NewValue;
        }

        //我已经不知道要怎么处理光标悬浮时的动画了就这样吧
        protected override bool OnHover(HoverEvent e)
        {
            if ( AllowTransformBasicEffects.Value )
            {
                this.TransformBindableTo(borderThickness, 2);
                this.TweenEdgeEffectTo(edgeEffectHover, DURATION, EASING);
            }

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if ( AllowTransformBasicEffects.Value )
            {
                this.TransformBindableTo(borderThickness, 0);
                this.TweenEdgeEffectTo(edgeEffectNormal, DURATION, EASING);
            }

            base.OnHoverLost(e);
        }
    }
}