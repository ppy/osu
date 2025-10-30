// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Online.Multiplayer.MatchTypes.Matchmaking;

namespace osu.Game.Tests.Online.Matchmaking
{
    public class MatchmakingRoomStateTest
    {
        /// <summary>
        /// The number of points awarded for each placement position (index 0 = #1, index 7 = #8).
        /// </summary>
        private static readonly int[] placement_points = [8, 7, 6, 5, 4, 3, 2, 1];

        [Test]
        public void Basic()
        {
            var state = new MatchmakingRoomState();

            // 1 -> 3 -> 2

            state.AdvanceRound();
            state.RecordScores(
            [
                new SoloScoreInfo { UserID = 2, TotalScore = 500 },
                new SoloScoreInfo { UserID = 1, TotalScore = 1000 },
                new SoloScoreInfo { UserID = 3, TotalScore = 750 },
            ], placement_points);

            Assert.AreEqual(8, state.Users[1].Points);
            Assert.AreEqual(1, state.Users[1].Placement);
            Assert.AreEqual(1, state.Users[1].Rounds[1].Placement);

            Assert.AreEqual(6, state.Users[2].Points);
            Assert.AreEqual(3, state.Users[2].Placement);
            Assert.AreEqual(3, state.Users[2].Rounds[1].Placement);

            Assert.AreEqual(7, state.Users[3].Points);
            Assert.AreEqual(2, state.Users[3].Placement);
            Assert.AreEqual(2, state.Users[3].Rounds[1].Placement);

            // 2 -> 1 -> 3

            state.AdvanceRound();
            state.RecordScores(
            [
                new SoloScoreInfo { UserID = 2, TotalScore = 1000 },
                new SoloScoreInfo { UserID = 1, TotalScore = 750 },
                new SoloScoreInfo { UserID = 3, TotalScore = 500 },
            ], placement_points);

            Assert.AreEqual(15, state.Users[1].Points);
            Assert.AreEqual(1, state.Users[1].Placement);
            Assert.AreEqual(2, state.Users[1].Rounds[2].Placement);

            Assert.AreEqual(14, state.Users[2].Points);
            Assert.AreEqual(2, state.Users[2].Placement);
            Assert.AreEqual(1, state.Users[2].Rounds[2].Placement);

            Assert.AreEqual(13, state.Users[3].Points);
            Assert.AreEqual(3, state.Users[3].Placement);
            Assert.AreEqual(3, state.Users[3].Rounds[2].Placement);
        }

        [Test]
        public void MatchingScores()
        {
            var state = new MatchmakingRoomState();

            // 1 + 2 -> 3 + 4

            state.AdvanceRound();
            state.RecordScores(
            [
                new SoloScoreInfo { UserID = 1, TotalScore = 1000 },
                new SoloScoreInfo { UserID = 2, TotalScore = 1000 },
                new SoloScoreInfo { UserID = 3, TotalScore = 500 },
                new SoloScoreInfo { UserID = 4, TotalScore = 500 },
            ], placement_points);

            Assert.AreEqual(7, state.Users[1].Points);
            Assert.AreEqual(1, state.Users[1].Placement);
            Assert.AreEqual(2, state.Users[1].Rounds[1].Placement);

            Assert.AreEqual(7, state.Users[2].Points);
            Assert.AreEqual(2, state.Users[2].Placement);
            Assert.AreEqual(2, state.Users[2].Rounds[1].Placement);

            Assert.AreEqual(5, state.Users[3].Points);
            Assert.AreEqual(3, state.Users[3].Placement);
            Assert.AreEqual(4, state.Users[3].Rounds[1].Placement);

            Assert.AreEqual(5, state.Users[4].Points);
            Assert.AreEqual(4, state.Users[4].Placement);
            Assert.AreEqual(4, state.Users[4].Rounds[1].Placement);
        }

        [Test]
        public void RoundTieBreaker()
        {
            var state = new MatchmakingRoomState();

            // 1 -> 2

            state.AdvanceRound();
            state.RecordScores(
            [
                new SoloScoreInfo { UserID = 1, TotalScore = 1000 },
                new SoloScoreInfo { UserID = 2, TotalScore = 500 },
            ], placement_points);

            // 2 -> 1

            state.AdvanceRound();
            state.RecordScores(
            [
                new SoloScoreInfo { UserID = 1, TotalScore = 500 },
                new SoloScoreInfo { UserID = 2, TotalScore = 1000 },
            ], placement_points);

            Assert.AreEqual(1, state.Users[1].Placement);
            Assert.AreEqual(2, state.Users[2].Placement);
        }

        [Test]
        public void UserIdTieBreaker()
        {
            var state = new MatchmakingRoomState();

            // 1 + 2 + 3 + 4 + 5 + 6

            state.AdvanceRound();
            state.RecordScores(
            [
                new SoloScoreInfo { UserID = 4, TotalScore = 1000 },
                new SoloScoreInfo { UserID = 6, TotalScore = 1000 },
                new SoloScoreInfo { UserID = 2, TotalScore = 1000 },
                new SoloScoreInfo { UserID = 3, TotalScore = 1000 },
                new SoloScoreInfo { UserID = 1, TotalScore = 1000 },
                new SoloScoreInfo { UserID = 5, TotalScore = 1000 },
            ], placement_points);

            Assert.AreEqual(1, state.Users[1].Placement);
            Assert.AreEqual(2, state.Users[2].Placement);
            Assert.AreEqual(3, state.Users[3].Placement);
            Assert.AreEqual(4, state.Users[4].Placement);
            Assert.AreEqual(5, state.Users[5].Placement);
            Assert.AreEqual(6, state.Users[6].Placement);
        }
    }
}
