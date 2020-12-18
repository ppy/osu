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

            AddStep("add local player", () => createLeaderboardScore(playerScore, "You", true));
            AddSliderStep("set player score", 50, 5000000, 1222333, v => playerScore.Value = v);
        }

        [Test]
        public void TestPlayerScore()
        {
            var player2Score = new BindableDouble(1234567);
            var player3Score = new BindableDouble(1111111);

            AddStep("add player 2", () => createLeaderboardScore(player2Score, "Player 2"));
            AddStep("add player 3", () => createLeaderboardScore(player3Score, "Player 3"));

            AddAssert("is player 2 position #1", () => leaderboard.CheckPositionByUsername("Player 2", 1));
            AddAssert("is player position #2", () => leaderboard.CheckPositionByUsername("You", 2));
            AddAssert("is player 3 position #3", () => leaderboard.CheckPositionByUsername("Player 3", 3));

            AddStep("set score above player 3", () => player2Score.Value = playerScore.Value - 500);
            AddAssert("is player position #1", () => leaderboard.CheckPositionByUsername("You", 1));
            AddAssert("is player 2 position #2", () => leaderboard.CheckPositionByUsername("Player 2", 2));
            AddAssert("is player 3 position #3", () => leaderboard.CheckPositionByUsername("Player 3", 3));

            AddStep("set score below players", () => player2Score.Value = playerScore.Value - 123456);
            AddAssert("is player position #1", () => leaderboard.CheckPositionByUsername("You", 1));
            AddAssert("is player 3 position #2", () => leaderboard.CheckPositionByUsername("Player 3", 2));
            AddAssert("is player 2 position #3", () => leaderboard.CheckPositionByUsername("Player 2", 3));
        }

        [Test]
        public void TestRandomScores()
        {
            int playerNumber = 1;
            AddRepeatStep("add player with random score", () => createLeaderboardScore(new BindableDouble(RNG.Next(0, 5_000_000)), $"Player {playerNumber++}"), 10);
        }

        private void createLeaderboardScore(BindableDouble score, string username, bool isTracked = false)
        {
            var leaderboardScore = leaderboard.AddPlayer(new User { Username = username }, isTracked);
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
