using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Overlays.Mf.TextBox
{
    public class MfMenuDropDownTextBoxContainer : MfMenuTextBoxContainer
    {
        private float unExpandedBarWidth;

        public Drawable D;

        private Container content;
        private FillFlowContainer drawableContentContainer;
        private Circle dropDownBar;
        private readonly BindableBool isExpanded = new BindableBool();
        private readonly BindableFloat contentWidth = new BindableFloat();
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

            if (D != null)
                content.Add(D);

            if (d != null)
                throw new InvalidOperationException("\"d\" should not be used here, use \"D\" instead");

            d = drawableContentContainer;

            isExpanded.Value = false;
            isExpanded.BindValueChanged(OnIsExpandedChanged, true);

            contentWidth.BindValueChanged(OnContentWidthChanged, true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            isExpanded.Toggle();
            return base.OnClick(e);
        }

        private void OnIsExpandedChanged(ValueChangedEvent<bool> value)
        {
            var v = value.NewValue;

            switch (v)
            {
                //点击时
                case true:
                    AllowTransformBasicEffects.Value = false;
                    this.TransformBindableTo(borderThickness, 4, Duration, Easing);
                    dropDownBar.ResizeTo(new Vector2(drawableContentContainer.DrawWidth, 3), Duration, Easing.OutQuint);
                    content.FadeIn(Duration, Easing.OutQuint);
                    break;

                //其他情况
                case false:
                    AllowTransformBasicEffects.Value = true;

                    if (BorderThickness != 0)
                        this.TransformBindableTo(borderThickness, 2, Duration, Easing);

                    dropDownBar.ResizeTo(new Vector2(contentWidth.Value * 0.1f, 7), Duration, Easing.OutQuint);
                    content.FadeOut(Duration, Easing.OutQuint);

                    TweenEdgeEffectTo(IsHovered
                        ? EdgeEffectHover
                        : EdgeEffectNormal, Duration, Easing);

                    break;
            }
        }

        private void OnContentWidthChanged(ValueChangedEvent<float> w)
        {
            unExpandedBarWidth = w.NewValue * 0.1f;

            dropDownBar.ResizeWidthTo(isExpanded.Value
                ? w.NewValue
                : unExpandedBarWidth);
        }

        protected override bool OnHover(HoverEvent e)
        {
            unExpandedBarWidth *= 1.5f;

            if (!isExpanded.Value)
                dropDownBar.ResizeWidthTo(unExpandedBarWidth, 500, Easing.OutElastic);

            return base.OnHover(e);
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            unExpandedBarWidth = contentWidth.Value * 0.1f;

            if (!isExpanded.Value)
                dropDownBar.ResizeWidthTo(unExpandedBarWidth, 500, Easing.OutQuint);

            base.OnHoverLost(e);
        }

        protected override void Update()
        {
            contentWidth.Value = drawableContentContainer?.DrawWidth ?? this.DrawWidth;
            base.Update();
        }
    }
}
