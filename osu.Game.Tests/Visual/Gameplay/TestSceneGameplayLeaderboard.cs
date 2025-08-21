// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Extensions.ObjectExtensions;
using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Leaderboards;
using osu.Game.Rulesets.Osu;
using osu.Game.Scoring;
using osu.Game.Screens.Play;
using osu.Game.Screens.Play.HUD;
using osu.Game.Screens.Select.Leaderboards;
using osu.Game.Tests.Gameplay;
using osuTK.Graphics;

namespace osu.Game.Tests.Visual.Gameplay
{
    public partial class TestSceneGameplayLeaderboard : OsuTestScene
    {
        private Box? blackBackground;
        private DrawableGameplayLeaderboard leaderboard = null!;

        [Cached]
        private readonly LeaderboardManager leaderboardManager = new LeaderboardManager();

        [Cached]
        private readonly GameplayState gameplayState;

        public TestSceneGameplayLeaderboard()
        {
            var localScore = new ScoreInfo
            {
                User = new APIUser { Username = "You", Id = 3 }
            };

            gameplayState = TestGameplayState.Create(new OsuRuleset(), null, new Score { ScoreInfo = localScore }, new Bindable<LocalUserPlayingState>(LocalUserPlayingState.Playing));
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            LoadComponent(leaderboardManager);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            AddStep("toggle collapsed", () =>
            {
                if (leaderboard.IsNotNull())
                    leaderboard.CollapseDuringGameplay.Value = !leaderboard.CollapseDuringGameplay.Value;
            });

            AddStep("toggle black background", () => blackBackground?.FadeTo(1 - blackBackground.Alpha, 300, Easing.OutQuint));

            AddSliderStep("leaderboard width", 0, 800, 300, v =>
            {
                if (leaderboard.IsNotNull())
                    leaderboard.Width = v;
            });

            AddSliderStep("leaderboard height", 0, 1000, 300, v =>
            {
                if (leaderboard.IsNotNull())
                    leaderboard.Height = v;
            });

            AddSliderStep("set player score", 50, 1_000_000, 700_000, v => gameplayState.ScoreProcessor.TotalScore.Value = v);
        }

        [Test]
        public void TestDisplay()
        {
            AddStep("set scores", () =>
            {
                var friend = new APIUser { Username = "Friend", Id = 1337 };

                var api = (DummyAPIAccess)API;

                api.Friends.Clear();
                api.Friends.Add(new APIRelation
                {
                    Mutual = true,
                    RelationType = RelationType.Friend,
                    TargetID = friend.OnlineID,
                    TargetUser = friend
                });

                // this is dodgy but anything less dodgy is a lot of work
                ((Bindable<LeaderboardScores?>)leaderboardManager.Scores).Value = LeaderboardScores.Success(new[]
                {
                    new ScoreInfo { User = new APIUser { Username = "Top", Id = 2 }, TotalScore = 900_000, Accuracy = 0.99, MaxCombo = 999 },
                    new ScoreInfo { User = new APIUser { Username = "Second", Id = 14 }, TotalScore = 800_000, Accuracy = 0.9, MaxCombo = 888 },
                    new ScoreInfo { User = friend, TotalScore = 700_000, Accuracy = 0.88, MaxCombo = 777 },
                }, 3, null);
            });

            createLeaderboard();

            AddStep("set score to 650k", () => gameplayState.ScoreProcessor.TotalScore.Value = 650_000);
            AddUntilStep("wait for 4th spot", () => leaderboard.TrackedScore!.ScorePosition.Value, () => Is.EqualTo(4));
            AddStep("set score to 750k", () => gameplayState.ScoreProcessor.TotalScore.Value = 750_000);
            AddUntilStep("wait for 3rd spot", () => leaderboard.TrackedScore!.ScorePosition.Value, () => Is.EqualTo(3));
            AddStep("set score to 850k", () => gameplayState.ScoreProcessor.TotalScore.Value = 850_000);
            AddUntilStep("wait for 2nd spot", () => leaderboard.TrackedScore!.ScorePosition.Value, () => Is.EqualTo(2));
            AddStep("set score to 950k", () => gameplayState.ScoreProcessor.TotalScore.Value = 950_000);
            AddUntilStep("wait for 1st spot", () => leaderboard.TrackedScore!.ScorePosition.Value, () => Is.EqualTo(1));
        }

        [Test]
        public void TestLongScores()
        {
            AddStep("set scores", () =>
            {
                var friend = new APIUser { Username = "Friend", Id = 1337 };

                var api = (DummyAPIAccess)API;

                api.Friends.Clear();
                api.Friends.Add(new APIRelation
                {
                    Mutual = true,
                    RelationType = RelationType.Friend,
                    TargetID = friend.OnlineID,
                    TargetUser = friend
                });

                // this is dodgy but anything less dodgy is a lot of work
                ((Bindable<LeaderboardScores?>)leaderboardManager.Scores).Value = LeaderboardScores.Success(new[]
                {
                    new ScoreInfo { User = new APIUser { Username = "Top", Id = 2 }, TotalScore = 900_000_000, Accuracy = 0.99, MaxCombo = 999999 },
                    new ScoreInfo { User = new APIUser { Username = "Second", Id = 14 }, TotalScore = 800_000_000, Accuracy = 0.9, MaxCombo = 888888 },
                    new ScoreInfo { User = friend, TotalScore = 700_000_000, Accuracy = 0.88, MaxCombo = 777777 },
                }, 3, null);
            });

            createLeaderboard();

            AddStep("set score to 650k", () => gameplayState.ScoreProcessor.TotalScore.Value = 650_000_000);
            AddUntilStep("wait for 4th spot", () => leaderboard.TrackedScore!.ScorePosition.Value, () => Is.EqualTo(4));
            AddStep("set score to 750k", () => gameplayState.ScoreProcessor.TotalScore.Value = 750_000_000);
            AddUntilStep("wait for 3rd spot", () => leaderboard.TrackedScore!.ScorePosition.Value, () => Is.EqualTo(3));
            AddStep("set score to 850k", () => gameplayState.ScoreProcessor.TotalScore.Value = 850_000_000);
            AddUntilStep("wait for 2nd spot", () => leaderboard.TrackedScore!.ScorePosition.Value, () => Is.EqualTo(2));
            AddStep("set score to 950k", () => gameplayState.ScoreProcessor.TotalScore.Value = 950_000_000);
            AddUntilStep("wait for 1st spot", () => leaderboard.TrackedScore!.ScorePosition.Value, () => Is.EqualTo(1));
        }

