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

        private const float left_edge_x_offset = 20f;
        private const float keyboard_active_x_offset = 25f;
        private const float active_x_offset = 50f;

        private const float duration = 500;

        protected float PanelXOffset { get; init; }

        private Box backgroundBorder = null!;
        private Box backgroundGradient = null!;
        private Box backgroundAccentGradient = null!;
        private Container backgroundLayer = null!;
        private Container backgroundLayerHorizontalPadding = null!;
        private Container backgroundContainer = null!;
        private Container iconContainer = null!;
        private Box activationFlash = null!;
        private Box hoverLayer = null!;

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
                                Child = backgroundLayer = new Container
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Child = new Container
                                    {
                                        Masking = true,
                                        CornerRadius = corner_radius,
                                        RelativeSizeAxes = Axes.Both,
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

            hoverLayer.Colour = colours.Blue.Opacity(0.1f);
            backgroundGradient.Colour = ColourInfo.GradientHorizontal(colourProvider.Background3, colourProvider.Background4);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(_ => updateDisplay());
            KeyboardSelected.BindValueChanged(_ => updateDisplay(), true);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();
            this.FadeInFromZero(duration, Easing.OutQuint);
        }

        [Resolved]
        private BeatmapCarousel? carousel { get; set; }

        protected override bool OnClick(ClickEvent e)
        {
            activationFlash.FadeOutFromOne(500, Easing.OutQuint);
            carousel?.Activate(Item!);
            return true;
        }

        private void updateDisplay()
        {
            backgroundLayer.TransformTo(nameof(Padding), backgroundLayer.Padding with { Vertical = Expanded.Value ? 2f : 0f }, duration, Easing.OutQuint);

            var backgroundColour = accentColour ?? Color4.White;
            var edgeEffectColour = accentColour ?? Color4Extensions.FromHex(@"4EBFFF");

            backgroundAccentGradient.FadeColour(ColourInfo.GradientHorizontal(backgroundColour.Opacity(0.25f), backgroundColour.Opacity(0f)), duration, Easing.OutQuint);
            backgroundBorder.FadeColour(backgroundColour, duration, Easing.OutQuint);

            TopLevelContent.FadeEdgeEffectTo(Expanded.Value ? edgeEffectColour.Opacity(0.5f) : Color4.Black.Opacity(0.4f), duration, Easing.OutQuint);

            updateXOffset();
            updateHover();
        }

        private void updateXOffset()
        {
            float x = PanelXOffset + active_x_offset + keyboard_active_x_offset + left_edge_x_offset;

            if (Expanded.Value)
                x -= active_x_offset;

            if (KeyboardSelected.Value)
                x -= keyboard_active_x_offset;

            this.TransformTo(nameof(Padding), new MarginPadding { Left = x }, duration, Easing.OutQuint);
        }

        private void updateHover()
        {
            bool hovered = IsHovered || KeyboardSelected.Value;

            if (hovered)
                hoverLayer.FadeIn(100, Easing.OutQuint);
            else
                hoverLayer.FadeOut(1000, Easing.OutQuint);
        }

        protected override bool OnHover(HoverEvent e)
        {
            updateDisplay();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateDisplay();
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
            activationFlash.FadeOutFromOne(500, Easing.OutQuint);
        }

        #endregion
    }
}
