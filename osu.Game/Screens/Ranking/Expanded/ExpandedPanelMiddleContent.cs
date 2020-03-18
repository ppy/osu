// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Localisation;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Scoring;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Ranking.Expanded.Accuracy;
using osu.Game.Screens.Ranking.Expanded.Statistics;
using osuTK;

namespace osu.Game.Screens.Ranking.Expanded
{
    /// <summary>
    /// The content that appears in the middle section of the <see cref="ScorePanel"/>.
    /// </summary>
    public class ExpandedPanelMiddleContent : CompositeDrawable
    {
        private readonly ScoreInfo score;

        private readonly List<StatisticDisplay> statisticDisplays = new List<StatisticDisplay>();
        private RollingCounter<long> scoreCounter;

        /// <summary>
        /// Creates a new <see cref="ExpandedPanelMiddleContent"/>.
        /// </summary>
        /// <param name="score">The score to display.</param>
        public ExpandedPanelMiddleContent(ScoreInfo score)
        {
            this.score = score;

            RelativeSizeAxes = Axes.Both;
            Masking = true;

            Padding = new MarginPadding { Vertical = 10, Horizontal = 10 };
        }

        [BackgroundDependencyLoader]
        private void load(Bindable<WorkingBeatmap> working)
        {
            var beatmap = working.Value.BeatmapInfo;
            var metadata = beatmap.Metadata;
            var creator = metadata.Author?.Username;

            var topStatistics = new List<StatisticDisplay>
            {
                new AccuracyStatistic(score.Accuracy),
                new ComboStatistic(score.MaxCombo, true),
                new CounterStatistic("pp", (int)(score.PP ?? 0)),
            };

            var bottomStatistics = new List<StatisticDisplay>();
            foreach (var stat in score.SortedStatistics)
                bottomStatistics.Add(new CounterStatistic(stat.Key.GetDescription(), stat.Value));

            statisticDisplays.AddRange(topStatistics);
            statisticDisplays.AddRange(bottomStatistics);

            InternalChild = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = FillDirection.Vertical,
                Spacing = new Vector2(20),
                Children = new Drawable[]
                {
                    new FillFlowContainer
                    {
                        Anchor = Anchor.TopCentre,
                        Origin = Anchor.TopCentre,
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Children = new Drawable[]
                        {
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = new LocalisedString((metadata.Title, metadata.TitleUnicode)),
                                Font = OsuFont.Torus.With(size: 20, weight: FontWeight.SemiBold),
                            },
                            new OsuSpriteText
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Text = new LocalisedString((metadata.Artist, metadata.ArtistUnicode)),
                                Font = OsuFont.Torus.With(size: 14, weight: FontWeight.SemiBold)
                            },
                            new Container
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Margin = new MarginPadding { Top = 40 },
                                RelativeSizeAxes = Axes.X,
                                Height = 230,
                                Child = new AccuracyCircle(score)
                                {
                                    Anchor = Anchor.Centre,
                                    Origin = Anchor.Centre,
                                    RelativeSizeAxes = Axes.Both,
                                    FillMode = FillMode.Fit,
                                }
                            },
                            scoreCounter = new TotalScoreCounter
                            {
                                Margin = new MarginPadding { Top = 0, Bottom = 5 },
                                Current = { Value = 0 },
                                Alpha = 0,
                                AlwaysPresent = true
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new StarRatingDisplay(beatmap)
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft
                                    },
                                    new ModDisplay
                                    {
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        DisplayUnrankedText = false,
                                        ExpansionMode = ExpansionMode.AlwaysExpanded,
                                        Scale = new Vector2(0.5f),
                                        Current = { Value = score.Mods }
                                    }
                                }
                            },
                            new FillFlowContainer
                            {
                                Anchor = Anchor.TopCentre,
                                Origin = Anchor.TopCentre,
                                Direction = FillDirection.Vertical,
                                AutoSizeAxes = Axes.Both,
                                Children = new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Text = beatmap.Version,
                                        Font = OsuFont.Torus.With(size: 16, weight: FontWeight.SemiBold),
                                    },
                                    new OsuTextFlowContainer(s => s.Font = OsuFont.Torus.With(size: 16))
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        AutoSizeAxes = Axes.Both,
                                        Direction = FillDirection.Horizontal,
                                    }.With(t =>
                                    {
                                        t.AddText("作图者：");
                                        t.AddText(beatmap.Metadata.Author?.Username, s => s.Font = s.Font.With(weight: FontWeight.SemiBold));
                                    }),
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.TopCentre,
                                        Origin = Anchor.TopCentre,
                                        Font = OsuFont.GetFont(size: 16, weight: FontWeight.SemiBold),
                                        Text = $"于 {score.Date.ToLocalTime():g} 游玩"
                                    }
                                }
                            },
                        }
                    },
                    new FillFlowContainer
                    {
                        RelativeSizeAxes = Axes.X,
                        AutoSizeAxes = Axes.Y,
                        Direction = FillDirection.Vertical,
                        Spacing = new Vector2(0, 5),
                        Children = new Drawable[]
                        {
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Content = new[] { topStatistics.Cast<Drawable>().ToArray() },
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                }
                            },
                            new GridContainer
                            {
                                RelativeSizeAxes = Axes.X,
                                AutoSizeAxes = Axes.Y,
                                Content = new[] { bottomStatistics.Cast<Drawable>().ToArray() },
                                RowDimensions = new[]
                                {
                                    new Dimension(GridSizeMode.AutoSize),
                                }
                            }
                        }
                    }
                }
            };
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            // Score counter value setting must be scheduled so it isn't transferred instantaneously
            ScheduleAfterChildren(() =>
            {
                using (BeginDelayedSequence(AccuracyCircle.ACCURACY_TRANSFORM_DELAY, true))
                {
                    scoreCounter.FadeIn();
                    scoreCounter.Current.Value = score.TotalScore;

                    double delay = 0;

                    foreach (var stat in statisticDisplays)
                    {
                        using (BeginDelayedSequence(delay, true))
                            stat.Appear();

                        delay += 200;
                    }
                }
            });
        }
    }
}