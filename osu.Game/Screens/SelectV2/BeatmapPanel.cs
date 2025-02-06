// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapPanel : PoolableDrawable, ICarouselPanel
    {
        public const float HEIGHT = CarouselItem.DEFAULT_HEIGHT;

        private const float colour_box_width = 30;
        private const float corner_radius = 10;

        // todo: this should be replaced with information from CarouselItem about how deep is BeatmapPanel in the carousel
        // (i.e. whether it's under a beatmap set that's under a group, or just under a top-level beatmap set).
        private const float difficulty_x_offset = 100f; // constant X offset for beatmap difficulty panels specifically.

        private const float preselected_x_offset = 25f;
        private const float selected_x_offset = 50f;

        private const float duration = 500;

        [Resolved]
        private BeatmapCarousel? carousel { get; set; }

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        private Container panel = null!;
        private StarCounter starCounter = null!;
        private ConstrainedIconContainer iconContainer = null!;
        private Box hoverLayer = null!;
        private Box activationFlash = null!;

        private Box backgroundBorder = null!;

        private StarRatingDisplay starRatingDisplay = null!;

        [Resolved]
        private OverlayColourProvider colourProvider { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private OsuSpriteText keyCountText = null!;

        private IBindable<StarDifficulty?>? starDifficultyBindable;
        private CancellationTokenSource? starDifficultyCancellationSource;

        private Container rightContainer = null!;
        private Box starRatingGradient = null!;
        private TopLocalRankV2 difficultyRank = null!;
        private OsuSpriteText difficultyText = null!;
        private OsuSpriteText authorText = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Anchor = Anchor.TopRight;
            Origin = Anchor.TopRight;

            RelativeSizeAxes = Axes.X;
            Width = 1f;
            Height = HEIGHT;

            InternalChild = panel = new Container
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
                                Colour = colours.ForStarDifficulty(0),
                                EdgeSmoothness = new Vector2(2, 0),
                            },
                            rightContainer = new Container
                            {
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Masking = true,
                                CornerRadius = corner_radius,
                                RelativeSizeAxes = Axes.X,
                                Height = HEIGHT,
                                X = colour_box_width,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = ColourInfo.GradientHorizontal(colourProvider.Background3, colourProvider.Background4),
                                    },
                                    starRatingGradient = new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Alpha = 0,
                                    },
                                },
                            },
                        }
                    },
                    iconContainer = new ConstrainedIconContainer
                    {
                        X = colour_box_width / 2,
                        Origin = Anchor.Centre,
                        Anchor = Anchor.CentreLeft,
                        Size = new Vector2(20),
                        Colour = colourProvider.Background5,
                    },
                    new FillFlowContainer
                    {
                        Padding = new MarginPadding { Top = 8, Left = colour_box_width + corner_radius },
                        Direction = FillDirection.Vertical,
                        AutoSizeAxes = Axes.Both,
                        Children = new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                Direction = FillDirection.Horizontal,
                                Spacing = new Vector2(3, 0),
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    starRatingDisplay = new StarRatingDisplay(default, StarRatingDisplaySize.Small)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                    },
                                    difficultyRank = new TopLocalRankV2
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Scale = new Vector2(0.75f)
                                    },
                                    starCounter = new StarCounter
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Scale = new Vector2(0.4f)
                                    }
                                }
                            },
                            new FillFlowContainer
                            {
                                Direction = FillDirection.Horizontal,
                                AutoSizeAxes = Axes.Both,
                                Children = new[]
                                {
                                    keyCountText = new OsuSpriteText
                                    {
                                        Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        Alpha = 0,
                                    },
                                    difficultyText = new OsuSpriteText
                                    {
                                        Font = OsuFont.GetFont(size: 18, weight: FontWeight.SemiBold),
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        Margin = new MarginPadding { Right = 8f },
                                    },
                                    authorText = new OsuSpriteText
                                    {
                                        Colour = colourProvider.Content2,
                                        Font = OsuFont.GetFont(weight: FontWeight.SemiBold),
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft
                                    }
                                }
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
                    activationFlash = new Box
                    {
                        Blending = BlendingParameters.Additive,
                        Alpha = 0f,
                        RelativeSizeAxes = Axes.Both,
                    },
                    new HoverSounds(),
                }
            };
        }

        public override bool ReceivePositionalInputAt(Vector2 screenSpacePos)
        {
            var inputRectangle = panel.DrawRectangle;

            // Cover the gaps introduced by the spacing between BeatmapPanels.
            inputRectangle = inputRectangle.Inflate(new MarginPadding { Vertical = BeatmapCarousel.SPACING / 2f });

            return inputRectangle.Contains(panel.ToLocalSpace(screenSpacePos));
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ruleset.BindValueChanged(_ =>
            {
                computeStarRating();
                updateKeyCount();
            });

            mods.BindValueChanged(_ =>
            {
                computeStarRating();
                updateKeyCount();
            }, true);

            Selected.BindValueChanged(_ => updateSelectionDisplay(), true);
            KeyboardSelected.BindValueChanged(_ => updateKeyboardSelectedDisplay(), true);
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            Debug.Assert(Item != null);
            var beatmap = (BeatmapInfo)Item.Model;

            iconContainer.Icon = beatmap.Ruleset.CreateInstance().CreateIcon();

            difficultyRank.Beatmap = beatmap;
            difficultyText.Text = beatmap.DifficultyName;
            authorText.Text = BeatmapsetsStrings.ShowDetailsMappedBy(beatmap.Metadata.Author.Username);

            starDifficultyBindable = null;

            computeStarRating();
            updateKeyCount();

            updateSelectionDisplay();
            FinishTransforms(true);

            this.FadeInFromZero(duration, Easing.OutQuint);

            // todo: only do this when visible.
            // starCounter.ReplayAnimation();
        }

        private void updateSelectionDisplay()
        {
            bool selected = Selected.Value;

            rightContainer.ResizeHeightTo(selected ? HEIGHT - 4 : HEIGHT, duration, Easing.OutQuint);

            updatePanelPosition();
            updateEdgeEffectColour();
            updateHover();
        }

        private void updateKeyboardSelectedDisplay()
        {
            updatePanelPosition();
            updateHover();
        }

        private void updatePanelPosition()
        {
            float x = difficulty_x_offset + selected_x_offset + preselected_x_offset;

            if (Selected.Value)
                x -= selected_x_offset;

            if (KeyboardSelected.Value)
                x -= preselected_x_offset;

            this.TransformTo(nameof(Padding), new MarginPadding { Left = x }, duration, Easing.OutQuint);
        }

        private void updateHover()
        {
            bool hovered = IsHovered || (KeyboardSelected.Value && !Selected.Value);

            if (hovered)
                hoverLayer.FadeIn(100, Easing.OutQuint);
            else
                hoverLayer.FadeOut(1000, Easing.OutQuint);
        }

        private void computeStarRating()
        {
            starDifficultyCancellationSource?.Cancel();
            starDifficultyCancellationSource = new CancellationTokenSource();

            if (Item == null)
                return;

            var beatmap = (BeatmapInfo)Item.Model;

            starDifficultyBindable = difficultyCache.GetBindableDifficulty(beatmap, starDifficultyCancellationSource.Token);
            starDifficultyBindable.BindValueChanged(d =>
            {
                var value = d.NewValue ?? default;

                starRatingDisplay.Current.Value = value;
                starCounter.Current = (float)value.Stars;

                iconContainer.FadeColour(value.Stars > 6.5f ? colours.Orange1 : colourProvider.Background5, duration, Easing.OutQuint);

                var starRatingColour = colours.ForStarDifficulty(value.Stars);

                backgroundBorder.FadeColour(starRatingColour, duration, Easing.OutQuint);
                starCounter.FadeColour(starRatingColour, duration, Easing.OutQuint);
                starRatingGradient.FadeColour(ColourInfo.GradientHorizontal(starRatingColour.Opacity(0.25f), starRatingColour.Opacity(0)), duration, Easing.OutQuint);
                starRatingGradient.FadeIn(duration, Easing.OutQuint);

                // todo: this doesn't work for dark star rating colours, still not sure how to fix.
                activationFlash.FadeColour(starRatingColour, duration, Easing.OutQuint);

                updateEdgeEffectColour();
            }, true);
        }

        private void updateEdgeEffectColour()
        {
            panel.FadeEdgeEffectTo(Selected.Value
                ? colours.ForStarDifficulty(starDifficultyBindable?.Value?.Stars ?? 0f).Opacity(0.5f)
                : Color4.Black.Opacity(0.4f), duration, Easing.OutQuint);
        }

        private void updateKeyCount()
        {
            if (Item == null)
                return;

            var beatmap = (BeatmapInfo)Item.Model;

            if (ruleset.Value.OnlineID == 3)
            {
                // Account for mania differences locally for now.
                // Eventually this should be handled in a more modular way, allowing rulesets to add more information to the panel.
                ILegacyRuleset legacyRuleset = (ILegacyRuleset)ruleset.Value.CreateInstance();
                int keyCount = legacyRuleset.GetKeyCount(beatmap, mods.Value);

                keyCountText.Alpha = 1;
                keyCountText.Text = $"[{keyCount}K] ";
            }
            else
                keyCountText.Alpha = 0;
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
            if (carousel == null)
                return true;

            if (carousel.CurrentSelection != Item!.Model)
            {
                carousel.CurrentSelection = Item!.Model;
                return true;
            }

            carousel.TryActivateSelection();
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
            activationFlash.FadeOutFromOne(500, Easing.OutQuint);
        }

        #endregion
    }
}
