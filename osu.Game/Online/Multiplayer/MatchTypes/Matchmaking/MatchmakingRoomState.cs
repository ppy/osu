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
        ///   <item>Matching scores are considered to be placed in the lower-equal (e.g. two equal top scores are considered "equal-second").</item>
        ///   <item>Failed scores are considered to have passed the map.</item>
        ///   <item>Missing scores are not considered.</item>
        /// </list>
        /// </remarks>
        /// <param name="scores">The scores to apply.</param>
        /// <param name="placementPoints">The number of points to award for each placement position (0-indexed). Must be at least of equal length to <paramref name="scores"/>.</param>
        public void RecordScores(SoloScoreInfo[] scores, int[] placementPoints)
        {
            if (placementPoints.Length < scores.Length)
                throw new ArgumentException($"{nameof(placementPoints)} must be at least of equal length to {nameof(scores)}.");

            SoloScoreInfo[] orderedScores = scores.OrderByDescending(s => s.TotalScore).ToArray();

            int placement = 0;

            foreach (var scoreGroup in orderedScores.GroupBy(s => s.TotalScore))
            {
                placement += scoreGroup.Count();

                foreach (var score in scoreGroup)
                {
                    MatchmakingUser mmUser = Users.GetOrAdd(score.UserID);
                    mmUser.Points += placementPoints[placement - 1];

                    MatchmakingRound mmRound = mmUser.Rounds.GetOrAdd(CurrentRound);
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
