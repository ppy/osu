// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using MessagePack;

namespace osu.Game.Online.Multiplayer.MatchTypes.RankedPlay
{
    [Serializable]
    [MessagePackObject]
    public class RankedPlayUserInfo
    {
        /// <summary>
        /// This user's matchmaking rating.
        /// </summary>
        [Key(0)]
        public required int Rating { get; set; }

        /// <summary>
        /// The current life points.
        /// </summary>
        [Key(1)]
        public int Life { get; set; } = 1_000_000;

        /// <summary>
        /// The cards in this user's hand.
        /// </summary>
        [Key(2)]
        public List<RankedPlayCardItem> Hand { get; set; } = [];

        /// <summary>
        /// Rating after conclusion of the match.
        /// </summary>
        [Key(3)]
        public int RatingAfter { get; set; }
    }
}
