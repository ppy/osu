// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Drawables;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Overlays;
using osu.Game.Scoring;
using osu.Game.Screens.Select;
using osuTK;

namespace osu.Game.Screens.RankingV2.Extended
{
    public partial class BeatmapInfoPanel : CompositeDrawable
    {
        private UpdateableBeatmapBackgroundSprite beatmapCover = null!;
        private BeatmapSetOnlineStatusPill statusPill = null!;
        private TruncatingSpriteText titleText = null!;
        private TruncatingSpriteText authorText = null!;
        private Container statsContainer = null!;
        private StarRatingDisplay starRatingDisplay = null!;
        private OsuTextFlowContainer difficultyNameFlow = null!;
        private FillFlowContainer<BeatmapTitleWedge.StatisticDifficulty> statisticsFlow = null!;

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private BeatmapDifficultyCache difficultyCache { get; set; } = null!;

        private CancellationTokenSource? cancellationTokenSource;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider)
        {
            RelativeSizeAxes = Axes.X;
            Height = 85;
            Masking = true;
            CornerRadius = 20;

            InternalChildren =
            [
                beatmapCover = new UpdateableBeatmapBackgroundSprite
                {
                    RelativeSizeAxes = Axes.Both,
                },
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = ColourInfo.GradientHorizontal(Colour4.Black.Opacity(0.5f), Colour4.Black.Opacity(0.2f))
                },
                new GridContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    Padding = new MarginPadding(5),
                    ColumnDimensions =
                    [
                        new Dimension(),
                        new Dimension(GridSizeMode.AutoSize),
                    ],
                    RowDimensions = [new Dimension()],
                    Content = new[]
                    {
                        new Drawable[]
                        {
                            new FillFlowContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Direction = FillDirection.Vertical,
                                Anchor = Anchor.CentreLeft,
                                Origin = Anchor.CentreLeft,
                                Padding = new MarginPadding
                                {
                                    Left = 15,
                                    Vertical = 5,
                                },
                                Children =
                                [
                                    statusPill = new BeatmapSetOnlineStatusPill
                                    {
                                        Animated = false,
                                    },
                                    titleText = new TruncatingSpriteText
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Font = OsuFont.Style.Title,
                                    },
                                    authorText = new TruncatingSpriteText
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        Font = OsuFont.Style.Heading2,
                                    },
                                ],
                            },
                            statsContainer = new Container
                            {
                                AutoSizeAxes = Axes.X,
                                RelativeSizeAxes = Axes.Y,
                                Masking = true,
                                AlwaysPresent = true,
                                CornerRadius = 15,
                                Children =
                                [
                                    new Box
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Colour = colourProvider.Background6,
                                        Alpha = 0.8f,
                                    },
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Anchor = Anchor.TopLeft,
                                        Origin = Anchor.TopLeft,
                                        Padding = new MarginPadding
                                        {
                                            Horizontal = 15,
                                            Vertical = 5,
                                        },
                                        Spacing = new Vector2(5),
                                        Children =
                                        [
                                            starRatingDisplay = new StarRatingDisplay(new StarDifficulty())
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                            },
                                            difficultyNameFlow = new OsuTextFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                            }
                                        ],
                                    },
                                    statisticsFlow = new FillFlowContainer<BeatmapTitleWedge.StatisticDifficulty>
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        AlwaysPresent = true,
                                        Anchor = Anchor.BottomLeft,
                                        Origin = Anchor.BottomLeft,
                                        Padding = new MarginPadding
                                        {
                                            Horizontal = 15,
                                            Vertical = 5,
                                        },
                                        Spacing = new Vector2(5),
                                    }
                                ],
                            },
                        },
                    },
                },
            ];
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            score.BindValueChanged(_ => updateState(), true);
        }

        private void updateState()
        {
            Debug.Assert(score.Value.Beatmap != null);

            beatmapCover.Beatmap.Value = score.Value.Beatmap;
            statusPill.Status = score.Value.Beatmap.Status;

            titleText.Text = new RomanisableString(score.Value.Beatmap.Metadata.TitleUnicode, score.Value.Beatmap.Metadata.Title);
            authorText.Text = new RomanisableString(score.Value.Beatmap.Metadata.ArtistUnicode, score.Value.Beatmap.Metadata.Artist);

            cancellationTokenSource?.Cancel();
            cancellationTokenSource = new CancellationTokenSource();

            var rulesetInstance = score.Value.Ruleset.CreateInstance();
            var mods = score.Value.Mods.Select(m => m.ToMod(rulesetInstance)).ToArray();

            difficultyCache.GetDifficultyAsync(score.Value.Beatmap, score.Value.Ruleset, mods, cancellationTokenSource.Token)
                           .ContinueWith(t =>
                           {
                               var difficulty = t.GetResultSafely() ?? new StarDifficulty(score.Value.Beatmap.StarRating, 0);
                               Schedule(() =>
                               {
                                   starRatingDisplay.Current.Value = difficulty;
                                   applyStarRatingColour();
                               });
                           }, TaskContinuationOptions.OnlyOnRanToCompletion);

            Task.Run(() =>
            {
                var token = cancellationTokenSource.Token;

                var playableBeatmap = beatmap.Value.GetPlayableBeatmap(score.Value.Ruleset, mods, token);
                var hitObjectCounts = playableBeatmap.GetStatistics()
                                                     .Select(s => new BeatmapTitleWedge.StatisticDifficulty
                                                     {
                                                         Value = new BeatmapTitleWedge.StatisticDifficulty.Data(s.Name, s.BarDisplayLength ?? 0, s.BarDisplayLength ?? 0, 1, s.Content),
                                                     })
                                                     .ToArray();
                var difficultyStats = rulesetInstance.GetBeatmapAttributesForDisplay(score.Value.Beatmap, mods)
                                                     .Select(s => new BeatmapTitleWedge.StatisticDifficulty
                                                     {
                                                         Value = new BeatmapTitleWedge.StatisticDifficulty.Data(s),
                                                     })
                                                     .ToArray();
                difficultyStats.First().Margin = new MarginPadding { Left = 30, };

                Schedule(() =>
                {
                    if (token.IsCancellationRequested)
                        return;

                    statsContainer.FadeOut();

                    difficultyNameFlow.Clear();
                    difficultyNameFlow.AddText(score.Value.Beatmap.DifficultyName, t => t.Font = OsuFont.Style.Heading2);
                    difficultyNameFlow.AddText(" mapped by ");
                    difficultyNameFlow.AddText(score.Value.Beatmap.Metadata.Author.Username, t => t.Font = OsuFont.Default.With(weight: FontWeight.SemiBold));

                    statisticsFlow.Clear();
                    statisticsFlow.AddRange(hitObjectCounts);
                    statisticsFlow.AddRange(difficultyStats);

                    applyStarRatingColour();

                    ScheduleAfterChildren(() =>
                    {
                        float statisticWidth = Math.Max(65, statisticsFlow.Max(s => s.LabelWidth));

                        foreach (var statistic in statisticsFlow)
                            statistic.Width = statisticWidth;

                        // fade-in hides a one-frame autosize issue
                        statsContainer.FadeIn(100, Easing.OutQuint);
                    });
                });
            }, cancellationTokenSource.Token);
        }

        private void applyStarRatingColour()
        {
            var col = starRatingDisplay.ForegroundTextColour;
            difficultyNameFlow.Colour = col;
            foreach (var statistic in statisticsFlow.OfType<BeatmapTitleWedge.StatisticDifficulty>())
                statistic.AccentColour = col;
        }
    }
}
