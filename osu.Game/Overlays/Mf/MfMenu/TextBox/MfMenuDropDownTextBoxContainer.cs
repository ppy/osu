using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osuTK;

namespace osu.Game.Overlays.MfMenu
{
    public class MfMenuDropDownTextBoxContainer : MfMenuTextBoxContainer
    {
        private const float DURATION = 500;

        public Drawable D;

        private Container content;
        private SpriteIcon dropDownIcon;
        private BindableBool ToggleValue = new BindableBool();

        protected override bool Clickable => true;

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;

            var drawableContent = new FillFlowContainer
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
                        Size = new Vector2(15),
                        Child = dropDownIcon = new SpriteIcon
                        {
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                            Size = new Vector2(15),
                            Icon = FontAwesome.Solid.AngleDown
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

            content.Add(D);

            if ( d != null )
                throw new InvalidOperationException("\"d\" should not be used here, use \"D\" instead");

            d = drawableContent;

            ToggleValue.Value = false;
            ToggleValue.BindValueChanged(OnToggleValueChanged, true);
        }

        protected override bool OnClick(ClickEvent e)
        {
            ToggleValue.Toggle();
            return base.OnClick(e);
        }

        private void OnToggleValueChanged(ValueChangedEvent<bool> value)
        {
            var v = value.NewValue;
            switch ( v )
            {
                case true:
                    CanChangeBorderThickness.Value = false;
                    backgroundContainer.BorderThickness = 4;
                    dropDownIcon.RotateTo(0, DURATION, Easing.OutQuint);
                    content.FadeIn(DURATION, Easing.OutQuint);
                    break;

                case false:
                    CanChangeBorderThickness.Value = true;

                    if ( backgroundContainer.BorderThickness != 0 )
                        backgroundContainer.BorderThickness = 2;

                    dropDownIcon.RotateTo(180, DURATION, Easing.OutQuint);
                    content.FadeOut(DURATION, Easing.OutQuint);
                    break;
            }
        }
    }
}