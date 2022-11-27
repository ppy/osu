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

        protected FillFlowContainer ModsContainer;
        protected Container TextContainer;

        private readonly SpriteText spriteText;
        private readonly SpriteIcon sprite;

        private readonly Box backgroundColourBox;
        private readonly Box boxColour;
        private readonly Box flashLayer;

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
                        flashLayer = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Colour4.White.Opacity(0.9f),
                            Blending = BlendingParameters.Additive,
                            Alpha = 0
                        }
                    }
                },
                //Elements that cant be sheared
                new Container
                {
                    Shear = -SHEAR,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        ModsContainer = new FillFlowContainer
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Direction = FillDirection.Horizontal,
                            Y = -40
                        },
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
            flashLayer.FadeOutFromOne(800, Easing.OutQuint);

            return base.OnClick(e);
        }

        protected override bool OnHover(HoverEvent e)
        {
            backgroundColourBox.FadeColour(colourProvider.Background3.Lighten(.2f));
            Hovered?.Invoke();

            updateDisplay();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            backgroundColourBox.FadeColour(colourProvider.Background3, 500, Easing.OutQuint);
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

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
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

        public virtual void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
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
