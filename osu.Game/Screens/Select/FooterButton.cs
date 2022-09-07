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
        public const float SHEAR_WIDTH = 16;
        private const float outer_corner_radius = 10;
        private const int button_height = 120;
        private const float shear_padding = 10;

        private readonly Colour4 backgroundColour = Colour4.FromHex("#394642");

        protected static readonly Vector2 SHEAR = new Vector2(SHEAR_WIDTH / button_height, 0);

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

        protected readonly Container TextContainer;
        private readonly SpriteText spriteText;
        private readonly Box boxColour;
        private readonly Box flashLayer;
        private readonly SpriteIcon sprite;
        private readonly Box backgroundColourBox;
        protected FillFlowContainer ButtonContentContainer;

        protected FooterButton()
        {
            Anchor = Anchor.TopLeft;
            AutoSizeAxes = Axes.X;
            Height = button_height;
            Shear = SHEAR;
            CornerRadius = outer_corner_radius;
            Masking = true;
            Margin = new MarginPadding { Top = -20 };
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Offset = new Vector2(.250f, 2),
                Colour = Colour4.Black.Opacity(.25f),
                Radius = outer_corner_radius
            };

            {
                Children = new Drawable[]
                {
                    backgroundColourBox = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = backgroundColour,
                    },
                    new Container
                    {
                        Shear = -SHEAR,
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        AutoSizeAxes = Axes.Both,
                        X = -10,
                        Children = new Drawable[]
                        {
                            sprite = new SpriteIcon
                            {
                                Margin = new MarginPadding
                                {
                                    Top = 12,
                                },
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Size = new Vector2(20)
                            },
                            ButtonContentContainer = new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Margin = new MarginPadding { Top = 42 },
                                Children = new Drawable[]
                                {
                                    TextContainer = new Container
                                    {
                                        Colour = Colour4.White,
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,

                                        AutoSizeAxes = Axes.Both,
                                        Child = spriteText = new OsuSpriteText
                                        {
                                            Font = OsuFont.TorusAlternate.With(size: 16),
                                            AlwaysPresent = true,
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                        }
                                    }
                                }
                            },
                            new Container
                            {
                                Masking = true,
                                CornerRadius = 3,
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AutoSizeAxes = Axes.Both,
                                Margin = new MarginPadding { Top = 77 },
                                Children = new Drawable[]
                                {
                                    boxColour = new Box
                                    {
                                        Height = 6,
                                        Width = 120,
                                        Origin = Anchor.Centre,
                                        Anchor = Anchor.Centre,
                                    }
                                }
                            },
                        }
                    },
                    flashLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.White.Opacity(0.9f),
                        Blending = BlendingParameters.Additive,
                        Alpha = 0
                    },
                };
            }
        }

        protected Action Hovered;
        protected Action HoverLost;
        protected GlobalAction? Hotkey;

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

            TriggerClick();
            return true;
        }

        public virtual void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
        }
    }
}
