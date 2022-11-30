// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using osu.Framework.Allocation;
using osu.Framework.Extensions.Color4Extensions;
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
using osu.Game.Overlays;
using osuTK;

namespace osu.Game.Screens.Select
{
    public partial class FooterButton : OsuClickableContainer, IKeyBindingHandler<GlobalAction>
    {
        private const int outer_corner_radius = 10;
        private const int button_height = 120;
        private const int button_width = 140;

        public const float SHEAR_WIDTH = 16;

        protected static readonly Vector2 SHEAR = new Vector2(SHEAR_WIDTH / button_height, 0);

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Aquamarine);

        protected LocalisableString Text
        {
            set
            {
                if (spriteText != null)
                    spriteText.Text = value;
            }
        }

        protected Colour4 ButtonAccentColour
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

        protected Container TextContainer;

        private readonly SpriteText spriteText;
        private readonly SpriteIcon sprite;

        private readonly Box backgroundColourBox;
        private readonly Box boxColour;

        protected FooterButton()
        {
            Shear = SHEAR;
            Width = button_width;
            Height = button_height;
            Children = new Drawable[]
            {
                //This container is needed for masking elements without hiding the mod display.
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = outer_corner_radius,
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Offset = new Vector2(.250f, 2),
                        Colour = Colour4.Black.Opacity(.25f),
                        Radius = outer_corner_radius
                    },
                    Children = new Drawable[]
                    {
                        backgroundColourBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = colourProvider.Background3
                        },
                    }
                },
                //Elements that cant be sheared
                new Container
                {
                    Shear = -SHEAR,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        sprite = new SpriteIcon
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Position = new Vector2(-SHEAR_WIDTH * (42f / button_height), 12),
                            Size = new Vector2(20)
                        },
                        TextContainer = new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Position = new Vector2(-SHEAR_WIDTH * (42f / button_height), 42),
                            AutoSizeAxes = Axes.Both,
                            Child = spriteText = new OsuSpriteText
                            {
                                Font = OsuFont.TorusAlternate.With(size: 16),
                                AlwaysPresent = true
                            }
                        },
                        new Container
                        {
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.Centre,
                            Position = new Vector2(-SHEAR_WIDTH * (80f / button_height), -40),
                            Height = 6,
                            CornerRadius = 3,
                            Masking = true,
                            RelativeSizeAxes = Axes.X,
                            Width = 120f / button_width,
                            Child = boxColour = new Box
                            {
                                RelativeSizeAxes = Axes.Both
                            }
                        }
                    }
                }
            };
        }

        public Action Hovered;
        public Action HoverLost;
        public GlobalAction? Hotkey;

        protected override void UpdateAfterChildren()
        {
        }

        protected override bool OnHover(HoverEvent e)
        {
            backgroundColourBox.FadeColour(colourProvider.Background3.Lighten(.2f));
            Hovered?.Invoke();

            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            backgroundColourBox.FadeColour(colourProvider.Background3, 500, Easing.OutQuint);
            HoverLost?.Invoke();
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (!Enabled.Value)
                return true;

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
    }
}
