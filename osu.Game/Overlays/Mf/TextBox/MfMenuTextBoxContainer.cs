using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Overlays.Mf.TextBox
{
    public class MfMenuTextBoxContainer : Container
    {
        public string Title;

        protected virtual bool Clickable => false;
        protected BindableBool AllowTransformBasicEffects = new BindableBool();
        protected BindableFloat ContentBorderThickness = new BindableFloat();
        protected const float ANIMATION_DURATION = 500;
        protected const Easing ANIMATION_EASING = Easing.OutQuint;

        protected EdgeEffectParameters EdgeEffectNormal = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Radius = 0,
            Colour = Color4.Black.Opacity(0),
        };

        protected EdgeEffectParameters EdgeEffectHover = new EdgeEffectParameters
        {
            Type = EdgeEffectType.Shadow,
            Radius = 7,
            Offset = new Vector2(0, 3.5f),
            Colour = Color4.Black.Opacity(0.35f),
        };

        protected override Container<Drawable> Content { get; } = new Container
        {
            AutoSizeAxes = Axes.Y,
            RelativeSizeAxes = Axes.X
        };

        public MfMenuTextBoxContainer()
        {
            Masking = true;
            EdgeEffect = EdgeEffectNormal;
            CornerRadius = 12.5f;
            Anchor = Anchor.TopCentre;
            Origin = Anchor.TopCentre;
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            BorderColour = colourProvider.Light1;
            AllowTransformBasicEffects.Value = true;

            InternalChildren = new[]
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
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Padding = new MarginPadding(25),
                    Spacing = new Vector2(15),
                    Masking = true,
                    LayoutEasing = ANIMATION_EASING,
                    LayoutDuration = ANIMATION_DURATION + 250,
                    Children = new Drawable[]
                    {
                        //bug: 不套Container会导致部分文本抽搐，加了会导致容器高度异常
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Child = new OsuSpriteText
                            {
                                RelativeSizeAxes = Axes.X,
                                Text = Title,
                                Font = OsuFont.GetFont(size: 30, weight: FontWeight.SemiBold)
                            }
                        },
                        Content
                    }
                },
                selectSounds()
            };
        }

        public override void Clear(bool disposeChildren) => Content.Clear(disposeChildren);

        protected override void LoadComplete()
        {
            ContentBorderThickness.BindValueChanged(OnborderThicknessChanged);

            base.LoadComplete();
        }

        private Drawable selectSounds() => Clickable ? new HoverClickSounds() : new HoverSounds();

        private void OnborderThicknessChanged(ValueChangedEvent<float> v)
        {
            BorderThickness = v.NewValue;
        }

        //我已经不知道要怎么处理光标悬浮时的动画了就这样吧
        protected override bool OnHover(HoverEvent e)
        {
            if (AllowTransformBasicEffects.Value)
            {
                this.TransformBindableTo(ContentBorderThickness, 2);
                TweenEdgeEffectTo(EdgeEffectHover, ANIMATION_DURATION, ANIMATION_EASING);
            }

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            if (AllowTransformBasicEffects.Value)
            {
                this.TransformBindableTo(ContentBorderThickness, 0);
                TweenEdgeEffectTo(EdgeEffectNormal, ANIMATION_DURATION, ANIMATION_EASING);
            }

            base.OnHoverLost(e);
        }
    }
}
