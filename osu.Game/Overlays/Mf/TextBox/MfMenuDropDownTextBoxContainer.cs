using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuDropDownTextBoxContainer : MfMenuTextBoxContainer
    {
        private const float DURATION = 500;
        private const Easing EASING = Easing.OutQuint;
        private float UnExpandedBarWidth;

        public Drawable D;

        private Container content;
        private FillFlowContainer drawableContentContainer;
        private Circle dropDownBar;
        private BindableBool IsExpanded = new BindableBool();
        private BindableFloat ContentWidth = new BindableFloat();

        protected override bool Clickable => true;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = Colour4.White.Opacity(0.35f),
                Radius = 18,
            };

            drawableContentContainer = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Masking = true,
                Spacing = new Vector2(15),
                Children = new Drawable[]
                {
                    new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        Height = 7,
                        Child = dropDownBar = new Circle
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        },
                    },
                    content = new Container
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Masking = true,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y
                    }
                }
            };

            if ( D != null )
                content.Add(D);

            if ( d != null )
                throw new InvalidOperationException("\"d\" should not be used here, use \"D\" instead");

            d = drawableContentContainer;

            IsExpanded.Value = false;
            IsExpanded.BindValueChanged(OnIsExpandedChanged, true);

            ContentWidth.BindValueChanged(OnContentWidthChanged, true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            IsExpanded.Toggle();
            return base.OnClick(e);
        }

        private void OnIsExpandedChanged(ValueChangedEvent<bool> value)
        {
            var v = value.NewValue;
            switch ( v )
            {
                //点击时
                case true:
                    CanChangeBorderThickness.Value = false;
                    this.TransformBindableTo(borderThickness, 4, DURATION, EASING);
                    dropDownBar.ResizeTo(new Vector2(drawableContentContainer.DrawWidth, 3), DURATION, Easing.OutQuint);
                    FadeEdgeEffectTo(0.35f, DURATION, EASING);
                    content.FadeIn(DURATION, Easing.OutQuint);
                    break;

                //其他情况
                case false:
                    CanChangeBorderThickness.Value = true;

                    if ( backgroundContainer.BorderThickness != 0 )
                        this.TransformBindableTo(borderThickness, 2, DURATION, EASING);

                    dropDownBar.ResizeTo(new Vector2(ContentWidth.Value * 0.1f, 7), DURATION, Easing.OutQuint);
                    FadeEdgeEffectTo(0, DURATION, EASING);
                    content.FadeOut(DURATION, Easing.OutQuint);
                    break;
            }
        }

        private void OnContentWidthChanged(ValueChangedEvent<float> w)
        {
            UnExpandedBarWidth = w.NewValue * 0.1f;

            if ( IsExpanded.Value )
                dropDownBar.ResizeWidthTo(w.NewValue);
            else
                dropDownBar.ResizeWidthTo(UnExpandedBarWidth);
        }

        protected override bool OnHover(HoverEvent e)
        {
            UnExpandedBarWidth *= 1.5f;

            if ( !IsExpanded.Value )
                dropDownBar.ResizeWidthTo(UnExpandedBarWidth, 500, Easing.OutElastic);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            UnExpandedBarWidth = ContentWidth.Value * 0.1f;

            if ( !IsExpanded.Value )
                dropDownBar.ResizeWidthTo(UnExpandedBarWidth, 500, Easing.OutQuint);

            base.OnHoverLost(e);
        }

        protected override void Update()
        {
            ContentWidth.Value = drawableContentContainer?.DrawWidth ?? this.DrawWidth;
            base.Update();
        }
    }
}