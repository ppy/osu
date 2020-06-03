// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;

namespace osu.Game.Tests.Visual.Ranking
{
    [TestFixture]
    public class TestSceneResultsScreen : ScreenTestScene
    {
        private BeatmapManager beatmaps;

        [BackgroundDependencyLoader]
        private void load(BeatmapManager beatmaps)
        {
            this.beatmaps = beatmaps;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            var beatmapInfo = beatmaps.QueryBeatmap(b => b.RulesetID == 0);
            if (beatmapInfo != null)
                Beatmap.Value = beatmaps.GetWorkingBeatmap(beatmapInfo);
        }

        private TestResultsScreen createResultsScreen() => new TestResultsScreen(new TestScoreInfo(new OsuRuleset().RulesetInfo));

        private UnrankedSoloResultsScreen createUnrankedSoloResultsScreen() => new UnrankedSoloResultsScreen(new TestScoreInfo(new OsuRuleset().RulesetInfo));

        [Test]
        public void ResultsWithoutPlayer()
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

        [Test]
        public void ResultsWithPlayer()
        {
            TestResultsScreen screen = null;

            AddStep("load results", () => Child = new TestResultsContainer(screen = createResultsScreen()));
            AddUntilStep("wait for loaded", () => screen.IsLoaded);
            AddAssert("retry overlay present", () => screen.RetryOverlay != null);
        }

        [Test]
        public void ResultsForUnranked()
        {
            UnrankedSoloResultsScreen screen = null;

            AddStep("load results", () => Child = new TestResultsContainer(screen = createUnrankedSoloResultsScreen()));
            AddUntilStep("wait for loaded", () => screen.IsLoaded);
            AddAssert("retry overlay present", () => screen.RetryOverlay != null);
        }

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
                : base(score)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                RetryOverlay = InternalChildren.OfType<HotkeyRetryOverlay>().SingleOrDefault();
            }
        }

        private class UnrankedSoloResultsScreen : SoloResultsScreen
        {
            public HotkeyRetryOverlay RetryOverlay;

            public UnrankedSoloResultsScreen(ScoreInfo score)
                : base(score)
            {
                Score.Beatmap.OnlineBeatmapID = 0;
                Score.Beatmap.Status = BeatmapSetOnlineStatus.Pending;
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                RetryOverlay = InternalChildren.OfType<HotkeyRetryOverlay>().SingleOrDefault();
            }
        }
    }
}
