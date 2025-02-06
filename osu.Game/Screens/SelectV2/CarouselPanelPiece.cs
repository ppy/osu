// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class CarouselPanelPiece : Container
    {
        private const float corner_radius = 10;

        private const float left_edge_x_offset = 20f;
        private const float keyboard_active_x_offset = 25f;
        private const float active_x_offset = 50f;

        private const float duration = 500;

        private readonly float panelXOffset;

        private readonly Box backgroundBorder;
        private readonly Box backgroundGradient;
        private readonly Box backgroundAccentGradient;
        private readonly Container backgroundLayer;
        private readonly Container backgroundLayerHorizontalPadding;
        private readonly Container backgroundContainer;
        private readonly Container iconContainer;
        private readonly Box activationFlash;
        private readonly Box hoverLayer;

        public Container TopLevelContent { get; }

        protected override Container Content { get; }

        public Drawable Background
        {
            set => backgroundContainer.Child = value;
        }

        public Drawable Icon
        {
            set => iconContainer.Child = value;
        }

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

        public readonly BindableBool Active = new BindableBool();
        public readonly BindableBool KeyboardActive = new BindableBool();

        public Action? Action;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            var inputRectangle = TopLevelContent.DrawRectangle;

            // Cover potential gaps introduced by the spacing between panels.
            inputRectangle = inputRectangle.Inflate(new MarginPadding { Vertical = BeatmapCarousel.SPACING / 2f });

            return inputRectangle.Contains(TopLevelContent.ToLocalSpace(screenSpacePos));
        }

        public CarouselPanelPiece(float panelXOffset)
        {
            this.panelXOffset = panelXOffset;

            RelativeSizeAxes = Axes.Both;

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
                        Padding = new MarginPadding { Right = panelXOffset + corner_radius },
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
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            hoverLayer.Colour = colours.Blue.Opacity(0.1f);
            backgroundGradient.Colour = ColourInfo.GradientHorizontal(colourProvider.Background3, colourProvider.Background4);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Active.BindValueChanged(_ => updateDisplay());
            KeyboardActive.BindValueChanged(_ => updateDisplay(), true);
        }

        public void Flash()
        {
            activationFlash.FadeOutFromOne(500, Easing.OutQuint);
        }

        private void updateDisplay()
        {
            backgroundLayer.TransformTo(nameof(Padding), backgroundLayer.Padding with { Vertical = Active.Value ? 2f : 0f }, duration, Easing.OutQuint);

            var backgroundColour = accentColour ?? Color4.White;
            var edgeEffectColour = accentColour ?? Color4Extensions.FromHex(@"4EBFFF");

            backgroundAccentGradient.FadeColour(ColourInfo.GradientHorizontal(backgroundColour.Opacity(0.25f), backgroundColour.Opacity(0f)), duration, Easing.OutQuint);
            backgroundBorder.FadeColour(backgroundColour, duration, Easing.OutQuint);

            TopLevelContent.FadeEdgeEffectTo(Active.Value ? edgeEffectColour.Opacity(0.5f) : Color4.Black.Opacity(0.4f), duration, Easing.OutQuint);

            updateXOffset();
            updateHover();
        }

        private void updateXOffset()
        {
            float x = panelXOffset + active_x_offset + keyboard_active_x_offset + left_edge_x_offset;

            if (Active.Value)
                x -= active_x_offset;

            if (KeyboardActive.Value)
                x -= keyboard_active_x_offset;

            this.TransformTo(nameof(Padding), new MarginPadding { Left = x }, duration, Easing.OutQuint);
        }

        private void updateHover()
        {
            bool hovered = IsHovered || KeyboardActive.Value;

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

        protected override bool OnClick(ClickEvent e)
        {
            Action?.Invoke();
            return true;
        }

        protected override void Update()
        {
            base.Update();
            Content.Padding = Content.Padding with { Left = iconContainer.DrawWidth };
            backgroundLayerHorizontalPadding.Padding = new MarginPadding { Left = iconContainer.DrawWidth };
        }
    }
}
