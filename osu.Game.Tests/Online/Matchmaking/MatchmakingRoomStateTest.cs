// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using NUnit.Framework;
using NUnit.Framework.Legacy;
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

            ClassicAssert.AreEqual(8, state.Users.GetOrAdd(1).Points);
            ClassicAssert.AreEqual(1, state.Users.GetOrAdd(1).Placement);
            ClassicAssert.AreEqual(1, state.Users.GetOrAdd(1).Rounds.GetOrAdd(1).Placement);

            ClassicAssert.AreEqual(6, state.Users.GetOrAdd(2).Points);
            ClassicAssert.AreEqual(3, state.Users.GetOrAdd(2).Placement);
            ClassicAssert.AreEqual(3, state.Users.GetOrAdd(2).Rounds.GetOrAdd(1).Placement);

            ClassicAssert.AreEqual(7, state.Users.GetOrAdd(3).Points);
            ClassicAssert.AreEqual(2, state.Users.GetOrAdd(3).Placement);
            ClassicAssert.AreEqual(2, state.Users.GetOrAdd(3).Rounds.GetOrAdd(1).Placement);

            // 2 -> 1 -> 3

            state.AdvanceRound();
            state.RecordScores(
            [
                new SoloScoreInfo { UserID = 2, TotalScore = 1000 },
                new SoloScoreInfo { UserID = 1, TotalScore = 750 },
                new SoloScoreInfo { UserID = 3, TotalScore = 500 },
            ], placement_points);

            ClassicAssert.AreEqual(15, state.Users.GetOrAdd(1).Points);
            ClassicAssert.AreEqual(1, state.Users.GetOrAdd(1).Placement);
            ClassicAssert.AreEqual(2, state.Users.GetOrAdd(1).Rounds.GetOrAdd(2).Placement);

            ClassicAssert.AreEqual(14, state.Users.GetOrAdd(2).Points);
            ClassicAssert.AreEqual(2, state.Users.GetOrAdd(2).Placement);
            ClassicAssert.AreEqual(1, state.Users.GetOrAdd(2).Rounds.GetOrAdd(2).Placement);

            ClassicAssert.AreEqual(13, state.Users.GetOrAdd(3).Points);
            ClassicAssert.AreEqual(3, state.Users.GetOrAdd(3).Placement);
            ClassicAssert.AreEqual(3, state.Users.GetOrAdd(3).Rounds.GetOrAdd(2).Placement);
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

            ClassicAssert.AreEqual(7, state.Users.GetOrAdd(1).Points);
            ClassicAssert.AreEqual(1, state.Users.GetOrAdd(1).Placement);
            ClassicAssert.AreEqual(2, state.Users.GetOrAdd(1).Rounds.GetOrAdd(1).Placement);

            ClassicAssert.AreEqual(7, state.Users.GetOrAdd(2).Points);
            ClassicAssert.AreEqual(2, state.Users.GetOrAdd(2).Placement);
            ClassicAssert.AreEqual(2, state.Users.GetOrAdd(2).Rounds.GetOrAdd(1).Placement);

            ClassicAssert.AreEqual(5, state.Users.GetOrAdd(3).Points);
            ClassicAssert.AreEqual(3, state.Users.GetOrAdd(3).Placement);
            ClassicAssert.AreEqual(4, state.Users.GetOrAdd(3).Rounds.GetOrAdd(1).Placement);

            ClassicAssert.AreEqual(5, state.Users.GetOrAdd(4).Points);
            ClassicAssert.AreEqual(4, state.Users.GetOrAdd(4).Placement);
            ClassicAssert.AreEqual(4, state.Users.GetOrAdd(4).Rounds.GetOrAdd(1).Placement);
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

            ClassicAssert.AreEqual(1, state.Users.GetOrAdd(1).Placement);
            ClassicAssert.AreEqual(2, state.Users.GetOrAdd(2).Placement);
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

            ClassicAssert.AreEqual(1, state.Users.GetOrAdd(1).Placement);
            ClassicAssert.AreEqual(2, state.Users.GetOrAdd(2).Placement);
            ClassicAssert.AreEqual(3, state.Users.GetOrAdd(3).Placement);
            ClassicAssert.AreEqual(4, state.Users.GetOrAdd(4).Placement);
            ClassicAssert.AreEqual(5, state.Users.GetOrAdd(5).Placement);
            ClassicAssert.AreEqual(6, state.Users.GetOrAdd(6).Placement);
        }

        [Test]
        public void AbandonOrder()
        {
            var state = new MatchmakingRoomState();

            state.AdvanceRound();
            state.RecordScores(
            [
                new SoloScoreInfo { UserID = 1, TotalScore = 1000 },
                new SoloScoreInfo { UserID = 2, TotalScore = 500 },
            ], placement_points);

            ClassicAssert.AreEqual(1, state.Users.GetOrAdd(1).Placement);
            ClassicAssert.AreEqual(2, state.Users.GetOrAdd(2).Placement);

            state.Users.GetOrAdd(1).AbandonedAt = DateTimeOffset.Now;
            state.RecordScores([], placement_points);

            ClassicAssert.AreEqual(2, state.Users.GetOrAdd(1).Placement);
            ClassicAssert.AreEqual(1, state.Users.GetOrAdd(2).Placement);

            state.Users.GetOrAdd(2).AbandonedAt = DateTimeOffset.Now - TimeSpan.FromMinutes(1);
            state.RecordScores([], placement_points);

            ClassicAssert.AreEqual(1, state.Users.GetOrAdd(1).Placement);
            ClassicAssert.AreEqual(2, state.Users.GetOrAdd(2).Placement);
        }
    }
}
