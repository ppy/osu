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
        // This should be 12 by design, but an extra allowance is added due to the corner radius specification.
        private const float shear_width = 13.5f;

        protected const int CORNER_RADIUS = 10;
        protected const int BUTTON_HEIGHT = 90;
        protected const int BUTTON_WIDTH = 140;

        public Bindable<Visibility> OverlayState = new Bindable<Visibility>();

        protected static readonly Vector2 BUTTON_SHEAR = new Vector2(shear_width / BUTTON_HEIGHT, 0);

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
        private readonly Box flashLayer;

        public FooterButtonV2()
        {
            Size = new Vector2(BUTTON_WIDTH, BUTTON_HEIGHT);

            Child = new Container
            {
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Radius = 4,
                    // Figma says 50% opacity, but it does not match up visually if taken at face value, and looks bad.
                    Colour = Colour4.Black.Opacity(0.25f),
                    Offset = new Vector2(0, 2),
                },
                Shear = BUTTON_SHEAR,
                Masking = true,
                CornerRadius = CORNER_RADIUS,
                RelativeSizeAxes = Axes.Both,
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
                        Shear = -BUTTON_SHEAR,
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
                        Shear = -BUTTON_SHEAR,
                        Anchor = Anchor.BottomCentre,
                        Origin = Anchor.Centre,
                        Y = -CORNER_RADIUS,
                        Size = new Vector2(120, 6),
                        Masking = true,
                        CornerRadius = 3,
                        Child = bar = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                        }
                    },
                    flashLayer = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Colour4.White.Opacity(0.9f),
                        Blending = BlendingParameters.Additive,
                        Alpha = 0,
                    },
                },
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

        protected override bool OnClick(ClickEvent e)
        {
            if (Enabled.Value)
                Flash();

            return base.OnClick(e);
        }

        protected virtual void Flash() => flashLayer.FadeOutFromOne(800, Easing.OutQuint);

        protected override bool OnHover(HoverEvent e)
        {
            updateDisplay();
            return true;
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
            Color4 backgroundColour = OverlayState.Value == Visibility.Visible ? buttonAccentColour : colourProvider.Background3;
            Color4 textColour = OverlayState.Value == Visibility.Visible ? colourProvider.Background6 : colourProvider.Content1;
            Color4 accentColour = OverlayState.Value == Visibility.Visible ? colourProvider.Background6 : buttonAccentColour;

            if (!Enabled.Value)
                backgroundColour = backgroundColour.Darken(1f);
            else if (IsHovered)
                backgroundColour = backgroundColour.Lighten(0.2f);

            backgroundBox.FadeColour(backgroundColour, 150, Easing.OutQuint);

            if (!Enabled.Value)
                textColour = textColour.Opacity(0.6f);

            text.FadeColour(textColour, 150, Easing.OutQuint);
            icon.FadeColour(accentColour, 150, Easing.OutQuint);
            bar.FadeColour(accentColour, 150, Easing.OutQuint);
        }
    }
}
