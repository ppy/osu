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
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Cursor;
using osu.Framework.Graphics.Shapes;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Configuration;
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

        private ModSettingChangeTracker? settingChangeTracker;

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
        private WedgeDifficultyStatisticsDisplay countStatisticsDisplay = null!;
        private AdjustableDifficultyStatisticsDisplay difficultyStatisticsDisplay = null!;

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
                    Colour = colourProvider.Background4.Opacity(0.6f),
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
                            AutoSizeAxes = Axes.Y,
                            Margin = new MarginPadding { Vertical = 5f },
                            Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN },
                            RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
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
                                                Font = OsuFont.Body.With(weight: FontWeight.SemiBold),
                                            },
                                            mappedByText = new OsuSpriteText
                                            {
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                Text = " mapped by ",
                                                Font = OsuFont.Body,
                                            },
                                            mapperLink = new MapperLinkContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Anchor = Anchor.BottomLeft,
                                                Origin = Anchor.BottomLeft,
                                                Child = mapperText = new TruncatingSpriteText
                                                {
                                                    Shadow = true,
                                                    Font = OsuFont.Body.With(weight: FontWeight.SemiBold),
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
                                        Colour = colourProvider.Background5.Opacity(0.8f),
                                    },
                                    new GridContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Padding = new MarginPadding { Left = SongSelect.WEDGE_CONTENT_MARGIN, Right = 20f, Vertical = 7.5f },
                                        Shear = -shear,
                                        RowDimensions = new[] { new Dimension(GridSizeMode.AutoSize) },
                                        ColumnDimensions = new[]
                                        {
                                            new Dimension(),
                                            new Dimension(GridSizeMode.Absolute, 30),
                                            new Dimension(GridSizeMode.AutoSize),
                                        },
                                        Content = new[]
                                        {
                                            new[]
                                            {
                                                countStatisticsDisplay = new WedgeDifficultyStatisticsDisplay
                                                {
                                                    RelativeSizeAxes = Axes.X,
                                                },
                                                Empty(),
                                                difficultyStatisticsDisplay = new AdjustableDifficultyStatisticsDisplay(autoSize: true),
                                            }
                                        },
                                    }
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

            mods.BindValueChanged(m =>
            {
                settingChangeTracker?.Dispose();

                updateDifficultyStatistics();

                settingChangeTracker = new ModSettingChangeTracker(m.NewValue);
                settingChangeTracker.SettingChanged += _ => updateDifficultyStatistics();
            });

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

            countStatisticsDisplay.Statistics = playableBeatmap.GetStatistics()
                                                               .Select(s => new WedgeStatisticDifficulty.Data(s.Name, s.Value, s.Value, s.Maximum))
                                                               .ToList();

            updateDifficultyStatistics();
        }

        private void updateDifficultyStatistics() => Scheduler.AddOnce(() =>
        {
            BeatmapDifficulty baseDifficulty = beatmap.Value.BeatmapInfo.Difficulty;
            BeatmapDifficulty originalDifficulty = new BeatmapDifficulty(baseDifficulty);

            foreach (var mod in mods.Value.OfType<IApplicableToDifficulty>())
                mod.ApplyToDifficulty(originalDifficulty);

            var rateAdjustedDifficulty = originalDifficulty;

            if (ruleset.Value != null)
            {
                double rate = ModUtils.CalculateRateWithMods(mods.Value);

                rateAdjustedDifficulty = ruleset.Value.CreateInstance().GetRateAdjustedDisplayDifficulty(originalDifficulty, rate);
                difficultyStatisticsDisplay.TooltipContent = new AdjustedAttributesTooltip.Data(originalDifficulty, rateAdjustedDifficulty);
            }

            WedgeStatisticDifficulty.Data firstStatistic;

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

                    firstStatistic = new WedgeStatisticDifficulty.Data(BeatmapsetsStrings.ShowStatsCsMania, keyCount, keyCount, 10);
                    break;

                default:
                    firstStatistic = new WedgeStatisticDifficulty.Data(BeatmapsetsStrings.ShowStatsCs, baseDifficulty.CircleSize, rateAdjustedDifficulty.CircleSize, 10);
                    break;
            }

            difficultyStatisticsDisplay.Statistics = new[]
            {
                firstStatistic,
                new WedgeStatisticDifficulty.Data(BeatmapsetsStrings.ShowStatsAccuracy, baseDifficulty.OverallDifficulty, rateAdjustedDifficulty.OverallDifficulty, 10),
                new WedgeStatisticDifficulty.Data(BeatmapsetsStrings.ShowStatsDrain, baseDifficulty.DrainRate, rateAdjustedDifficulty.DrainRate, 10),
                new WedgeStatisticDifficulty.Data(BeatmapsetsStrings.ShowStatsAr, baseDifficulty.ApproachRate, rateAdjustedDifficulty.ApproachRate, 10),
            };
        });

        private void updateStars()
        {
            starRatingDisplay.Current.Value = new StarDifficulty(displayedStars.Value, 0);

            Color4 colour = displayedStars.Value >= 6.5f ? colours.Orange1 : colours.ForStarDifficulty(displayedStars.Value);
            difficultyText.FadeColour(colour, 300, Easing.OutQuint);
            mappedByText.FadeColour(colour, 300, Easing.OutQuint);
            countStatisticsDisplay.TransformTo(nameof(countStatisticsDisplay.AccentColour), colour, 300, Easing.OutQuint);
            difficultyStatisticsDisplay.TransformTo(nameof(difficultyStatisticsDisplay.AccentColour), colour, 300, Easing.OutQuint);
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

        private partial class AdjustableDifficultyStatisticsDisplay : WedgeDifficultyStatisticsDisplay, IHasCustomTooltip<AdjustedAttributesTooltip.Data>
        {
            [Resolved]
            private OverlayColourProvider colourProvider { get; set; } = null!;

            public ITooltip<AdjustedAttributesTooltip.Data> GetCustomTooltip() => new AdjustedAttributesTooltip(colourProvider);

            public AdjustedAttributesTooltip.Data? TooltipContent { get; set; }

            public AdjustableDifficultyStatisticsDisplay(bool autoSize)
                : base(autoSize)
            {
            }
        }
    }
}
