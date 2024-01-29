// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics.UserInterface;
using osu.Game.Online.API;
using osu.Game.Rulesets;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Expanded.Statistics;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Input;
using Realms;

namespace osu.Game.Tests.Visual.Ranking
{
    [TestFixture]
    public partial class TestSceneResultsScreen : OsuManualInputManagerTestScene
    {
        [Resolved]
        private BeatmapManager beatmaps { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            realm.Run(r =>
            {
                var beatmapInfo = r.All<BeatmapInfo>()
                                   .Filter($"{nameof(BeatmapInfo.Ruleset)}.{nameof(RulesetInfo.OnlineID)} = $0", 0)
                                   .FirstOrDefault();

                if (beatmapInfo != null)
                    Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmapInfo);
            });
        }

        [Test]
        public void TestScaling()
        {
            // scheduling is needed as scaling the content immediately causes the entire scene to shake badly, for some odd reason.
            AddSliderStep("scale", 0.5f, 1.6f, 1f, v => Schedule(() =>
            {
                Content.Scale = new Vector2(v);
                Content.Size = new Vector2(1f / v);
            }));
        }

        private int onlineScoreID = 1;

        [TestCase(1, ScoreRank.X, 0)]
        [TestCase(0.9999, ScoreRank.S, 0)]
        [TestCase(0.975, ScoreRank.S, 0)]
        [TestCase(0.975, ScoreRank.A, 1)]
        [TestCase(0.925, ScoreRank.A, 5)]
        [TestCase(0.85, ScoreRank.B, 9)]
        [TestCase(0.75, ScoreRank.C, 11)]
        [TestCase(0.5, ScoreRank.D, 21)]
        [TestCase(0.2, ScoreRank.D, 51)]
        public void TestResultsWithPlayer(double accuracy, ScoreRank rank, int missCount)
        {
            TestResultsScreen screen = null;

            loadResultsScreen(() =>
            {
                var score = TestResources.CreateTestScoreInfo();

                score.OnlineID = onlineScoreID++;
                score.HitEvents = TestSceneStatisticsPanel.CreatePositionDistributedHitEvents();
                score.Accuracy = accuracy;
                score.Rank = rank;
                score.Statistics[HitResult.Miss] = missCount;

                return screen = createResultsScreen(score);
            });
            AddUntilStep("wait for loaded", () => screen.IsLoaded);
            AddAssert("retry overlay present", () => screen.RetryOverlay != null);
        }

        [Test]
        public void TestResultsWithoutPlayer()
        {
            TestResultsScreen screen = null;
            OsuScreenStack stack;

            AddStep("load results", () =>
            {
                Child = stack = new OsuScreenStack
                {
                    RelativeSizeAxes = Axes.Both
                };

                var score = TestResources.CreateTestScoreInfo();

                stack.Push(screen = createResultsScreen(score));
            });
            AddUntilStep("wait for loaded", () => screen.IsLoaded);
            AddAssert("retry overlay not present", () => screen.RetryOverlay == null);
        }

        [Test]
        public void TestResultsForUnranked()
        {
            UnrankedSoloResultsScreen screen = null;

            loadResultsScreen(() => screen = createUnrankedSoloResultsScreen());
            AddUntilStep("wait for loaded", () => screen.IsLoaded);
            AddAssert("retry overlay present", () => screen.RetryOverlay != null);
        }

        [Test]
        public void TestShowHideStatisticsViaOutsideClick()
        {
            TestResultsScreen screen = null;

            loadResultsScreen(() => screen = createResultsScreen());
            AddUntilStep("wait for load", () => this.ChildrenOfType<ScorePanelList>().Single().AllPanelsVisible);

            AddStep("click expanded panel", () =>
            {
                var expandedPanel = this.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded);
                InputManager.MoveMouseTo(expandedPanel);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("statistics shown", () => this.ChildrenOfType<StatisticsPanel>().Single().State.Value == Visibility.Visible);

            AddUntilStep("expanded panel at the left of the screen", () =>
            {
                var expandedPanel = this.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded);
                return expandedPanel.ScreenSpaceDrawQuad.TopLeft.X - screen.ScreenSpaceDrawQuad.TopLeft.X < 150;
            });

            AddStep("click to right of panel", () =>
            {
                var expandedPanel = this.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded);
                InputManager.MoveMouseTo(expandedPanel.ScreenSpaceDrawQuad.TopRight + new Vector2(50, 0));
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("statistics hidden", () => this.ChildrenOfType<StatisticsPanel>().Single().State.Value == Visibility.Hidden);

            AddUntilStep("expanded panel in centre of screen", () =>
            {
                var expandedPanel = this.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded);
                return Precision.AlmostEquals(expandedPanel.ScreenSpaceDrawQuad.Centre.X, screen.ScreenSpaceDrawQuad.Centre.X, 1);
            });
        }

