// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public abstract partial class PanelBase : PoolableDrawable, ICarouselPanel
    {
        private const float corner_radius = 10;

        private const float active_x_offset = 50f;

        protected const float DURATION = 400;

        protected float PanelXOffset { get; init; }

        private Box backgroundBorder = null!;
        private Box backgroundGradient = null!;
        private Box backgroundAccentGradient = null!;
        private Container backgroundLayerHorizontalPadding = null!;
        private Container backgroundContainer = null!;
        private Container iconContainer = null!;
        private Box activationFlash = null!;
        private Box hoverLayer = null!;
        private Box keyboardSelectionLayer = null!;
        private Box selectionLayer = null!;

        public Container TopLevelContent { get; private set; } = null!;

        protected Container Content { get; private set; } = null!;

        public Drawable Background { set => backgroundContainer.Child = value; }

        public Drawable Icon { set => iconContainer.Child = value; }

        private Color4? accentColour;

        public Color4? AccentColour
        {
            get => accentColour;
            set
            {
                accentColour = value;
                updateDisplay();
            }
        }

        // content is offset by PanelXOffset, make sure we only handle input at the actual visible
        // offset region.
        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos) =>
            TopLevelContent.ReceivePositionalInputAt(screenSpacePos);

        [Resolved]
        private BeatmapCarousel? carousel { get; set; }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;

            RelativeSizeAxes = Axes.X;
            Height = CarouselItem.DEFAULT_HEIGHT;

            InternalChild = TopLevelContent = new Container
            {
                Masking = true,
                CornerRadius = corner_radius,
                RelativeSizeAxes = Axes.Both,
                X = corner_radius,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
                    Offset = new Vector2(1f),
                    Radius = 10,
                },
                Children = new Drawable[]
                {
                    new BufferedContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            backgroundBorder = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.White,
                            },
                            backgroundLayerHorizontalPadding = new Container
                            {
                                RelativeSizeAxes = Axes.Both,
                                Child = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Masking = true,
                                    CornerRadius = corner_radius,
                                    Children = new Drawable[]
                                    {
                                        backgroundGradient = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        backgroundAccentGradient = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                        backgroundContainer = new Container
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                        },
                                    }
                                },
                            }
                        },
                    },
                    iconContainer = new Container
                    {
                        Anchor = Anchor.CentreLeft,
                        Origin = Anchor.CentreLeft,
                        AutoSizeAxes = Axes.Both,
                    },
                    Content = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Right = PanelXOffset + corner_radius },
                    },
                    hoverLayer = new Box
                    {
                        Alpha = 0,
                        Colour = colours.Blue.Opacity(0.1f),
                        Blending = BlendingParameters.Additive,
                        RelativeSizeAxes = Axes.Both,
                    },
                    selectionLayer = new Box
                    {
                        Alpha = 0,
                        Colour = ColourInfo.GradientHorizontal(colours.Yellow.Opacity(0), colours.Yellow.Opacity(0.5f)),
                        Blending = BlendingParameters.Additive,
                        RelativeSizeAxes = Axes.Both,
                        Width = 0.7f,
                        Anchor = Anchor.TopRight,
                        Origin = Anchor.TopRight,
                    },
                    keyboardSelectionLayer = new Box
                    {
                        Alpha = 0,
                        Colour = colours.Yellow.Opacity(0.1f),
                        Blending = BlendingParameters.Additive,
                        RelativeSizeAxes = Axes.Both,
                    },
                    activationFlash = new Box
                    {
                        Colour = Color4.White.Opacity(0.4f),
                        Blending = BlendingParameters.Additive,
                        Alpha = 0f,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new HoverSounds(),
                }
            };

            backgroundGradient.Colour = ColourInfo.GradientHorizontal(colourProvider.Background3, colourProvider.Background4);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(_ => updateDisplay(), true);

            Selected.BindValueChanged(selected =>
            {
                if (selected.NewValue)
                    selectionLayer.FadeIn(100, Easing.OutQuint);
                else
                    selectionLayer.FadeOut(200, Easing.OutQuint);

                updateXOffset();
            }, true);

            KeyboardSelected.BindValueChanged(selected =>
            {
                if (selected.NewValue)
                    keyboardSelectionLayer.FadeIn(100, Easing.OutQuint);
                else
                    keyboardSelectionLayer.FadeOut(1000, Easing.OutQuint);

                updateXOffset();
            }, true);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();
            this.FadeInFromZero(DURATION, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            carousel?.Activate(Item!);
            return true;
        }

        private void updateDisplay()
        {
            var backgroundColour = accentColour ?? Color4.White;
            var edgeEffectColour = accentColour ?? Color4Extensions.FromHex(@"4EBFFF");

            backgroundAccentGradient.FadeColour(ColourInfo.GradientHorizontal(backgroundColour.Opacity(0.25f), backgroundColour.Opacity(0f)), DURATION, Easing.OutQuint);
            backgroundBorder.FadeColour(backgroundColour, DURATION, Easing.OutQuint);

            TopLevelContent.FadeEdgeEffectTo(Expanded.Value ? edgeEffectColour.Opacity(0.5f) : Color4.Black.Opacity(0.4f), DURATION, Easing.OutQuint);

            updateXOffset();
            updateHover();
        }

        private void updateXOffset()
        {
            float x = PanelXOffset + corner_radius;

            if (!Expanded.Value && !Selected.Value)
                x += active_x_offset;

            if (!KeyboardSelected.Value)
                x += active_x_offset * 0.5f;

            TopLevelContent.MoveToX(x, DURATION, Easing.OutQuint);
        }

        private void updateHover()
        {
            if (IsHovered)
                hoverLayer.FadeIn(100, Easing.OutQuint);
            else
                hoverLayer.FadeOut(1000, Easing.OutQuint);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateHover();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateHover();
            base.OnHoverLost(e);
        }

        protected override void Update()
        {
            base.Update();
            Content.Padding = Content.Padding with { Left = iconContainer.DrawWidth };
            backgroundLayerHorizontalPadding.Padding = new MarginPadding { Left = iconContainer.DrawWidth };
        }

        #region ICarouselPanel

        public CarouselItem? Item { get; set; }
        public BindableBool Selected { get; } = new BindableBool();
        public BindableBool Expanded { get; } = new BindableBool();
        public BindableBool KeyboardSelected { get; } = new BindableBool();

        public double DrawYPosition { get; set; }

        public virtual void Activated()
        {
            activationFlash.FadeOutFromOne(1000, Easing.OutQuint);
        }

        #endregion
    }
}
