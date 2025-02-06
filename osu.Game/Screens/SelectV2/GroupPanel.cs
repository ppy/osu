// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class GroupPanel : PoolableDrawable, ICarouselPanel
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT;

        private const float corner_radius = 10;

        private const float glow_offset = 10f; // extra space for any edge effect to not be cutoff by the right edge of the carousel.
        private const float preselected_x_offset = 25f;
        private const float selected_x_offset = 50f;

        private const float duration = 500;

        [Resolved]
        private BeatmapCarousel? carousel { get; set; }

        private Container panel = null!;
        private Box activationFlash = null!;
        private OsuSpriteText titleText = null!;
        private Box hoverLayer = null!;

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            var inputRectangle = panel.DrawRectangle;

            // Cover a gap introduced by the spacing between a GroupPanel and a BeatmapPanel either below/above it.
            inputRectangle = inputRectangle.Inflate(new MarginPadding { Vertical = BeatmapCarousel.SPACING / 2f });

            return inputRectangle.Contains(panel.ToLocalSpace(screenSpacePos));
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours)
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            InternalChild = panel = new Container
            {
                RelativeSizeAxes = Axes.Both,
                CornerRadius = corner_radius,
                Masking = true,
                X = corner_radius,
                Children = new Drawable[]
                {
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Left = 10f },
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = corner_radius,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background6,
                                },
                            }
                        }
                    },
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = colourProvider.Background3,
                    },
                    new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Padding = new MarginPadding { Left = 10f },
                        Child = new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            CornerRadius = corner_radius,
                            Masking = true,
                            Children = new Drawable[]
                            {
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background5,
                                },
                                titleText = new OsuSpriteText
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    X = 10f,
                                },
                                new CircularContainer
                                {
                                    Anchor = Anchor.CentreRight,
                                    Origin = Anchor.CentreRight,
                                    Size = new Vector2(50f, 14f),
                                    Margin = new MarginPadding { Right = 30f },
                                    Masking = true,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Color4.Black.Opacity(0.7f),
                                        },
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Font = OsuFont.Torus.With(size: 14.4f, weight: FontWeight.Bold),
                                            // TODO: requires Carousel/CarouselItem-side implementation
                                            Text = "43",
                                            UseFullGlyphHeight = false,
                                        }
                                    },
                                },
                            }
                        }
                    },
                    activationFlash = new Box
                    {
                        Colour = Color4.White,
                        Blending = BlendingParameters.Additive,
                        Alpha = 0,
                        RelativeSizeAxes = Axes.Both,
                    },
                    hoverLayer = new Box
                    {
                        Colour = colours.Blue.Opacity(0.1f),
                        Alpha = 0,
                        Blending = BlendingParameters.Additive,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new HoverSounds(),
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(_ => updateExpandedDisplay(), true);
            KeyboardSelected.BindValueChanged(_ => updateKeyboardSelectedDisplay(), true);
        }

        private void updateExpandedDisplay()
        {
            updatePanelPosition();

            // todo: figma shares no extra visual feedback on this.

            activationFlash.FadeTo(0.2f).FadeTo(0f, 500, Easing.OutQuint);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);

            GroupDefinition group = (GroupDefinition)Item.Model;

            titleText.Text = group.Title;

            this.FadeInFromZero(500, Easing.OutQuint);
        }

        protected override bool OnClick(ClickEvent e)
        {
            if (carousel != null)
                carousel.CurrentSelection = Item!.Model;

            return true;
        }

        private void updateKeyboardSelectedDisplay()
        {
            updatePanelPosition();
            updateHover();
        }

        private void updatePanelPosition()
        {
            float x = glow_offset + selected_x_offset + preselected_x_offset;

            if (Expanded.Value)
                x -= selected_x_offset;

            if (KeyboardSelected.Value)
                x -= preselected_x_offset;

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
            updateHover();
            return true;
        }

        protected override void OnHoverLost(HoverLostEvent e)
        {
            updateHover();
            base.OnHoverLost(e);
        }

        #region ICarouselPanel

        public CarouselItem? Item { get; set; }
        public BindableBool Selected { get; } = new BindableBool();
        public BindableBool Expanded { get; } = new BindableBool();
        public BindableBool KeyboardSelected { get; } = new BindableBool();

        public double DrawYPosition { get; set; }

        public void Activated()
        {
            // sets should never be activated.
            throw new InvalidOperationException();
        }

        #endregion
    }
}
