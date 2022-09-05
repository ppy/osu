// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Input.Bindings;
using osuTK;

namespace osu.Game.Screens.Select
{
    public abstract class FooterButton : OsuClickableContainer, IKeyBindingHandler<GlobalAction>
    {
        public const float SHEAR_WIDTH = 7.5f;
        private const float outer_corner_radius = 7;
        private const int ease_out_time = 800;
        protected readonly FontUsage TorusFont = OsuFont.TorusAlternate.With(size: 20);

        private readonly Colour4 backgroundColour = Colour4.FromHex("#394642");

        protected static readonly Vector2 SHEAR = new Vector2(SHEAR_WIDTH / Footer.HEIGHT, 0);

        protected LocalisableString Text
        {
            set
            {
                if (spriteText != null)
                    spriteText.Text = value;
            }
        }

        protected Colour4 ButtonColour
        {
            set
            {
                boxColour.Colour = value;
                sprite.Colour = value;
            }
        }

        protected IconUsage IconUsageBox
        {
            set => sprite.Icon = value;
        }

        protected FillFlowContainer ButtonContentContainer;
        protected readonly Container TextContainer;
        private readonly SpriteText spriteText;
        private readonly Box boxColour;
        private readonly Box flashLayer;
        private readonly SpriteIcon sprite;
        private readonly Box backgroundColourBox;

        protected FooterButton()
        {
            Anchor = Anchor.BottomLeft;
            AutoSizeAxes = Axes.Both;
            Shear = SHEAR;
            CornerRadius = outer_corner_radius;
            Masking = true;
            Margin = new MarginPadding { Left = 2, Top = -20 };
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Colour = new Colour4(0, 0, 0, 50),
                Radius = outer_corner_radius
            };

            {
                Children = new Drawable[]
                {
                    backgroundColourBox = new Box
                    {
                        Colour = backgroundColour,
                        RelativeSizeAxes = Axes.X,
                        Depth = 2,
                        Height = 100
                    },
                    sprite = new SpriteIcon
                    {
                        Shear = -SHEAR,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        Size = new Vector2(20),
                        Margin = new MarginPadding
                        {
                            Top = 12,
                            Left = -10
                        }
                    },
                    ButtonContentContainer = new FillFlowContainer
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Direction = FillDirection.Horizontal,
                        AutoSizeAxes = Axes.X,
                        Children = new Drawable[]
                        {
                            TextContainer = new Container
                            {
                                Colour = Colour4.White,
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Shear = -SHEAR,
                                AutoSizeAxes = Axes.Both,
                                Padding = new MarginPadding { Top = -5 },
                                Child = spriteText = new OsuSpriteText
                                {
                                    Font = TorusFont,
                                    AlwaysPresent = true,
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                }
                            }
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.BottomCentre,
                        Height = 6,
                        Width = 130,
                        Margin = new MarginPadding
                        {
                            Left = 10,
                            Bottom = 27
                        },
                        Shear = -SHEAR,
                        CornerRadius = 3,
                        Masking = true,
                        Children = new Drawable[]
                        {
                            boxColour = new Box
                            {
                                Origin = Anchor.Centre,
                                Anchor = Anchor.Centre,
                                RelativeSizeAxes = Axes.Both,
                                Depth = 1,
                            }
                        }
                    },
                    flashLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.White.Opacity(0.9f),
                        Blending = BlendingParameters.Additive,
                        Alpha = 0,
                    },
                };
            }
        }

        protected Action Hovered;
        protected Action HoverLost;
        protected GlobalAction? Hotkey;

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

        protected override bool OnClick(ClickEvent e)
        {
            flashLayer.FadeOutFromOne(800, Easing.OutQuint);

            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            backgroundColourBox.FadeColour(backgroundColour.Lighten(.2f));
            Hovered?.Invoke();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            backgroundColourBox.FadeColour(backgroundColour, 500, Easing.OutQuint);
            HoverLost?.Invoke();
        }

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action != Hotkey || e.Repeat) return false;

            this.ScaleTo(.9f, 2000, Easing.OutQuint);
            TriggerClick();
            return true;
        }

        public virtual void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            if (e.Action != Hotkey) return;

            this.ScaleTo(1, ease_out_time, Easing.OutBounce);
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            Content.ScaleTo(0.9f, 2000, Easing.OutQuint);
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            Content.ScaleTo(1, 1000, Easing.OutElastic);
            base.OnMouseUp(e);
        }
    }
}
