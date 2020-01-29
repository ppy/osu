// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osuTK.Graphics;
using osuTK.Input;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.Containers;

namespace osu.Game.Screens.Select
{
    public class FooterButton : OsuClickableContainer
    {
        public const float SHEAR_WIDTH = 7.5f;

        protected static readonly Vector2 SHEAR = new Vector2(SHEAR_WIDTH / Footer.HEIGHT, 0);

        public string Text
        {
            get => SpriteText?.Text;
            set
            {
                if (SpriteText != null)
                    SpriteText.Text = value;
            }
        }

        private Color4 deselectedColour;

        public Color4 DeselectedColour
        {
            get => deselectedColour;
            set
            {
                deselectedColour = value;
                if (light.Colour != SelectedColour)
                    light.Colour = value;
            }
        }

        private Color4 selectedColour;

        public Color4 SelectedColour
        {
            get => selectedColour;
            set
            {
                selectedColour = value;
                box.Colour = selectedColour;
            }
        }

        protected readonly Container TextContainer;
        protected readonly Container LightContainer;
        protected readonly SpriteText SpriteText;
        private readonly Box box;
        private readonly Box light;

        public FooterButton()
        {
            AutoSizeAxes = Axes.Both;
            Shear = SHEAR;
            // Masks away one or two overlapping pixel
            // when UI scale is at minimum.
            Masking = true;
            Children = new Drawable[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    EdgeSmoothness = new Vector2(2, 0),
                    Colour = Color4.White,
                    Alpha = 0,
                },
                LightContainer = new Container
                {
                    AutoSizeAxes = Axes.Y,
                    RelativeSizeAxes = Axes.X,
                    // Make container for light slightly smaller than parent.
                    Width = 0.996f,
                    // Anchor is at top right because ModIcons are added to the right
                    // so rounding errors (relative size of x) are hidden to the left.
                    Anchor = Anchor.TopRight,
                    Origin = Anchor.TopRight,
                    Child = light = new Box
                    {
                        Height = 4,
                        Width = 0.998f,
                        EdgeSmoothness = new Vector2(2, 0),
                        RelativeSizeAxes = Axes.X,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                    }
                },
                TextContainer = new Container
                {
                    Size = new Vector2(100 - SHEAR_WIDTH, 50),
                    Shear = -SHEAR,
                    Child = SpriteText = new OsuSpriteText
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                    }
                },
            };
        }

        public Action Hovered;
        public Action HoverLost;
        public Key? Hotkey;

        protected override bool OnHover(HoverEvent e)
        {
            Hovered?.Invoke();
            light.ScaleTo(new Vector2(1, 2), Footer.TRANSITION_LENGTH, Easing.OutQuint);
            light.FadeColour(SelectedColour, Footer.TRANSITION_LENGTH, Easing.OutQuint);
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            HoverLost?.Invoke();
            light.ScaleTo(new Vector2(1, 1), Footer.TRANSITION_LENGTH, Easing.OutQuint);
            light.FadeColour(DeselectedColour, Footer.TRANSITION_LENGTH, Easing.OutQuint);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            box.FadeTo(0.3f, Footer.TRANSITION_LENGTH * 2, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            box.FadeOut(Footer.TRANSITION_LENGTH, Easing.OutQuint);
            base.OnMouseUp(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            box.ClearTransforms();
            box.Alpha = 1;
            box.FadeOut(Footer.TRANSITION_LENGTH * 3, Easing.OutQuint);
            return base.OnClick(e);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!e.Repeat && e.Key == Hotkey)
            {
                Click();
                return true;
            }

            return base.OnKeyDown(e);
        }
    }
}
