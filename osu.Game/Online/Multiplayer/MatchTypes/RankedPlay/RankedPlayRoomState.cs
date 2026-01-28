// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using MessagePack;

namespace osu.Game.Online.Multiplayer.MatchTypes.RankedPlay
{
    [Serializable]
    [MessagePackObject]
    public class RankedPlayRoomState : MatchRoomState
    {
        /// <summary>
        /// The current room stage.
        /// </summary>
        [Key(0)]
        public RankedPlayStage Stage { get; set; }

        /// <summary>
        /// The current round number (1-based).
        /// </summary>
        [Key(1)]
        public int CurrentRound { get; set; }

        /// <summary>
        /// A multiplier applied to life point damage.
        /// </summary>
        [Key(2)]
        public double DamageMultiplier { get; set; } = 1;

        /// <summary>
        /// A dictionary containing all users in the room.
        /// </summary>
        [Key(3)]
        public Dictionary<int, RankedPlayUserInfo> Users { get; set; } = [];

        /// <summary>
        /// The ID of the user currently playing a card.
        /// </summary>
        [Key(4)]
        public int? ActiveUserId { get; set; }

        /// <summary>
        /// The average star rating of all cards.
        /// </summary>
        [Key(5)]
        public double StarRating { get; set; }

        /// <summary>
        /// The winner of the match.
        /// </summary>
        [Key(6)]
        public int? WinningUserId { get; set; }

        /// <summary>
        /// The user currently playing a card.
        /// </summary>
        [IgnoreMember]
        public RankedPlayUserInfo? ActiveUser => ActiveUserId == null ? null : Users[ActiveUserId.Value];

        [IgnoreMember]
        public RankedPlayUserInfo? WinningUser => WinningUserId == null ? null : Users[WinningUserId.Value];
    }
}