        [Test]
        public void TestShowHideStatistics()
        {
            TestResultsScreen screen = null;

            loadResultsScreen(() => screen = createResultsScreen());
            AddUntilStep("wait for load", () => this.ChildrenOfType<ScorePanelList>().Single().AllPanelsVisible);

            AddStep("click expanded panel", () =>
            {
                var expandedPanel = this.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded);
                InputManager.MoveMouseTo(expandedPanel);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("statistics shown", () => this.ChildrenOfType<StatisticsPanel>().Single().State.Value == Visibility.Visible);

            AddUntilStep("expanded panel at the left of the screen", () =>
            {
                var expandedPanel = this.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded);
                return expandedPanel.ScreenSpaceDrawQuad.TopLeft.X - screen.ScreenSpaceDrawQuad.TopLeft.X < 150;
            });

            AddStep("click expanded panel", () =>
            {
                var expandedPanel = this.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded);
                InputManager.MoveMouseTo(expandedPanel);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("statistics hidden", () => this.ChildrenOfType<StatisticsPanel>().Single().State.Value == Visibility.Hidden);

            AddUntilStep("expanded panel in centre of screen", () =>
            {
                var expandedPanel = this.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded);
                return Precision.AlmostEquals(expandedPanel.ScreenSpaceDrawQuad.Centre.X, screen.ScreenSpaceDrawQuad.Centre.X, 1);
            });
        }

