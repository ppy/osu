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
        public Drawable D;

        private Container content;
        private SpriteIcon dropDownIcon;
        private BindableBool ToggleValue = new BindableBool();


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

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            ToggleValue.Toggle();
            return base.OnMouseDown(e);
        }

        private void OnToggleValueChanged(ValueChangedEvent<bool> value)
        {
            var v = value.NewValue;
            switch ( v )
            {
                case true:
                    dropDownIcon.RotateTo(0, 500, Easing.OutQuint);
                    content.FadeIn(500, Easing.OutQuint);
                    break;

                case false:
                    dropDownIcon.RotateTo(180, 500, Easing.OutQuint);
                    content.FadeOut(500, Easing.OutQuint);
                    break;
            }
        }
    }
}