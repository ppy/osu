// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Localisation;
using osu.Game.Online;
using osu.Game.Online.Chat;
using osu.Game.Overlays;
using osu.Game.Overlays.Mods;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class WedgeDifficultyDisplay : CompositeDrawable
    {
        private const float border_weight = 2;

        private static readonly Vector2 shear = new Vector2(OsuGame.SHEAR, 0);

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<RulesetInfo> ruleset { get; set; } = null!;

        [Resolved]
        private IBindable<IReadOnlyList<Mod>> mods { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        private StarRatingDisplay starRatingDisplay = null!;
        private FillFlowContainer nameLine = null!;
        private OsuSpriteText difficultyText = null!;
        private OsuSpriteText mappedByText = null!;
        private OsuHoverContainer mapperLink = null!;
        private OsuSpriteText mapperText = null!;

        private GridContainer ratingAndNameContainer = null!;
        private FillFlowContainer<WedgeStatisticDifficulty> beatmapStatisticsFlow = null!;
        private DifficultyStatisticsFlow difficultyStatisticsFlow = null!;

        private WedgeStatisticDifficulty firstStatisticDifficulty = null!;
        private WedgeStatisticDifficulty accuracy = null!;
        private WedgeStatisticDifficulty hpDrain = null!;
        private WedgeStatisticDifficulty approachRate = null!;

        private CancellationTokenSource? cancellationSource;

        public IBindable<double> DisplayedStars => displayedStars;

        private readonly Bindable<double> displayedStars = new BindableDouble();

        public WedgeDifficultyDisplay()
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
        }

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            Masking = true;
            CornerRadius = 10;
            Shear = shear;

            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = colourProvider.Background4,
                },
                new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Vertical,
                    Shear = -shear,
                    Children = new Drawable[]
                    {
                        new ShearAlignedDrawable(shear, ratingAndNameContainer = new GridContainer
                        {
                            AlwaysPresent = true,
                            RelativeSizeAxes = Axes.X,
                            Height = 28f,
                            Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN },
                            ColumnDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Absolute, 6),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new[]
                                {
                                    starRatingDisplay = new StarRatingDisplay(default, animated: true)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Scale = new Vector2(1f),
                                    },
                                    Empty(),
                                    nameLine = new FillFlowContainer
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Direction = FillDirection.Horizontal,
                                        Margin = new MarginPadding { Bottom = 2f },
                                        Children = new Drawable[]
                                        {
                                            difficultyText = new TruncatingSpriteText
                                            {
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                Font = OsuFont.Torus.With(size: 16f, weight: FontWeight.SemiBold),
                                            },
                                            mappedByText = new OsuSpriteText
                                            {
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                Text = BeatmapsetsStrings.ShowDetailsMappedBy(string.Empty),
                                                Margin = new MarginPadding { Left = 4f },
                                                Font = OsuFont.Torus.With(size: 14f, weight: FontWeight.Regular),
                                            },
                                            mapperLink = new MapperLinkContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                Child = mapperText = new TruncatingSpriteText
                                                {
                                                    Shadow = true,
                                                    Font = OsuFont.Torus.With(size: 14f, weight: FontWeight.SemiBold),
                                                },
                                            },
                                        },
                                    },
                                }
                            },
                        }),
                        new ShearAlignedDrawable(shear, new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            Padding = new MarginPadding { Bottom = border_weight, Right = border_weight },
                            Child = new Container
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Masking = true,
                                CornerRadius = 10 - border_weight,
                                Shear = shear,
                                Children = new Drawable[]
                                {
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background5,
                                    },
                                    new Container
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 20f, Top = 7.5f, Bottom = 5f },
                                        Shear = -shear,
                                        Children = new Drawable[]
                                        {
                                            new Container
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Children = new Drawable[]
                                                {
                                                    beatmapStatisticsFlow = new FillFlowContainer<WedgeStatisticDifficulty>
                                                    {
                                                        AutoSizeAxes = Axes.Both,
                                                        Spacing = new Vector2(8f, 0f),
                                                    },
                                                }
                                            },
                                            new Container
                                            {
                                                Anchor = Anchor.TopRight,
                                                Origin = Anchor.TopRight,
                                                AutoSizeAxes = Axes.Both,
                                                Children = new Drawable[]
                                                {
                                                    new Box
                                                    {
                                                        Colour = ColourInfo.GradientHorizontal(colourProvider.Background5.Opacity(0), colourProvider.Background5),
                                                        Width = 30,
                                                        RelativeSizeAxes = Axes.Y,
                                                        Origin = Anchor.TopRight,
                                                    },
                                                    new Box
                                                    {
                                                        Colour = colourProvider.Background5,
                                                        RelativeSizeAxes = Axes.Both,
                                                    },
                                                    difficultyStatisticsFlow = new DifficultyStatisticsFlow
                                                    {
                                                        AutoSizeAxes = Axes.Both,
                                                        Spacing = new Vector2(8f, 0f),
                                                        Padding = new MarginPadding { Left = 10f },
                                                        Children = new[]
                                                        {
                                                            firstStatisticDifficulty = new WedgeStatisticDifficulty(BeatmapsetsStrings.ShowStatsCs) { Width = 65 },
                                                            accuracy = new WedgeStatisticDifficulty(BeatmapsetsStrings.ShowStatsAccuracy) { Width = 65 },
                                                            hpDrain = new WedgeStatisticDifficulty(BeatmapsetsStrings.ShowStatsDrain) { Width = 65 },
                                                            approachRate = new WedgeStatisticDifficulty(BeatmapsetsStrings.ShowStatsAr) { Width = 65 },
                                                        },
                                                    }
                                                }
                                            },
                                        }
                                    },
                                },
                            }
                        }),
                    }
                },
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            beatmap.BindValueChanged(_ => updateDisplay());
            ruleset.BindValueChanged(_ => updateDisplay());
            mods.BindValueChanged(_ => updateDisplay());
            updateDisplay();

            displayedStars.BindValueChanged(_ => updateStars(), true);
            FinishTransforms(true);
        }

        [Resolved]
        private ILinkHandler? linkHandler { get; set; }

        private void updateDisplay()
        {
            cancellationSource?.Cancel();
            cancellationSource = new CancellationTokenSource();

            computeStarDifficulty(cancellationSource.Token);

            if (beatmap.IsDefault)
                ratingAndNameContainer.FadeOut(300, Easing.OutQuint);
            else
            {
                ratingAndNameContainer.FadeIn(300, Easing.OutQuint);
                difficultyText.Text = beatmap.Value.BeatmapInfo.DifficultyName;
                mapperLink.Action = () => linkHandler?.HandleLink(new LinkDetails(LinkAction.OpenUserProfile, beatmap.Value.Metadata.Author));
                mapperText.Text = beatmap.Value.Metadata.Author.Username;
            }

            var playableBeatmap = beatmap.Value.GetPlayableBeatmap(ruleset.Value);
            var newStatistics = playableBeatmap.GetStatistics().Select(s => new WedgeStatisticDifficulty(s.Name)
            {
                Width = 75,
                Value = (s.Count, s.Maximum),
            }).ToArray();

            var currentStatistics = beatmapStatisticsFlow.Children;

            if (currentStatistics.Select(s => s.Label).SequenceEqual(newStatistics.Select(s => s.Label)))
            {
                for (int i = 0; i < newStatistics.Length; i++)
                    currentStatistics[i].Value = newStatistics[i].Value;
            }
            else
                beatmapStatisticsFlow.Children = newStatistics;

            BeatmapDifficulty baseDifficulty = beatmap.Value.BeatmapInfo.Difficulty;
            BeatmapDifficulty originalDifficulty = new BeatmapDifficulty(baseDifficulty);

            foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(originalDifficulty);

            var rateAdjustedDifficulty = originalDifficulty;

            if (ruleset.Value != null)
            {
                double rate = ModUtils.CalculateRateWithMods(mods.Value);

                rateAdjustedDifficulty = ruleset.Value.CreateInstance().GetRateAdjustedDisplayDifficulty(originalDifficulty, rate);
                difficultyStatisticsFlow.TooltipContent = new AdjustedAttributesTooltip.Data(originalDifficulty, rateAdjustedDifficulty);
            }

            switch (ruleset.Value?.OnlineID)
            {
                case 3:
                    // Account for mania differences locally for now.
                    // Eventually this should be handled in a more modular way, allowing rulesets to return arbitrary difficulty attributes.
                    ILegacyRuleset legacyRuleset = (ILegacyRuleset)ruleset.Value.CreateInstance();

                    // For the time being, the key count is static no matter what, because:
                    // a) The method doesn't have knowledge of the active keymods. Doing so may require considerations for filtering.
                    // b) Using the difficulty adjustment mod to adjust OD doesn't have an effect on conversion.
                    int keyCount = legacyRuleset.GetKeyCount(beatmap.Value.BeatmapInfo, mods.Value);

                    firstStatisticDifficulty.Label = BeatmapsetsStrings.ShowStatsCsMania;
                    firstStatisticDifficulty.Value = (keyCount, 10);
                    break;

                default:
                    firstStatisticDifficulty.Label = BeatmapsetsStrings.ShowStatsCs;
                    firstStatisticDifficulty.Value = (rateAdjustedDifficulty.CircleSize, 10f);
                    break;
            }

            accuracy.Value = (rateAdjustedDifficulty.OverallDifficulty, 10f);
            hpDrain.Value = (rateAdjustedDifficulty.DrainRate, 10f);
            approachRate.Value = (rateAdjustedDifficulty.ApproachRate, 10f);
        }

        private void updateStars()
        {
            starRatingDisplay.Current.Value = new StarDifficulty(displayedStars.Value, 0);

            Color4 colour = displayedStars.Value >= 6.5f ? colours.Orange1 : colours.ForStarDifficulty(displayedStars.Value);
            difficultyText.FadeColour(colour, 300, Easing.OutQuint);
            mappedByText.FadeColour(colour, 300, Easing.OutQuint);

            foreach (var statistic in beatmapStatisticsFlow.Concat(difficultyStatisticsFlow))
                statistic.TransformTo(nameof(statistic.AccentColour), colour, 300, Easing.OutQuint);
        }

        private void computeStarDifficulty(CancellationToken cancellationToken)
        {
            difficultyCache.GetDifficultyAsync(beatmap.Value.BeatmapInfo, ruleset.Value, mods.Value, cancellationToken)
                           .ContinueWith(task =>
                           {
                               Schedule(() =>
                               {
                                   if (cancellationToken.IsCancellationRequested)
                                       return;

                                   var result = task.GetResultSafely() ?? default;
                                   displayedStars.Value = result.Stars;
                               });
                           }, cancellationToken);
        }

        protected override void Update()
        {
            base.Update();
            difficultyText.MaxWidth = Math.Max(nameLine.DrawWidth - mappedByText.DrawWidth - mapperText.DrawWidth - 20, 0);
        }

        private partial class MapperLinkContainer : OsuHoverContainer
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider? overlayColourProvider, OsuColour colours)
            {
                TooltipText = ContextMenuStrings.ViewProfile;
                IdleColour = overlayColourProvider?.Light2 ?? colours.Blue;
            }
        }

        private partial class DifficultyStatisticsFlow : FillFlowContainer<WedgeStatisticDifficulty>, IHasCustomTooltip<AdjustedAttributesTooltip.Data>
        {
            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public ITooltip<AdjustedAttributesTooltip.Data> GetCustomTooltip() => new AdjustedAttributesTooltip(colourProvider);

            public AdjustedAttributesTooltip.Data? TooltipContent { get; set; }
        }
    }
}
