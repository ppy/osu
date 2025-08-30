// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using MessagePack;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Online.Multiplayer.MatchTypes.Matchmaking
{
    /// <summary>
    /// Describes the state of a matchmaking room.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public class MatchmakingRoomState : MatchRoomState
    {
        /// <summary>
        /// The number of points awarded for each placement position (index 0 = #1, index 7 = #8).
        /// </summary>
        private static readonly int[] placement_points = [8, 7, 6, 5, 4, 3, 2, 1];

        /// <summary>
        /// The current room status.
        /// </summary>
        [Key(0)]
        public MatchmakingStage Stage { get; set; }

        /// <summary>
        /// The current round number (1-based).
        /// </summary>
        [Key(1)]
        public int CurrentRound { get; set; }

        /// <summary>
        /// The playlist items that were picked as gameplay candidates.
        /// </summary>
        [Key(2)]
        public long[] CandidateItems { get; set; } = [];

        /// <summary>
        /// The final gameplay candidate.
        /// </summary>
        [Key(3)]
        public long CandidateItem { get; set; }

        /// <summary>
        /// The users in the room.
        /// </summary>
        [Key(4)]
        public MatchmakingUserList Users { get; set; } = new MatchmakingUserList();

        /// <summary>
        /// Advances to the next round.
        /// </summary>
        public void AdvanceRound()
        {
            CurrentRound++;
        }

        /// <summary>
        /// Sets scores for the current round, applying points and adjusting user placements.
        /// </summary>
        /// <remarks>
        /// When applying points:
        /// <list type="bullet">
        ///   <item>Matching scores are considered to be placed in the lower-equal (e.g. two equal top scores would be considered "equal-second").</item>
        ///   <item>Failed scores are considered to have passed the map.</item>
        ///   <item>Missing scores are not considered.</item>
        /// </list>
        /// </remarks>
        /// <param name="scores">The scores to apply.</param>
        public void RecordScores(SoloScoreInfo[] scores)
        {
            SoloScoreInfo[] orderedScores = scores.OrderByDescending(s => s.TotalScore).ToArray();

            int placement = 0;

            foreach (var scoreGroup in orderedScores.GroupBy(s => s.TotalScore))
            {
                placement += scoreGroup.Count();

                foreach (var score in scoreGroup)
                {
                    MatchmakingUser mmUser = Users[score.UserID];
                    mmUser.Points += placement_points[placement - 1];

                    MatchmakingRound mmRound = mmUser.Rounds[CurrentRound];
                    mmRound.Placement = placement;
                    mmRound.TotalScore = score.TotalScore;
                    mmRound.Accuracy = score.Accuracy;
                    mmRound.MaxCombo = score.MaxCombo;
                    mmRound.Statistics = score.Statistics;
                }
            }

            int i = 1;
            foreach (var user in Users.Order(new MatchmakingUserComparer(CurrentRound)))
                user.Placement = i++;
        }
    }
}
