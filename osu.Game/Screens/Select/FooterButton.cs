// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.Select
{
    public partial class FooterButton : OsuClickableContainer, IKeyBindingHandler<GlobalAction>
    {
        public const float SHEAR_WIDTH = 7.5f;

        protected static readonly Vector2 SHEAR = new Vector2(SHEAR_WIDTH / Footer.HEIGHT, 0);

        public LocalisableString Text
        {
            get => SpriteText?.Text ?? default;
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
                    Colour = Color4.White,
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
                                        AlwaysPresent = true,
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

        protected override void LoadComplete()
        {
            base.LoadComplete();
            Enabled.BindValueChanged(_ => updateDisplay(), true);
        }

        public Action Hovered;
        public Action HoverLost;
        public GlobalAction? Hotkey;

        private bool mouseDown;

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
            updateDisplay();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            HoverLost?.Invoke();
            updateDisplay();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (!Enabled.Value)
                return true;

            mouseDown = true;
            updateDisplay();
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            mouseDown = false;
            updateDisplay();
            base.OnMouseUp(e);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!Enabled.Value)
                return true;

            box.ClearTransforms();
            box.Alpha = 1;
            box.FadeOut(Footer.TRANSITION_LENGTH * 3, Easing.OutQuint);
            return base.OnClick(e);
        }

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action == Hotkey && !e.Repeat)
            {
                TriggerClick();
                return true;
            }

            return false;
        }

        public virtual void OnReleased(KeyBindingReleaseEvent<GlobalAction> e) { }

        private void updateDisplay()
        {
            this.FadeTo(Enabled.Value ? 1 : 0.25f, Footer.TRANSITION_LENGTH, Easing.OutQuint);

            light.ScaleTo(Enabled.Value && IsHovered ? new Vector2(1, 2) : new Vector2(1), Footer.TRANSITION_LENGTH, Easing.OutQuint);
            light.FadeColour(Enabled.Value && IsHovered ? SelectedColour : DeselectedColour, Footer.TRANSITION_LENGTH, Easing.OutQuint);

            box.FadeTo(Enabled.Value & mouseDown ? 0.3f : 0f, Footer.TRANSITION_LENGTH * 2, Easing.OutQuint);

            if (Enabled.Value && IsHovered)
                Hovered?.Invoke();
            else
                HoverLost?.Invoke();
        }
    }
}
