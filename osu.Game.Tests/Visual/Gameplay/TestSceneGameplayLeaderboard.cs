// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Framework.Utils;
using osu.Game.Screens.Play.HUD;
using osu.Game.Users;
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

            AddStep("add local player", () => createLeaderboardScore(playerScore, new User { Username = "You", Id = 3 }, true));
            AddSliderStep("set player score", 50, 5000000, 1222333, v => playerScore.Value = v);
        }

        [Test]
        public void TestPlayerScore()
        {
            var player2Score = new BindableDouble(1234567);
            var player3Score = new BindableDouble(1111111);

            AddStep("add player 2", () => createLeaderboardScore(player2Score, new User { Username = "Player 2" }));
            AddStep("add player 3", () => createLeaderboardScore(player3Score, new User { Username = "Player 3" }));

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
            AddRepeatStep("add player with random score", () => createRandomScore(new User { Username = $"Player {playerNumber++}" }), 10);
        }

        [Test]
        public void TestExistingUsers()
        {
            AddStep("add peppy", () => createRandomScore(new User { Username = "peppy", Id = 2 }));
            AddStep("add smoogipoo", () => createRandomScore(new User { Username = "smoogipoo", Id = 1040328 }));
            AddStep("add flyte", () => createRandomScore(new User { Username = "flyte", Id = 3103765 }));
            AddStep("add frenzibyte", () => createRandomScore(new User { Username = "frenzibyte", Id = 14210502 }));
        }

        private void createRandomScore(User user) => createLeaderboardScore(new BindableDouble(RNG.Next(0, 5_000_000)), user);

        private void createLeaderboardScore(BindableDouble score, User user, bool isTracked = false)
        {
            var leaderboardScore = leaderboard.AddPlayer(user, isTracked);
            leaderboardScore.TotalScore.BindTo(score);
        }

        private class TestGameplayLeaderboard : GameplayLeaderboard
        {
            public bool CheckPositionByUsername(string username, int? expectedPosition)
            {
                var scoreItem = this.FirstOrDefault(i => i.User.Username == username);

                return scoreItem != null && scoreItem.ScorePosition == expectedPosition;
            }
        }
    }
}
