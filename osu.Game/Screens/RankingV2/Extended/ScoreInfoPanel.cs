// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Extensions.LocalisationExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game.Configuration;
using osu.Game.Beatmaps;
using osu.Game.Graphics;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Localisation;
using osu.Game.Online.Leaderboards;
using osu.Game.Overlays;
using osu.Game.Resources.Localisation.Web;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.RankingV2.Argon;
using osu.Game.Skinning;
using osu.Game.Utils;
using osuTK;

namespace osu.Game.Screens.RankingV2.Extended
{
    public partial class ScoreInfoPanel : CompositeDrawable
    {
        public BindableBool ShowGlobalStatistics { get; } = new BindableBool(true);

        private Sprite rankSprite = null!;
        private OsuSpriteText totalScoreText = null!;
        private FillFlowContainer modsFlow = null!;
        private ModDisplay modDisplay = null!;
        private OsuSpriteText modMultiplierSpriteText = null!;
        private FillFlowContainer<StatisticValue> basicHitStatisticsFlow = null!;
        private FillFlowContainer<StatisticValue> nonBasicHitStatisticsFlow = null!;
        private FillFlowContainer modMultiplierFlow = null!;
        private LoadingLayer loadingLayer = null!;
        private FillFlowContainer leftStatisticsColumn = null!;
        private FillFlowContainer rightStatisticsColumn = null!;
        private GlobalStatisticsUpdatePanel globalStatistics = null!;

        private CancellationTokenSource? loadCancellation;

        private Bindable<ScoringMode> scoringMode { get; } = new Bindable<ScoringMode>();

        [Resolved]
        private IBindable<WorkingBeatmap> beatmap { get; set; } = null!;

        [Resolved]
        private IBindable<IScoreInfo> score { get; set; } = null!;

