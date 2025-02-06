// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Pooling;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Input.Events;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapSetPanel : PoolableDrawable, ICarouselPanel
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT * 1.6f;

        private const float arrow_container_width = 20;
        private const float corner_radius = 10;

        // todo: this should be replaced with information from CarouselItem about how deep is BeatmapPanel in the carousel
        // (i.e. whether it's under a beatmap set that's under a group, or just under a top-level beatmap set).
        private const float set_x_offset = 20f; // constant X offset for beatmap set/standalone panels specifically.

        private const float preselected_x_offset = 25f;
        private const float expanded_x_offset = 50f;

        private const float duration = 500;

        [Resolved]
        private BeatmapCarousel? carousel { get; set; }

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmaps { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private Container panel = null!;
        private Box backgroundBorder = null!;
        private BeatmapSetPanelBackground background = null!;
        private Container backgroundContainer = null!;
        private FillFlowContainer mainFlowContainer = null!;
        private SpriteIcon chevronIcon = null!;
        private Box hoverLayer = null!;

        private OsuSpriteText titleText = null!;
        private OsuSpriteText artistText = null!;
        private UpdateBeatmapSetButtonV2 updateButton = null!;
        private BeatmapSetOnlineStatusPill statusPill = null!;
        private DifficultySpectrumDisplay difficultiesDisplay = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;
            RelativeSizeAxes = Axes.X;
            Height = HEIGHT;

            InternalChild = panel = new Container
            {
                Masking = true,
                CornerRadius = corner_radius,
                RelativeSizeAxes = Axes.Both,
                EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Shadow,
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
                                RelativeSizeAxes = Axes.Y,
                                Alpha = 0,
                                EdgeSmoothness = new Vector2(2, 0),
                            },
                            backgroundContainer = new Container
                            {
                                Masking = true,
                                CornerRadius = corner_radius,
                                RelativeSizeAxes = Axes.X,
                                MaskingSmoothness = 2,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Children = new Drawable[]
                                {
                                    background = new BeatmapSetPanelBackground
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        RelativeSizeAxes = Axes.Both,
                                    },
                                },
                            },
                        }
                    },
                    chevronIcon = new SpriteIcon
                    {
                        X = arrow_container_width / 2,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.CentreLeft,
                        Icon = FontAwesome.Solid.ChevronRight,
                        Size = new Vector2(12),
                        Colour = colourProvider.Background5,
                    },
                    mainFlowContainer = new FillFlowContainer
                    {
                        AutoSizeAxes = Axes.Both,
                        Direction = FillDirection.Vertical,
                        Padding = new MarginPadding { Top = 7.5f, Left = 15, Bottom = 5 },
                        Children = new Drawable[]
                        {
                            titleText = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(weight: FontWeight.Bold, size: 22, italics: true),
                                Shadow = true,
                            },
                            artistText = new OsuSpriteText
                            {
                                Font = OsuFont.GetFont(weight: FontWeight.SemiBold, size: 17, italics: true),
                                Shadow = true,
                            },
                            new FillFlowContainer
                            {
                                Direction = FillDirection.Horizontal,
                                AutoSizeAxes = Axes.Both,
                                Margin = new MarginPadding { Top = 5f },
                                Children = new Drawable[]
                                {
                                    updateButton = new UpdateBeatmapSetButtonV2
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Margin = new MarginPadding { Right = 5f, Top = -2f },
                                    },
                                    statusPill = new BeatmapSetOnlineStatusPill
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Origin = Anchor.CentreLeft,
                                        Anchor = Anchor.CentreLeft,
                                        TextSize = 11,
                                        TextPadding = new MarginPadding { Horizontal = 8, Vertical = 2 },
                                        Margin = new MarginPadding { Right = 5f },
                                    },
                                    difficultiesDisplay = new DifficultySpectrumDisplay
                                    {
                                        DotSize = new Vector2(5, 10),
                                        DotSpacing = 2,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    },
                                },
                            }
                        }
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

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            var inputRectangle = panel.DrawRectangle;

            // Cover a gap introduced by the spacing between a BeatmapSetPanel and a BeatmapPanel either above it or below it.
            inputRectangle = inputRectangle.Inflate(new MarginPadding { Vertical = BeatmapCarousel.SPACING / 2f });

            return inputRectangle.Contains(panel.ToLocalSpace(screenSpacePos));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            Expanded.BindValueChanged(_ => updateExpandedDisplay(), true);
            KeyboardSelected.BindValueChanged(_ => updateKeyboardSelectedDisplay(), true);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);

            var beatmapSet = (BeatmapSetInfo)Item.Model;

            // Choice of background image matches BSS implementation (always uses the lowest `beatmap_id` from the set).
            background.Beatmap = beatmaps.GetWorkingBeatmap(beatmapSet.Beatmaps.MinBy(b => b.OnlineID));

            titleText.Text = new RomanisableString(beatmapSet.Metadata.TitleUnicode, beatmapSet.Metadata.Title);
            artistText.Text = new RomanisableString(beatmapSet.Metadata.ArtistUnicode, beatmapSet.Metadata.Artist);
            updateButton.BeatmapSet = beatmapSet;
            statusPill.Status = beatmapSet.Status;
            difficultiesDisplay.BeatmapSet = beatmapSet;

            updateExpandedDisplay();
            FinishTransforms(true);

            this.FadeInFromZero(duration, Easing.OutQuint);
        }

        protected override void FreeAfterUse()
        {
            base.FreeAfterUse();

            background.Beatmap = null;
            updateButton.BeatmapSet = null;
            difficultiesDisplay.BeatmapSet = null;
        }

        private void updateExpandedDisplay()
        {
            if (Item == null)
                return;

            updatePanelPosition();

            backgroundBorder.RelativeSizeAxes = Expanded.Value ? Axes.Both : Axes.Y;
            backgroundBorder.Width = Expanded.Value ? 1 : arrow_container_width + corner_radius;
            backgroundBorder.FadeTo(Expanded.Value ? 1 : 0, duration, Easing.OutQuint);
            chevronIcon.FadeTo(Expanded.Value ? 1 : 0, duration, Easing.OutQuint);

            backgroundContainer.ResizeHeightTo(Expanded.Value ? HEIGHT - 4 : HEIGHT, duration, Easing.OutQuint);
            backgroundContainer.MoveToX(Expanded.Value ? arrow_container_width : 0, duration, Easing.OutQuint);
            mainFlowContainer.MoveToX(Expanded.Value ? arrow_container_width : 0, duration, Easing.OutQuint);

            panel.EdgeEffect = panel.EdgeEffect with { Radius = Expanded.Value ? 15 : 10 };

            panel.FadeEdgeEffectTo(Expanded.Value
                ? Color4Extensions.FromHex(@"4EBFFF").Opacity(0.5f)
                : Color4.Black.Opacity(0.4f), duration, Easing.OutQuint);
        }

        private void updateKeyboardSelectedDisplay()
        {
            updatePanelPosition();
            updateHover();
        }

        private void updatePanelPosition()
        {
            float x = set_x_offset + expanded_x_offset + preselected_x_offset;

            if (Expanded.Value)
                x -= expanded_x_offset;

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

        protected override bool OnClick(ClickEvent e)
        {
            if (carousel != null)
                carousel.CurrentSelection = Item!.Model;

            return true;
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
