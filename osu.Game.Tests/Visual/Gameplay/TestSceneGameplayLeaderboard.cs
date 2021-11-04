// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Screens.Play.HUD;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneGameplayLeaderboard : OsuTestScene
    {
        private readonly TestGameplayLeaderboard leaderboard;

        private readonly BindableDouble playerScore = new BindableDouble();

        public TestSceneGameplayLeaderboard()
        {
            Add(leaderboard = new TestGameplayLeaderboard
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(2),
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset leaderboard", () =>
            {
                leaderboard.Clear();
                playerScore.Value = 1222333;
            });

            AddStep("add local player", () => createLeaderboardScore(playerScore, new APIUser { Username = "You", Id = 3 }, true));
            AddStep("toggle expanded", () => leaderboard.Expanded.Value = !leaderboard.Expanded.Value);
            AddSliderStep("set player score", 50, 5000000, 1222333, v => playerScore.Value = v);
        }

        [Test]
        public void TestPlayerScore()
        {
            var player2Score = new BindableDouble(1234567);
            var player3Score = new BindableDouble(1111111);

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
            int playerNumber = 1;
            AddRepeatStep("add player with random score", () => createRandomScore(new APIUser { Username = $"Player {playerNumber++}" }), 10);
        }

        [Test]
        public void TestExistingUsers()
        {
            AddStep("add peppy", () => createRandomScore(new APIUser { Username = "peppy", Id = 2 }));
            AddStep("add smoogipoo", () => createRandomScore(new APIUser { Username = "smoogipoo", Id = 1040328 }));
            AddStep("add flyte", () => createRandomScore(new APIUser { Username = "flyte", Id = 3103765 }));
            AddStep("add frenzibyte", () => createRandomScore(new APIUser { Username = "frenzibyte", Id = 14210502 }));
        }

        [Test]
        public void TestMaxHeight()
        {
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

        private void createRandomScore(APIUser user) => createLeaderboardScore(new BindableDouble(RNG.Next(0, 5_000_000)), user);

        private void createLeaderboardScore(BindableDouble score, APIUser user, bool isTracked = false)
        {
            var leaderboardScore = leaderboard.Add(user, isTracked);
            leaderboardScore.TotalScore.BindTo(score);
        }

        private class TestGameplayLeaderboard : GameplayLeaderboard
        {
            public float Spacing => Flow.Spacing.Y;

            public bool CheckPositionByUsername(string username, int? expectedPosition)
            {
                var scoreItem = Flow.FirstOrDefault(i => i.User?.Username == username);

                return scoreItem != null && scoreItem.ScorePosition == expectedPosition;
            }
        }
    }
}
