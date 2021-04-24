using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;

namespace osu.Game.Overlays.Mf.TextBox
{
    public class MfMenuDropDownTextBoxContainer : MfMenuTextBoxContainer
    {
        private readonly BindableBool isExpanded = new BindableBool();
        protected override bool Clickable => true;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            isExpanded.Value = false;
            isExpanded.BindValueChanged(OnIsExpandedChanged, true);
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
                    this.TransformBindableTo(ContentBorderThickness, 4, ANIMATION_DURATION, ANIMATION_EASING);
                    Content.FadeIn(ANIMATION_DURATION, Easing.OutQuint);
                    break;

                //其他情况
                case false:
                    AllowTransformBasicEffects.Value = true;

                    if (BorderThickness != 0)
                        this.TransformBindableTo(ContentBorderThickness, 2, ANIMATION_DURATION, ANIMATION_EASING);

                    Content.FadeOut(ANIMATION_DURATION, Easing.OutQuint);

                    TweenEdgeEffectTo(IsHovered
                        ? EdgeEffectHover
                        : EdgeEffectNormal, ANIMATION_DURATION, ANIMATION_EASING);

                    break;
            }
        }
    }
}
