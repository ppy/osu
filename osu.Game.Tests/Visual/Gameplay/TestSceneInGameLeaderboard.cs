// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Linq;
using NUnit.Framework;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Testing;
using osu.Game.Screens.Play;
using osu.Game.Users;
using osuTK;

namespace osu.Game.Tests.Visual.Gameplay
{
    [TestFixture]
    public class TestSceneInGameLeaderboard : OsuTestScene
    {
        private readonly TestInGameLeaderboard leaderboard;
        private readonly BindableDouble playerScore;

        public TestSceneInGameLeaderboard()
        {
            Add(leaderboard = new TestInGameLeaderboard
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Scale = new Vector2(2),
                RelativeSizeAxes = Axes.X,
                PlayerCurrentScore = { BindTarget = playerScore = new BindableDouble(1222333) }
            });
        }

        [SetUpSteps]
        public void SetUpSteps()
        {
            AddStep("reset leaderboard", () =>
            {
                leaderboard.ClearScores();
                playerScore.Value = 1222333;
            });

            AddStep("add player user", () => leaderboard.PlayerUser = new User { Username = "You" });
            AddSliderStep("set player score", 50, 5000000, 1222333, v => playerScore.Value = v);
        }

        [Test]
        public void TestPlayerScore()
        {
            var player2Score = new BindableDouble(1234567);
            var player3Score = new BindableDouble(1111111);

            AddStep("add player 2", () => leaderboard.AddDummyPlayer(player2Score, "Player 2"));
            AddStep("add player 3", () => leaderboard.AddDummyPlayer(player3Score, "Player 3"));

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

        private class TestInGameLeaderboard : InGameLeaderboard
        {
            public void ClearScores() => ScoresContainer.RemoveAll(s => s.User.Username != PlayerUser.Username);

            public bool CheckPositionByUsername(string username, int? estimatedPosition)
            {
                var scoreItem = ScoresContainer.FirstOrDefault(i => i.User.Username == username);

                return scoreItem != null && scoreItem.ScorePosition == estimatedPosition;
            }

            public void AddDummyPlayer(BindableDouble currentScore, string username) => ScoresContainer.AddRealTimePlayer(currentScore, new User { Username = username });
        }
    }
}
