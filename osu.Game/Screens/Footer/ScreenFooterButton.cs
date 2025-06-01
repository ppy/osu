// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
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

namespace osu.Game.Screens.Footer
{
    public partial class ScreenFooterButton : OsuClickableContainer, IKeyBindingHandler<GlobalAction>
    {
        public const int Y_OFFSET = 10;

        protected const int BUTTON_HEIGHT = 75;
        protected const int BUTTON_WIDTH = 116;

        public Bindable<Visibility> OverlayState = new Bindable<Visibility>();

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        private Colour4 buttonAccentColour;

        public Colour4 AccentColour
        {
            set
            {
                buttonAccentColour = value;
                bar.Colour = buttonAccentColour;
                icon.Colour = buttonAccentColour;
            }
        }

        public IconUsage Icon
        {
            set => icon.Icon = value;
        }

        public LocalisableString Text
        {
            get => text.Text;
            set => text.Text = value;
        }

        private readonly SpriteText text;
        private readonly SpriteIcon icon;

        protected Container TextContainer;
        private readonly Box bar;
        private readonly Box backgroundBox;
        private readonly Box glowBox;
        private readonly Box flashLayer;

        public readonly OverlayContainer? Overlay;

        public ScreenFooterButton(OverlayContainer? overlay = null)
        {
            Overlay = overlay;

            Size = new Vector2(BUTTON_WIDTH, BUTTON_HEIGHT);

            Children = new Drawable[]
            {
                new Container
                {
                    EdgeEffect = new EdgeEffectParameters
                    {
                        Type = EdgeEffectType.Shadow,
                        Radius = 4,
                        // Figma says 50% opacity, but it does not match up visually if taken at face value, and looks bad.
                        Colour = Colour4.Black.Opacity(0.25f),
                        Offset = new Vector2(0, 2),
                    },
                    Shear = OsuGame.SHEAR,
                    Masking = true,
                    CornerRadius = 10,
                    RelativeSizeAxes = Axes.Both,
                    Children = new Drawable[]
                    {
                        backgroundBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        glowBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both
                        },
                        // For elements that should not be sheared.
                        new Container
                        {
                            Anchor = Anchor.CentreLeft,
                            Origin = Anchor.CentreLeft,
                            Shear = -OsuGame.SHEAR,
                            RelativeSizeAxes = Axes.Both,
                            Children = new Drawable[]
                            {
                                TextContainer = new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Y = 35,
                                    AutoSizeAxes = Axes.Both,
                                    Child = text = new OsuSpriteText
                                    {
                                        Font = OsuFont.TorusAlternate.With(size: 16),
                                        AlwaysPresent = true
                                    }
                                },
                                icon = new SpriteIcon
                                {
                                    Y = 10,
                                    Size = new Vector2(16),
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre
                                },
                            }
                        },
                        new Container
                        {
                            Shear = -OsuGame.SHEAR,
                            Anchor = Anchor.BottomCentre,
                            Origin = Anchor.Centre,
                            Y = -Y_OFFSET,
                            Size = new Vector2(100, 5),
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
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Overlay != null)
                OverlayState.BindTo(Overlay.State);

            OverlayState.BindValueChanged(_ => UpdateDisplay());
            Enabled.BindValueChanged(_ => UpdateDisplay(), true);

            FinishTransforms(true);
        }

        // use Content for tracking input as some buttons might be temporarily hidden with DisappearToBottom, and they become hidden by moving Content away from screen.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) => Content.ReceivePositionalInputAt(screenSpacePos);

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
            UpdateDisplay();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e) => UpdateDisplay();

        public virtual bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action != Hotkey || e.Repeat) return false;

            TriggerClick();
            return true;
        }

        public virtual void OnReleased(KeyBindingReleaseEvent<GlobalAction> e) { }

        public void UpdateDisplay()
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

            glowBox.FadeColour(ColourInfo.GradientVertical(buttonAccentColour.Opacity(0f), buttonAccentColour.Opacity(0.2f)), 150, Easing.OutQuint);
        }

        public void AppearFromLeft(double delay)
        {
            Content.FinishTransforms();
            Content.MoveToX(-300f)
                   .FadeOut()
                   .Delay(delay)
                   .MoveToX(0f, 240, Easing.OutCubic)
                   .FadeIn(240, Easing.OutCubic);
        }

        public void AppearFromBottom(double delay)
        {
            Content.FinishTransforms();
            Content.MoveToY(100f)
                   .FadeOut()
                   .Delay(delay)
                   .MoveToY(0f, 240, Easing.OutCubic)
                   .FadeIn(240, Easing.OutCubic);
        }

        public void DisappearToRight(double delay, bool expire)
        {
            Content.FinishTransforms();
            Content.Delay(delay)
                   .FadeOut(240, Easing.InOutCubic)
                   .MoveToX(300f, 360, Easing.InOutCubic);

            if (expire)
                this.Delay(Content.LatestTransformEndTime - Time.Current).Expire();
        }

        public void DisappearToBottom(double delay, bool expire)
        {
            Content.FinishTransforms();
            Content.Delay(delay)
                   .FadeOut(240, Easing.InOutCubic)
                   .MoveToY(100f, 240, Easing.InOutCubic);

            if (expire)
                this.Delay(Content.LatestTransformEndTime - Time.Current).Expire();
        }
    }
}
