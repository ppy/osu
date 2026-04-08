// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Online.Multiplayer;

namespace osu.Game.Online.Matchmaking
{
    [Serializable]
    [MessagePackObject]
    public class MatchmakingLobbyStatus
    {
        /// <summary>
        /// A sample of users in the lobby.
        /// </summary>
        [Key(0)]
        public int[] UsersInQueue { get; set; } = [];

        /// <summary>
        /// The distribution of user ratings in the lobby.
        /// </summary>
        [Key(1)]
        public (int Rating, int Count)[] RatingDistribution { get; set; } = [];

        /// <summary>
        /// The current user's rating.
        /// </summary>
        [Key(2)]
        public int? UserRating { get; set; }

        /// <summary>
        /// A sample of the most recent completed matches.
        /// </summary>
        [Key(3)]
        public MatchRoomState[] RecentMatches { get; set; } = [];
    }
}