        [Resolved]
        private SkinManager skins { get; set; } = null!;

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load(OverlayColourProvider colourProvider, OsuColour colours, OsuConfigManager configManager)
        {
            RelativeSizeAxes = Axes.X;
            AutoSizeAxes = Axes.Y;
            Masking = true;
            CornerRadius = 20;

            InternalChildren =
            [
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
                    Children =
                    [
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            ColumnDimensions =
                            [
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(),
                            ],
                            RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        AutoSizeAxes = Axes.Both,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(15),
                                        Padding = new MarginPadding
                                        {
                                            Vertical = 5,
                                            Left = 15,
                                        },
                                        Children =
                                        [
                                            new Container
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Size = new Vector2(50, 60),
                                                Child = rankSprite = new Sprite
                                                {
                                                    RelativeSizeAxes = Axes.Both,
                                                    FillMode = FillMode.Fit,
                                                    Anchor = Anchor.Centre,
                                                    Origin = Anchor.Centre,
                                                },
                                            },
                                            totalScoreText = new OsuSpriteText
                                            {
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Font = OsuFont.TorusAlternate.With(size: 54, weight: FontWeight.Light),
                                                Colour = ArgonTotalScorePanel.TEXT_GRADIENT,
                                                UseFullGlyphHeight = false,
                                            },
                                            modsFlow = new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Direction = FillDirection.Vertical,
                                                Children =
                                                [
                                                    new OsuSpriteText
                                                    {
                                                        Text = BeatmapsetsStrings.ShowScoreboardHeadersMods.ToUpper(),
                                                        Font = OsuFont.Default.With(weight: FontWeight.Bold),
                                                        Colour = colourProvider.Content2,
                                                    },
                                                    modDisplay = new ModDisplay
                                                    {
                                                        ExpansionMode = ExpansionMode.AlwaysContracted,
                                                        Scale = new Vector2(0.6f),
                                                    }
                                                ]
                                            },
                                            modMultiplierFlow = new FillFlowContainer
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Anchor = Anchor.CentreLeft,
                                                Origin = Anchor.CentreLeft,
                                                Direction = FillDirection.Vertical,
                                                Children =
                                                [
                                                    new OsuSpriteText
                                                    {
                                                        Text = ResultsScreenStrings.ModScoreMultiplierCaption.ToUpper(),
                                                        Font = OsuFont.Default.With(weight: FontWeight.Bold),
                                                        Colour = colourProvider.Content2,
                                                    },
                                                    modMultiplierSpriteText = new OsuSpriteText
                                                    {
                                                        Font = OsuFont.Default.With(size: 28),
                                                        Colour = ColourInfo.GradientVertical(colours.Red0, colours.Red1),
                                                    },
                                                ]
                                            },
                                        ],
                                    },
                                    new FillFlowContainer
                                    {
                                        RelativeSizeAxes = Axes.X,
                                        AutoSizeAxes = Axes.Y,
                                        Anchor = Anchor.CentreLeft,
                                        Origin = Anchor.CentreLeft,
                                        Spacing = new Vector2(10, 0),
                                        Padding = new MarginPadding
                                        {
                                            Vertical = 5,
                                            Horizontal = 15,
                                        },
                                        Children =
                                        [
                                            nonBasicHitStatisticsFlow = new FillFlowContainer<StatisticValue>
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Anchor = Anchor.BottomRight,
                                                Origin = Anchor.BottomRight,
                                                Spacing = new Vector2(10, 0),
                                            },
                                            basicHitStatisticsFlow = new FillFlowContainer<StatisticValue>
                                            {
                                                AutoSizeAxes = Axes.Both,
                                                Anchor = Anchor.BottomRight,
                                                Origin = Anchor.BottomRight,
                                                Spacing = new Vector2(10, 0),
                                            },
                                        ],
                                    },
                                },
                            },
                        },
                        new Container
                        {
                            RelativeSizeAxes = Axes.X,
                            AutoSizeAxes = Axes.Y,
                            CornerRadius = 20,
                            Masking = true,
                            Children =
                            [
                                new Box
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    Colour = colourProvider.Background5,
                                },
                                new GridContainer
                                {
                                    Padding = new MarginPadding(15),
                                    RelativeSizeAxes = Axes.X,
                                    AutoSizeAxes = Axes.Y,
                                    ColumnDimensions =
                                    [
                                        new Dimension(GridSizeMode.Absolute, 300),
                                        new Dimension(GridSizeMode.Absolute, 10),
                                        new Dimension(),
                                    ],
                                    RowDimensions = [new Dimension(GridSizeMode.AutoSize)],
                                    Content = new[]
                                    {
                                        new[]
                                        {
                                            leftStatisticsColumn = new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Spacing = new Vector2(10),
                                                Direction = FillDirection.Vertical,
                                                Children =
                                                [
                                                    globalStatistics = new GlobalStatisticsUpdatePanel(),
                                                ]
                                            },
                                            Empty(),
                                            rightStatisticsColumn = new FillFlowContainer
                                            {
                                                RelativeSizeAxes = Axes.X,
                                                AutoSizeAxes = Axes.Y,
                                                Spacing = new Vector2(0, 10),
                                                Direction = FillDirection.Full,
                                            },
                                        },
                                    },
                                },
                                loadingLayer = new LoadingLayer(dimBackground: true)
                                {
                                    RelativeSizeAxes = Axes.Both,
                                    State = { Value = Visibility.Hidden },
                                },
                            ]
                        }
                    ],
                },
            ];

            configManager.BindWith(OsuSetting.ScoreDisplayMode, scoringMode);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            scoringMode.BindValueChanged(_ => updateTotalScore());
            score.BindValueChanged(_ => updateScore(), true);
            ShowGlobalStatistics.BindValueChanged(val => globalStatistics.Alpha = val.NewValue ? 1 : 0, true);
        }

        private void updateScore()
        {
            rankSprite.Texture = skins.DefaultClassicSkin.GetTexture(DrawableRank.GetLegacyRankTextureName(score.Value.Rank));
            updateTotalScore();

            var rulesetInstance = score.Value.Ruleset.CreateInstance();
            var scoreMultiplierCalculator = rulesetInstance.CreateScoreMultiplierCalculator(new ScoreMultiplierContext(score.Value.Beatmap!.Difficulty));
            var mods = score.Value.Mods.Select(m => m.ToMod(rulesetInstance)).ToArray();

            modsFlow.Alpha = modMultiplierFlow.Alpha = mods.Length > 0 ? 1 : 0;

            modDisplay.Current.Value = mods;
            double scoreMultiplier = scoreMultiplierCalculator.CalculateFor(mods);
            modMultiplierSpriteText.Text = ModUtils.FormatScoreMultiplier(scoreMultiplier);
            modMultiplierSpriteText.Colour = scoreMultiplier switch
            {
                < 1 => ColourInfo.GradientVertical(colours.Lime0, colours.Lime1),
                > 1 => ColourInfo.GradientVertical(colours.Red0, colours.Red1),
                _ => ArgonTotalScorePanel.TEXT_GRADIENT,
            };

            basicHitStatisticsFlow.Clear();
            nonBasicHitStatisticsFlow.Clear();

            var statistics = score.Value.GetStatisticsForDisplay();

            foreach (var statistic in statistics.Reverse())
            {
                (statistic.Result.IsBasic() ? basicHitStatisticsFlow : nonBasicHitStatisticsFlow).Add(new StatisticValue
                {
                    AccentColour = statistic.Result.IsBasic() ? colours.ForHitResult(statistic.Result) : null,
                    Caption = statistic.DisplayName,
                    Value = statistic.Count.ToLocalisableString(@"N0"),
                    MaxValue = statistic.MaxCount?.ToLocalisableString(@"N0") ?? default,
                    Anchor = Anchor.BottomRight,
                    Origin = Anchor.BottomRight,
                });
            }

            loadingLayer.Show();

            loadCancellation?.Cancel();
            loadCancellation = new CancellationTokenSource();
            Task.Run(() => beatmap.Value.GetPlayableBeatmap(score.Value.Ruleset, mods), loadCancellation.Token).ContinueWith(task => Schedule(() =>
            {
                var playableBeatmap = task.GetResultSafely();
                var rulesetContent = rulesetInstance.CreateStatisticsForResults(score.Value, playableBeatmap);

                int loadsCompleted = 0;

                var leftStatistics = rulesetContent.LeftColumn.Invoke().ToArray();
                var rightStatistics = rulesetContent.RightColumn.Invoke().ToArray();

                if (leftStatistics.Length > 0)
                {
                    LoadComponentsAsync(leftStatistics, loaded =>
                    {
                        leftStatisticsColumn.RemoveAll(r => r is not GlobalStatisticsUpdatePanel, true);
                        leftStatisticsColumn.AddRange(loaded);
                        completeLoad();
                    });
                }
                else
                {
                    leftStatisticsColumn.RemoveAll(r => r is not GlobalStatisticsUpdatePanel, true);
                    completeLoad();
                }

                if (rightStatistics.Length > 0)
                {
                    LoadComponentsAsync(rightStatistics, loaded =>
                    {
                        rightStatisticsColumn.Clear();
                        rightStatisticsColumn.AddRange(loaded);
                        completeLoad();
                    });
                }
                else
                {
                    rightStatisticsColumn.Clear();
                    completeLoad();
                }

                void completeLoad()
                {
                    // ReSharper disable once AccessToModifiedClosure
                    if (Interlocked.Increment(ref loadsCompleted) == 2)
                        loadingLayer.Hide();
                }
            }), TaskContinuationOptions.OnlyOnRanToCompletion);

            ScheduleAfterChildren(() =>
            {
                unifyWidths(basicHitStatisticsFlow);
                unifyWidths(nonBasicHitStatisticsFlow);
            });
        }

        private void unifyWidths(FillFlowContainer<StatisticValue> flow)
        {
            if (flow.Count == 0)
                return;

            float maxWidth = flow.Max(d => d.DrawWidth);

            foreach (var d in flow)
            {
                d.AutoSizeAxes = Axes.Y;
                d.Width = maxWidth;
            }
        }

        private void updateTotalScore()
        {
            totalScoreText.Text = score.Value.GetDisplayScore(scoringMode.Value).ToLocalisableString(@"N0");
        }
    }
}