        [Test]
        public void TestLayoutWithManyScores()
        {
            AddStep("set scores", () =>
            {
                var scores = new List<ScoreInfo>();

                for (int i = 0; i < 32; i++)
                    scores.Add(new ScoreInfo { User = new APIUser { Username = $"Player {i + 1}" }, TotalScore = RNG.Next(700_000, 1_000_000) });

                // this is dodgy but anything less dodgy is a lot of work
                ((Bindable<LeaderboardScores?>)leaderboardManager.Scores).Value = LeaderboardScores.Success(scores, scores.Count, null);
                gameplayState.ScoreProcessor.TotalScore.Value = 0;
            });

            createLeaderboard();

            // Gameplay leaderboard has custom scroll logic, which when coupled with LayoutDuration
            // has caused layout to not work in the past.

            AddUntilStep("wait for fill flow layout",
                () => leaderboard.ChildrenOfType<FillFlowContainer<DrawableGameplayLeaderboardScore>>().First().ScreenSpaceDrawQuad.Intersects(leaderboard.ScreenSpaceDrawQuad));

            AddUntilStep("wait for some scores not masked away",
                () => leaderboard.ChildrenOfType<DrawableGameplayLeaderboardScore>().Any(s => leaderboard.ScreenSpaceDrawQuad.Contains(s.ScreenSpaceDrawQuad.Centre)));

            AddUntilStep("wait for tracked score fully visible", () => leaderboard.ScreenSpaceDrawQuad.Intersects(leaderboard.TrackedScore!.ScreenSpaceDrawQuad));

            AddStep("change score to middle", () => gameplayState.ScoreProcessor.TotalScore.Value = 850_000);
            AddWaitStep("wait for movement", 5);
            AddUntilStep("wait for tracked score fully visible", () => leaderboard.ScreenSpaceDrawQuad.Intersects(leaderboard.TrackedScore!.ScreenSpaceDrawQuad));

            AddStep("change score to first", () => gameplayState.ScoreProcessor.TotalScore.Value = 1_000_000);
            AddWaitStep("wait for movement", 5);
            AddUntilStep("wait for tracked score fully visible", () => leaderboard.ScreenSpaceDrawQuad.Intersects(leaderboard.TrackedScore!.ScreenSpaceDrawQuad));
        }

        [Test]
        public void TestExistingUsers()
        {
            AddStep("set scores", () =>
            {
                // this is dodgy but anything less dodgy is a lot of work
                ((Bindable<LeaderboardScores?>)leaderboardManager.Scores).Value = LeaderboardScores.Success(new[]
                {
                    new ScoreInfo { User = new APIUser { Username = "peppy", Id = 2 }, TotalScore = 900_000, Accuracy = 0.99, MaxCombo = 999 },
                    new ScoreInfo { User = new APIUser { Username = "smoogipoo", Id = 1040328 }, TotalScore = 800_000, Accuracy = 0.9, MaxCombo = 888 },
                    new ScoreInfo { User = new APIUser { Username = "flyte", Id = 3103765 }, TotalScore = 700_000, Accuracy = 0.9, MaxCombo = 888 },
                    new ScoreInfo { User = new APIUser { Username = "frenzibyte", Id = 14210502 }, TotalScore = 600_000, Accuracy = 0.9, MaxCombo = 777 },
                }, 4, null);
            });

            createLeaderboard();
        }

        [Test]
        public void TestQuitScore()
        {
            AddStep("set scores", () =>
            {
                // this is dodgy but anything less dodgy is a lot of work
                ((Bindable<LeaderboardScores?>)leaderboardManager.Scores).Value = LeaderboardScores.Success(new[]
                {
                    new ScoreInfo { User = new APIUser { Username = "Quit", Id = 3 }, TotalScore = 100_000, Accuracy = 0.99, MaxCombo = 999 },
                }, 1, null);
            });

            createLeaderboard();

            AddStep("mark score as quit", () =>
            {
                var quitScore = this.ChildrenOfType<SoloGameplayLeaderboardProvider>().Single().Scores.Single(s => s.User.Username == "Quit");
                quitScore.HasQuit.Value = true;
            });
        }

        private void createLeaderboard()
        {
            AddStep("create leaderboard", () =>
            {
                SoloGameplayLeaderboardProvider soloGameplayLeaderboardProvider;

                Child = new DependencyProvidingContainer
                {
                    RelativeSizeAxes = Axes.Both,
                    CachedDependencies = new (Type, object)[]
                    {
                        (typeof(IGameplayLeaderboardProvider), soloGameplayLeaderboardProvider = new SoloGameplayLeaderboardProvider()),
                    },
                    Children = new Drawable[]
                    {
                        soloGameplayLeaderboardProvider,
                        blackBackground = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Colour = Color4.Black,
                            Alpha = 0f,
                        },
                        leaderboard = new DrawableGameplayLeaderboard
                        {
                            CollapseDuringGameplay = { Value = false },
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    }
                };
            });
        }
    }
}