        [Test]
        public void TestShowStatisticsAndClickOtherPanel()
        {
            TestResultsScreen screen = null;

            loadResultsScreen(() => screen = createResultsScreen());
            AddUntilStep("wait for load", () => this.ChildrenOfType<ScorePanelList>().Single().AllPanelsVisible);

            ScorePanel expandedPanel = null;
            ScorePanel contractedPanel = null;

            AddStep("click expanded panel then contracted panel", () =>
            {
                expandedPanel = this.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded);
                InputManager.MoveMouseTo(expandedPanel);
                InputManager.Click(MouseButton.Left);

                contractedPanel = this.ChildrenOfType<ScorePanel>().First(p => p.State == PanelState.Contracted && p.ScreenSpaceDrawQuad.TopLeft.X > screen.ScreenSpaceDrawQuad.TopLeft.X);
                InputManager.MoveMouseTo(contractedPanel);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("statistics shown", () => this.ChildrenOfType<StatisticsPanel>().Single().State.Value == Visibility.Visible);

            AddAssert("contracted panel still contracted", () => contractedPanel.State == PanelState.Contracted);
            AddAssert("expanded panel still expanded", () => expandedPanel.State == PanelState.Expanded);
        }

        [Test]
        public void TestFetchScoresAfterShowingStatistics()
        {
            DelayedFetchResultsScreen screen = null;

            var tcs = new TaskCompletionSource<bool>();

            loadResultsScreen(() => screen = new DelayedFetchResultsScreen(TestResources.CreateTestScoreInfo(), tcs.Task));

            AddUntilStep("wait for loaded", () => screen.IsLoaded);

            AddStep("click expanded panel", () =>
            {
                var expandedPanel = this.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded);
                InputManager.MoveMouseTo(expandedPanel);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("no fetch yet", () => !screen.FetchCompleted);

            AddStep("allow fetch", () => tcs.SetResult(true));

            AddUntilStep("wait for fetch", () => screen.FetchCompleted);
            AddAssert("expanded panel still on screen", () => this.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded).ScreenSpaceDrawQuad.TopLeft.X > 0);
        }

        [Test]
        public void TestDownloadButtonInitiallyDisabled()
        {
            TestResultsScreen screen = null;

            loadResultsScreen(() => screen = createResultsScreen());
            AddUntilStep("wait for load", () => this.ChildrenOfType<ScorePanelList>().Single().AllPanelsVisible);

            AddAssert("download button is disabled", () => !screen.ChildrenOfType<DownloadButton>().Last().Enabled.Value);

            AddStep("click contracted panel", () =>
            {
                var contractedPanel = this.ChildrenOfType<ScorePanel>().First(p => p.State == PanelState.Contracted && p.ScreenSpaceDrawQuad.TopLeft.X > screen.ScreenSpaceDrawQuad.TopLeft.X);
                InputManager.MoveMouseTo(contractedPanel);
                InputManager.Click(MouseButton.Left);
            });

            AddAssert("download button is enabled", () => screen.ChildrenOfType<DownloadButton>().Last().Enabled.Value);
        }

        [Test]
        public void TestRulesetWithNoPerformanceCalculator()
        {
            var ruleset = new RulesetWithNoPerformanceCalculator();
            var score = TestResources.CreateTestScoreInfo(ruleset.RulesetInfo);

            loadResultsScreen(() => createResultsScreen(score));
            AddUntilStep("wait for load", () => this.ChildrenOfType<ScorePanelList>().Single().AllPanelsVisible);

            AddAssert("PP displayed as 0", () =>
            {
                var performance = this.ChildrenOfType<PerformanceStatistic>().Single();
                var counter = performance.ChildrenOfType<StatisticCounter>().Single();
                return counter.Current.Value == 0;
            });
        }

        private void loadResultsScreen(Func<ResultsScreen> createResults)
        {
            ResultsScreen results = null;

            AddStep("load results", () => Child = new TestResultsContainer(results = createResults()));

            // expanded panel should be centered the moment results screen is loaded
            // but can potentially be scrolled away on certain specific load scenarios.
            // see: https://github.com/ppy/osu/issues/18226
            AddUntilStep("expanded panel in centre of screen", () =>
            {
                var expandedPanel = this.ChildrenOfType<ScorePanel>().Single(p => p.State == PanelState.Expanded);
                return Precision.AlmostEquals(expandedPanel.ScreenSpaceDrawQuad.Centre.X, results.ScreenSpaceDrawQuad.Centre.X, 1);
            });
        }

        private TestResultsScreen createResultsScreen(ScoreInfo score = null) => new TestResultsScreen(score ?? TestResources.CreateTestScoreInfo());

        private UnrankedSoloResultsScreen createUnrankedSoloResultsScreen() => new UnrankedSoloResultsScreen(TestResources.CreateTestScoreInfo());

        private partial class TestResultsContainer : Container
        {
            [Cached(typeof(Player))]
            private readonly Player player = new TestPlayer();

            public TestResultsContainer(IScreen screen)
            {
                RelativeSizeAxes = Axes.Both;
                OsuScreenStack stack;

                InternalChild = stack = new OsuScreenStack
                {
                    RelativeSizeAxes = Axes.Both,
                };

                stack.Push(screen);
            }
        }

        private partial class TestResultsScreen : SoloResultsScreen
        {
            public HotkeyRetryOverlay RetryOverlay;

            public TestResultsScreen(ScoreInfo score)
                : base(score, true)
            {
                ShowUserStatistics = true;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                RetryOverlay = InternalChildren.OfType<HotkeyRetryOverlay>().SingleOrDefault();
            }

            protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
            {
                var scores = new List<ScoreInfo>();

                for (int i = 0; i < 20; i++)
                {
                    var score = TestResources.CreateTestScoreInfo();
                    score.TotalScore += 10 - i;
                    score.HasOnlineReplay = true;
                    scores.Add(score);
                }

                scoresCallback?.Invoke(scores);

                return null;
            }
        }

        private partial class DelayedFetchResultsScreen : TestResultsScreen
        {
            private readonly Task fetchWaitTask;

            public bool FetchCompleted { get; private set; }

            public DelayedFetchResultsScreen(ScoreInfo score, Task fetchWaitTask = null)
                : base(score)
            {
                this.fetchWaitTask = fetchWaitTask ?? Task.CompletedTask;
            }

            protected override APIRequest FetchScores(Action<IEnumerable<ScoreInfo>> scoresCallback)
            {
                Task.Run(async () =>
                {
                    await fetchWaitTask;

                    var scores = new List<ScoreInfo>();

                    for (int i = 0; i < 20; i++)
                    {
                        var score = TestResources.CreateTestScoreInfo();
                        score.TotalScore += 10 - i;
                        scores.Add(score);
                    }

                    scoresCallback?.Invoke(scores);

                    Schedule(() => FetchCompleted = true);
                });

                return null;
            }
        }

        private partial class UnrankedSoloResultsScreen : SoloResultsScreen
        {
            public HotkeyRetryOverlay RetryOverlay;

            public UnrankedSoloResultsScreen(ScoreInfo score)
                : base(score, true)
            {
                Score!.BeatmapInfo!.OnlineID = 0;
                Score.BeatmapInfo.Status = BeatmapOnlineStatus.Pending;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                RetryOverlay = InternalChildren.OfType<HotkeyRetryOverlay>().SingleOrDefault();
            }
        }

        private class RulesetWithNoPerformanceCalculator : OsuRuleset
        {
            public override PerformanceCalculator CreatePerformanceCalculator() => null;
        }
    }
}
