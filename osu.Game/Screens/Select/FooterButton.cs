// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
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

        private Colour4 deselectedColour;

        public Colour4 DeselectedColour
        {
            get => deselectedColour;
            set
            {
                deselectedColour = value;
                if (light.Colour != SelectedColour)
                    light.Colour = value;
            }
        }

        private Colour4 selectedColour;

        public Colour4 SelectedColour
        {
            get => selectedColour;
            set
            {
                selectedColour = value;
                box.Colour = selectedColour;
            }
        }

        protected FillFlowContainer ButtonContentContainer;
        protected readonly Container TextContainer;
        protected readonly SpriteText SpriteText;
        private readonly Box box;
        private readonly Box light;

        public FooterButton()
        {
            AutoSizeAxes = Axes.Both;
            Shear = SHEAR;
            Children = new Drawable[]
            {
                box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    EdgeSmoothness = new Vector2(2, 0),
                    Colour = Colour4.White,
                    Alpha = 0,
                },
                light = new Box
                {
                    Height = 4,
                    EdgeSmoothness = new Vector2(2, 0),
                    RelativeSizeAxes = Axes.X,
                },
                new Container
                {
                    AutoSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        ButtonContentContainer = new FillFlowContainer
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Direction = FillDirection.Horizontal,
                            Shear = -SHEAR,
                            AutoSizeAxes = Axes.X,
                            Height = 50,
                            Spacing = new Vector2(15, 0),
                            Children = new Drawable[]
                            {
                                TextContainer = new Container
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    AutoSizeAxes = Axes.Both,
                                    Child = SpriteText = new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                    }
                                },
                            },
                        },
                    },
                },
            };
        }

        public Action Hovered;
        public Action HoverLost;
        public Key? Hotkey;

        protected override void UpdateAfterChildren()
        {
            base.UpdateAfterChildren();

            float horizontalMargin = (100 - TextContainer.Width) / 2;
            ButtonContentContainer.Padding = new MarginPadding
            {
                Left = horizontalMargin,
                // right side margin offset to compensate for shear
                Right = horizontalMargin - SHEAR_WIDTH / 2
            };
        }

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
