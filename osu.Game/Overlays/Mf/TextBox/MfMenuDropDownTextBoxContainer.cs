using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuDropDownTextBoxContainer : MfMenuTextBoxContainer
    {
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
                    AllowTransformBasicEffects.Value = false;
                    this.TransformBindableTo(borderThickness, 4, DURATION, EASING);
                    dropDownBar.ResizeTo(new Vector2(drawableContentContainer.DrawWidth, 3), DURATION, Easing.OutQuint);
                    content.FadeIn(DURATION, Easing.OutQuint);
                    break;

                //其他情况
                case false:
                    AllowTransformBasicEffects.Value = true;

                    if ( this.BorderThickness != 0 )
                        this.TransformBindableTo(borderThickness, 2, DURATION, EASING);

                    dropDownBar.ResizeTo(new Vector2(ContentWidth.Value * 0.1f, 7), DURATION, Easing.OutQuint);
                    content.FadeOut(DURATION, Easing.OutQuint);

                    if ( IsHovered )
                        this.TweenEdgeEffectTo(edgeEffectHover, DURATION, EASING);
                    else
                        this.TweenEdgeEffectTo(edgeEffectNormal, DURATION, EASING);

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