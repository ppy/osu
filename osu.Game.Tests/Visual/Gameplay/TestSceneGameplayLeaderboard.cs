// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Extensions.PolygonExtensions;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public partial class TestSceneGameplayLeaderboard : OsuTestScene
    {
        private TestGameplayLeaderboard leaderboard;

        private readonly BindableLong playerScore = new BindableLong();
        private bool leaderboardExpanded;

        public TestSceneGameplayLeaderboard()
        {
            AddToggleStep("toggle expanded", e =>
            {
                leaderboardExpanded = e;
                if (leaderboard != null)
                    leaderboard.Expanded.Value = e;
            });

            AddSliderStep("set player score", 50, 5000000, 1222333, v => playerScore.Value = v);
        }

        [Test]
        public void TestLayoutWithManyScores()
        {
            createLeaderboard();

            AddStep("add many scores in one go", () =>
            {
                for (int i = 0; i < 32; i++)
                    createRandomScore(new APIUser { Username = $"Player {i + 1}" });

                // Add player at end to force an animation down the whole list.
                playerScore.Value = 0;
                createLeaderboardScore(playerScore, new APIUser { Username = "You", Id = 3 }, true);
            });

            // Gameplay leaderboard has custom scroll logic, which when coupled with LayoutDuration
            // has caused layout to not work in the past.

            AddUntilStep("wait for fill flow layout",
                () => leaderboard.ChildrenOfType<FillFlowContainer<GameplayLeaderboardScore>>().First().ScreenSpaceDrawQuad.Intersects(leaderboard.ScreenSpaceDrawQuad));

            AddUntilStep("wait for some scores not masked away",
                () => leaderboard.ChildrenOfType<GameplayLeaderboardScore>().Any(s => leaderboard.ScreenSpaceDrawQuad.Contains(s.ScreenSpaceDrawQuad.Centre)));

            AddUntilStep("wait for tracked score fully visible", () => leaderboard.ScreenSpaceDrawQuad.Intersects(leaderboard.TrackedScore!.ScreenSpaceDrawQuad));

            AddStep("change score to middle", () => playerScore.Value = 1000000);
            AddWaitStep("wait for movement", 5);
            AddUntilStep("wait for tracked score fully visible", () => leaderboard.ScreenSpaceDrawQuad.Intersects(leaderboard.TrackedScore!.ScreenSpaceDrawQuad));

            AddStep("change score to first", () => playerScore.Value = 5000000);
            AddWaitStep("wait for movement", 5);
            AddUntilStep("wait for tracked score fully visible", () => leaderboard.ScreenSpaceDrawQuad.Intersects(leaderboard.TrackedScore!.ScreenSpaceDrawQuad));
        }

        [Test]
        public void TestPlayerScore()
        {
            createLeaderboard();
            addLocalPlayer();

            var player2Score = new BindableLong(1234567);
            var player3Score = new BindableLong(1111111);

            AddStep("add player 2", () => createLeaderboardScore(player2Score, new APIUser { Username = "Player 2" }));
            AddStep("add player 3", () => createLeaderboardScore(player3Score, new APIUser { Username = "Player 3" }));

            AddUntilStep("is player 2 position #1", () => leaderboard.CheckPositionByUsername("Player 2", 1));
            AddUntilStep("is player position #2", () => leaderboard.CheckPositionByUsername("You", 2));
            AddUntilStep("is player 3 position #3", () => leaderboard.CheckPositionByUsername("Player 3", 3));

            AddStep("set score above player 3", () => player2Score.Value = playerScore.Value - 500);
            AddUntilStep("is player position #1", () => leaderboard.CheckPositionByUsername("You", 1));
            AddUntilStep("is player 2 position #2", () => leaderboard.CheckPositionByUsername("Player 2", 2));
            AddUntilStep("is player 3 position #3", () => leaderboard.CheckPositionByUsername("Player 3", 3));

            AddStep("set score below players", () => player2Score.Value = playerScore.Value - 123456);
            AddUntilStep("is player position #1", () => leaderboard.CheckPositionByUsername("You", 1));
            AddUntilStep("is player 3 position #2", () => leaderboard.CheckPositionByUsername("Player 3", 2));
            AddUntilStep("is player 2 position #3", () => leaderboard.CheckPositionByUsername("Player 2", 3));
        }

        [Test]
        public void TestRandomScores()
        {
            createLeaderboard();
            addLocalPlayer();

            int playerNumber = 1;
            AddRepeatStep("add player with random score", () => createRandomScore(new APIUser { Username = $"Player {playerNumber++}" }), 10);
        }

        [Test]
        public void TestExistingUsers()
        {
            createLeaderboard();
            addLocalPlayer();

            AddStep("add peppy", () => createRandomScore(new APIUser { Username = "peppy", Id = 2 }));
            AddStep("add smoogipoo", () => createRandomScore(new APIUser { Username = "smoogipoo", Id = 1040328 }));
            AddStep("add flyte", () => createRandomScore(new APIUser { Username = "flyte", Id = 3103765 }));
            AddStep("add frenzibyte", () => createRandomScore(new APIUser { Username = "frenzibyte", Id = 14210502 }));
            AddStep("add really long user", () => createRandomScore(new APIUser { Username = "really long user", Id = 1 }));
        }

        [Test]
        public void TestMaxHeight()
        {
            createLeaderboard();
            addLocalPlayer();

            int playerNumber = 1;
            AddRepeatStep("add 3 other players", () => createRandomScore(new APIUser { Username = $"Player {playerNumber++}" }), 3);
            checkHeight(4);

            AddRepeatStep("add 4 other players", () => createRandomScore(new APIUser { Username = $"Player {playerNumber++}" }), 4);
            checkHeight(8);

            AddRepeatStep("add 4 other players", () => createRandomScore(new APIUser { Username = $"Player {playerNumber++}" }), 4);
            checkHeight(8);

            void checkHeight(int panelCount)
                => AddAssert($"leaderboard height is {panelCount} panels high", () => leaderboard.DrawHeight == (GameplayLeaderboardScore.PANEL_HEIGHT + leaderboard.Spacing) * panelCount);
        }

        [Test]
        public void TestFriendScore()
        {
            APIUser friend = new APIUser { Username = "my friend", Id = 10000 };

            createLeaderboard();
            addLocalPlayer();

            AddStep("Add friend to API", () =>
            {
                var api = (DummyAPIAccess)API;

                api.Friends.Clear();
                api.Friends.Add(friend);
            });

            int playerNumber = 1;

            AddRepeatStep("add 3 other players", () => createRandomScore(new APIUser { Username = $"Player {playerNumber++}" }), 3);
            AddUntilStep("no pink color scores",
                () => leaderboard.ChildrenOfType<Box>().Select(b => ((Colour4)b.Colour).ToHex()),
                () => Does.Not.Contain("#FF549A"));

            AddRepeatStep("add 3 friend score", () => createRandomScore(friend), 3);
            AddUntilStep("at least one friend score is pink",
                () => leaderboard.GetAllScoresForUsername("my friend")
                                 .SelectMany(score => score.ChildrenOfType<Box>())
                                 .Select(b => ((Colour4)b.Colour).ToHex()),
                () => Does.Contain("#FF549A"));
        }

        private void addLocalPlayer()
        {
            AddStep("add local player", () =>
            {
                playerScore.Value = 1222333;
                createLeaderboardScore(playerScore, new APIUser { Username = "You", Id = 3 }, true);
            });
        }

        private void createLeaderboard()
        {
            AddStep("create leaderboard", () =>
            {
                Child = leaderboard = new TestGameplayLeaderboard
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(2),
                    Expanded = { Value = leaderboardExpanded },
                };
            });
        }

        private void createRandomScore(APIUser user) => createLeaderboardScore(new BindableLong(RNG.Next(0, 5_000_000)), user);

        private void createLeaderboardScore(BindableLong score, APIUser user, bool isTracked = false)
        {
            var leaderboardScore = leaderboard.Add(user, isTracked);
            leaderboardScore.TotalScore.BindTo(score);
        }

        private partial class TestGameplayLeaderboard : GameplayLeaderboard
        {
            public float Spacing => Flow.Spacing.Y;

            public bool CheckPositionByUsername(string username, int? expectedPosition)
            {
                var scoreItem = Flow.FirstOrDefault(i => i.User?.Username == username);

                return scoreItem != null && scoreItem.ScorePosition == expectedPosition;
            }

            public IEnumerable<GameplayLeaderboardScore> GetAllScoresForUsername(string username)
                => Flow.Where(i => i.User?.Username == username);
        }
    }
}
