// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Statistics;
using osu.Game.Tests.Resources;
using osuTK;
using osuTK.Input;
using Realms;

namespace osu.Game.Tests.Visual.Ranking
{
    [TestFixture]
    public class TestSceneResultsScreen : OsuManualInputManagerTestScene
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

                stack.Push(screen = createResultsScreen());
            });
            AddUntilStep("wait for loaded", () => screen.IsLoaded);
            AddAssert("retry overlay not present", () => screen.RetryOverlay == null);
        }

        [TestCase(0.2, ScoreRank.D)]
        [TestCase(0.5, ScoreRank.D)]
        [TestCase(0.75, ScoreRank.C)]
        [TestCase(0.85, ScoreRank.B)]
        [TestCase(0.925, ScoreRank.A)]
        [TestCase(0.975, ScoreRank.S)]
        [TestCase(0.9999, ScoreRank.S)]
        [TestCase(1, ScoreRank.X)]
        public void TestResultsWithPlayer(double accuracy, ScoreRank rank)
        {
            TestResultsScreen screen = null;

            var score = TestResources.CreateTestScoreInfo();

            score.Accuracy = accuracy;
            score.Rank = rank;

            AddStep("load results", () => Child = new TestResultsContainer(screen = createResultsScreen(score)));
            AddUntilStep("wait for loaded", () => screen.IsLoaded);
            AddAssert("retry overlay present", () => screen.RetryOverlay != null);
        }

        [Test]
        public void TestResultsForUnranked()
        {
            UnrankedSoloResultsScreen screen = null;

            AddStep("load results", () => Child = new TestResultsContainer(screen = createUnrankedSoloResultsScreen()));
            AddUntilStep("wait for loaded", () => screen.IsLoaded);
            AddAssert("retry overlay present", () => screen.RetryOverlay != null);
        }

        [Test]
        public void TestShowHideStatisticsViaOutsideClick()
        {
            TestResultsScreen screen = null;

            AddStep("load results", () => Child = new TestResultsContainer(screen = createResultsScreen()));
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
                InputManager.MoveMouseTo(expandedPanel.ScreenSpaceDrawQuad.TopRight + new Vector2(100, 0));
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

            AddStep("load results", () => Child = new TestResultsContainer(screen = createResultsScreen()));
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

            AddStep("load results", () => Child = new TestResultsContainer(screen = createResultsScreen()));
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

            AddStep("load results", () => Child = new TestResultsContainer(screen = new DelayedFetchResultsScreen(TestResources.CreateTestScoreInfo(), tcs.Task)));

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

            AddStep("load results", () => Child = new TestResultsContainer(screen = createResultsScreen()));
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

        private TestResultsScreen createResultsScreen(ScoreInfo score = null) => new TestResultsScreen(score ?? TestResources.CreateTestScoreInfo());

        private UnrankedSoloResultsScreen createUnrankedSoloResultsScreen() => new UnrankedSoloResultsScreen(TestResources.CreateTestScoreInfo());

        private class TestResultsContainer : Container
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

        private class TestResultsScreen : ResultsScreen
        {
            public HotkeyRetryOverlay RetryOverlay;

            public TestResultsScreen(ScoreInfo score)
                : base(score, true)
            {
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
                    score.Hash = $"test{i}";
                    scores.Add(score);
                }

                scoresCallback?.Invoke(scores);

                return null;
            }
        }

        private class DelayedFetchResultsScreen : TestResultsScreen
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

        private class UnrankedSoloResultsScreen : SoloResultsScreen
        {
            public HotkeyRetryOverlay RetryOverlay;

            public UnrankedSoloResultsScreen(ScoreInfo score)
                : base(score, true)
            {
                Score.BeatmapInfo.OnlineID = 0;
                Score.BeatmapInfo.Status = BeatmapOnlineStatus.Pending;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                RetryOverlay = InternalChildren.OfType<HotkeyRetryOverlay>().SingleOrDefault();
            }
        }
    }
}
