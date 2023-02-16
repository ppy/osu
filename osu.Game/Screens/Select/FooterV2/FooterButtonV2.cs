// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
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
using osuTK.Graphics;

namespace osu.Game.Screens.Select.FooterV2
{
    public partial class FooterButtonV2 : OsuClickableContainer, IKeyBindingHandler<GlobalAction>
    {
        private const int button_height = 90;
        private const int button_width = 140;
        private const int corner_radius = 10;
        private const int transition_length = 500;

        // This should be 12 by design, but an extra allowance is added due to the corner radius specification.
        public const float SHEAR_WIDTH = 13.5f;

        public Bindable<Visibility> OverlayState = new Bindable<Visibility>();

        protected static readonly Vector2 SHEAR = new Vector2(SHEAR_WIDTH / button_height, 0);

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private Colour4 buttonAccentColour;

        protected Colour4 AccentColour
        {
            set
            {
                buttonAccentColour = value;
                bar.Colour = buttonAccentColour;
                icon.Colour = buttonAccentColour;
            }
        }

        protected IconUsage Icon
        {
            set => icon.Icon = value;
        }

        protected LocalisableString Text
        {
            set => text.Text = value;
        }

        private readonly SpriteText text;
        private readonly SpriteIcon icon;

        protected Container TextContainer;
        private readonly Box bar;
        private readonly Box backgroundBox;

        public FooterButtonV2()
        {
            EdgeEffect = new EdgeEffectParameters
            {
                Type = EdgeEffectType.Shadow,
                Radius = 4,
                // Figma says 50% opacity, but it does not match up visually if taken at face value, and looks bad.
                Colour = Colour4.Black.Opacity(0.25f),
                Offset = new Vector2(0, 2),
            };
            Shear = SHEAR;
            Size = new Vector2(button_width, button_height);
            Masking = true;
            CornerRadius = corner_radius;
            Children = new Drawable[]
            {
                backgroundBox = new Box
                {
                    RelativeSizeAxes = Axes.Both
                },

                // For elements that should not be sheared.
                new Container
                {
                    Anchor = Anchor.CentreLeft,
                    Origin = Anchor.CentreLeft,
                    Shear = -SHEAR,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        TextContainer = new Container
                        {
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre,
                            Y = 42,
                            AutoSizeAxes = Axes.Both,
                            Child = text = new OsuSpriteText
                            {
                                // figma design says the size is 16, but due to the issues with font sizes 19 matches better
                                Font = OsuFont.TorusAlternate.With(size: 19),
                                AlwaysPresent = true
                            }
                        },
                        icon = new SpriteIcon
                        {
                            Y = 12,
                            Size = new Vector2(20),
                            Anchor = Anchor.TopCentre,
                            Origin = Anchor.TopCentre
                        },
                    }
                },
                new Container
                {
                    Shear = -SHEAR,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.Centre,
                    Y = -corner_radius,
                    Size = new Vector2(120, 6),
                    Masking = true,
                    CornerRadius = 3,
                    Child = bar = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            OverlayState.BindValueChanged(_ => updateDisplay());
            Enabled.BindValueChanged(_ => updateDisplay(), true);

            FinishTransforms(true);
        }

        public GlobalAction? Hotkey;

        private bool handlingMouse;

        protected override bool OnHover(HoverEvent e)
        {
            updateDisplay();
            return true;
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            handlingMouse = true;
            updateDisplay();
            return base.OnMouseDown(e);
        }

        protected override void OnMouseUp(MouseUpEvent e)
        {
            handlingMouse = false;
            updateDisplay();
            base.OnMouseUp(e);
        }

        protected override void OnHoverLost(HoverLostEvent e) => updateDisplay();

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action != Hotkey || e.Repeat) return false;

            TriggerClick();
            return true;
        }

        public virtual void OnReleased(KeyBindingReleaseEvent<GlobalAction> e) { }

        private void updateDisplay()
        {
            Color4 backgroundColour = colourProvider.Background3;

            if (!Enabled.Value)
            {
                backgroundColour = colourProvider.Background3.Darken(0.4f);
            }
            else
            {
                if (OverlayState.Value == Visibility.Visible)
                    backgroundColour = buttonAccentColour.Darken(0.5f);

                if (IsHovered)
                {
                    backgroundColour = backgroundColour.Lighten(0.3f);

                    if (handlingMouse)
                        backgroundColour = backgroundColour.Lighten(0.3f);
                }
            }

            backgroundBox.FadeColour(backgroundColour, transition_length, Easing.OutQuint);
        }
    }
}
