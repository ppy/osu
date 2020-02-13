// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Scoring;
using osu.Game.Scoring;
using osu.Game.Screens;
using osu.Game.Screens.Play;
using osu.Game.Screens.Ranking;
using osu.Game.Screens.Ranking.Pages;
using osu.Game.Users;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneResults : ScreenTestScene
    {
        private BeatmapManager beatmaps;

        public override IReadOnlyList<Type> RequiredTypes => new[]
        {
            typeof(Results),
            typeof(ResultsPage),
            typeof(ScoreResultsPage),
            typeof(RetryButton),
            typeof(ReplayDownloadButton),
            typeof(LocalLeaderboardPage),
            typeof(TestPlayer)
        };

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

        private TestSoloResults createResultsScreen() => new TestSoloResults(new ScoreInfo
        {
            TotalScore = 2845370,
            Accuracy = 0.98,
            MaxCombo = 123,
            Rank = ScoreRank.A,
            Date = DateTimeOffset.Now,
            Statistics = new Dictionary<HitResult, int>
            {
                { HitResult.Great, 50 },
                { HitResult.Good, 20 },
                { HitResult.Meh, 50 },
                { HitResult.Miss, 1 }
            },
            User = new User
            {
                Username = "peppy",
            }
        });

        [Test]
        public void ResultsWithoutPlayer()
        {
            TestSoloResults screen = null;
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
            TestSoloResults screen = null;

            AddStep("load results", () => Child = new TestResultsContainer(screen = createResultsScreen()));
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

        private class TestSoloResults : SoloResults
        {
            public HotkeyRetryOverlay RetryOverlay;

            public TestSoloResults(ScoreInfo score)
                : base(score)
            {
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();

                RetryOverlay = InternalChildren.OfType<HotkeyRetryOverlay>().SingleOrDefault();
            }
        }
    }
}
