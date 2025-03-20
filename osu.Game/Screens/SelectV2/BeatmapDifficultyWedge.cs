// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Mods;
using osu.Game.Screens.SelectV2.Wedges;
using osu.Game.Utils;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Screens.SelectV2
{
    public partial class BeatmapDifficultyWedge : CompositeDrawable
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
        private OsuSpriteText difficultyText = null!;
        private OsuSpriteText mappedByText = null!;
        private OsuHoverContainer mapperLink = null!;
        private OsuSpriteText mapperText = null!;

        private FillFlowContainer ratingAndNameFlow = null!;
        private FillFlowContainer<BeatmapDifficultyWedgeStatistic> beatmapStatisticsFlow = null!;
        private FillFlowContainer<BeatmapDifficultyWedgeStatistic> difficultyStatisticsFlow = null!;

        private BeatmapDifficultyWedgeStatistic firstDifficultyStatistic = null!;
        private BeatmapDifficultyWedgeStatistic accuracyStatistic = null!;
        private BeatmapDifficultyWedgeStatistic hpDrainStatistic = null!;
        private BeatmapDifficultyWedgeStatistic approachRateStatistic = null!;

        private CancellationTokenSource? cancellationSource;

        public IBindable<double> DisplayedStars => displayedStars;

        private readonly Bindable<double> displayedStars = new BindableDouble();

        public BeatmapDifficultyWedge()
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
                        new ShearAlignedDrawable(shear, ratingAndNameFlow = new FillFlowContainer
                        {
                            AlwaysPresent = true,
                            AutoSizeAxes = Axes.X,
                            Height = 28f,
                            Direction = FillDirection.Horizontal,
                            Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN },
                            Spacing = new Vector2(8f, 0f),
                            Children = new Drawable[]
                            {
                                starRatingDisplay = new StarRatingDisplay(default, animated: true)
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                },
                                new FillFlowContainer
                                {
                                    Anchor = Anchor.CentreLeft,
                                    Origin = Anchor.CentreLeft,
                                    AutoSizeAxes = Axes.Both,
                                    Direction = FillDirection.Horizontal,
                                    Margin = new MarginPadding { Bottom = 2f },
                                    Children = new Drawable[]
                                    {
                                        difficultyText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Font = OsuFont.Torus.With(size: 19.2f, weight: FontWeight.SemiBold),
                                        },
                                        mappedByText = new OsuSpriteText
                                        {
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Text = " mapped by ",
                                            Font = OsuFont.Torus.With(size: 16.8f, weight: FontWeight.Regular),
                                        },
                                        mapperLink = new MapperLinkContainer
                                        {
                                            AutoSizeAxes = Axes.Both,
                                            Anchor = Anchor.BottomLeft,
                                            Origin = Anchor.BottomLeft,
                                            Child = mapperText = new TruncatingSpriteText
                                            {
                                                Shadow = true,
                                                Font = OsuFont.Torus.With(size: 16.8f, weight: FontWeight.SemiBold),
                                            },
                                        },
                                    },
                                },
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
                                                    beatmapStatisticsFlow = new FillFlowContainer<BeatmapDifficultyWedgeStatistic>
                                                    {
                                                        AutoSizeAxes = Axes.Both,
                                                        Spacing = new Vector2(12f, 0f),
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
                                                        Width = 50,
                                                        RelativeSizeAxes = Axes.Y,
                                                        Origin = Anchor.TopRight,
                                                    },
                                                    new Box
                                                    {
                                                        Colour = colourProvider.Background5,
                                                        RelativeSizeAxes = Axes.Both,
                                                    },
                                                    difficultyStatisticsFlow = new FillFlowContainer<BeatmapDifficultyWedgeStatistic>
                                                    {
                                                        AutoSizeAxes = Axes.Both,
                                                        Spacing = new Vector2(12f, 0f),
                                                        Padding = new MarginPadding { Left = 10f },
                                                        Children = new[]
                                                        {
                                                            firstDifficultyStatistic = new BeatmapDifficultyWedgeStatistic(BeatmapsetsStrings.ShowStatsCs),
                                                            accuracyStatistic = new BeatmapDifficultyWedgeStatistic(BeatmapsetsStrings.ShowStatsAccuracy),
                                                            hpDrainStatistic = new BeatmapDifficultyWedgeStatistic(BeatmapsetsStrings.ShowStatsDrain),
                                                            approachRateStatistic = new BeatmapDifficultyWedgeStatistic(BeatmapsetsStrings.ShowStatsAr),
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
                ratingAndNameFlow.FadeOut(300, Easing.OutQuint);
            else
            {
                ratingAndNameFlow.FadeIn(300, Easing.OutQuint);
                difficultyText.Text = beatmap.Value.BeatmapInfo.DifficultyName;
                mapperLink.Action = () => linkHandler?.HandleLink(new LinkDetails(LinkAction.OpenUserProfile, beatmap.Value.Metadata.Author));
                mapperText.Text = beatmap.Value.Metadata.Author.Username;
            }

            var playableBeatmap = beatmap.Value.GetPlayableBeatmap(ruleset.Value);
            var newStatistics = playableBeatmap.GetStatistics().Select(s => new BeatmapDifficultyWedgeStatistic(s.Name)
            {
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

            BeatmapDifficulty? baseDifficulty = beatmap.Value.BeatmapInfo.Difficulty;

            if (baseDifficulty != null)
            {
                BeatmapDifficulty originalDifficulty = new BeatmapDifficulty(baseDifficulty);

                foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                    mod.ApplyToDifficulty(originalDifficulty);

                var rateAdjustedDifficulty = originalDifficulty;

                if (ruleset.Value != null)
                {
                    double rate = ModUtils.CalculateRateWithMods(mods.Value);

                    rateAdjustedDifficulty = ruleset.Value.CreateInstance().GetRateAdjustedDisplayDifficulty(originalDifficulty, rate);

                    // TooltipContent = new AdjustedAttributesTooltip.Data(originalDifficulty, adjustedDifficulty);
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

                        firstDifficultyStatistic.Label = BeatmapsetsStrings.ShowStatsCsMania;
                        firstDifficultyStatistic.Value = (keyCount, 10);
                        break;

                    default:
                        firstDifficultyStatistic.Label = BeatmapsetsStrings.ShowStatsCs;
                        firstDifficultyStatistic.Value = (rateAdjustedDifficulty.CircleSize, 10f);
                        break;
                }

                accuracyStatistic.Value = (rateAdjustedDifficulty.OverallDifficulty, 10f);
                hpDrainStatistic.Value = (rateAdjustedDifficulty.DrainRate, 10f);
                approachRateStatistic.Value = (rateAdjustedDifficulty.ApproachRate, 10f);
            }
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

        private partial class MapperLinkContainer : OsuHoverContainer
        {
            [BackgroundDependencyLoader]
            private void load(OverlayColourProvider? overlayColourProvider, OsuColour colours)
            {
                TooltipText = ContextMenuStrings.ViewProfile;
                IdleColour = overlayColourProvider?.Light2 ?? colours.Blue;
            }
        }
    }
}
