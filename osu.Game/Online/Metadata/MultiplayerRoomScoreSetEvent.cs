// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;

namespace osu.Game.Online.Metadata
{
    [Serializable]
    [MessagePackObject]
    public class MultiplayerRoomScoreSetEvent
    {
        /// <summary>
        /// The ID of the room in which the score was set.
        /// </summary>
        [Key(0)]
        public long RoomID { get; set; }

        /// <summary>
        /// The ID of the playlist item on which the score was set.
        /// </summary>
        [Key(1)]
        public long PlaylistItemID { get; set; }

        /// <summary>
        /// The ID of the score set.
        /// </summary>
        [Key(2)]
        public long ScoreID { get; set; }

        /// <summary>
        /// The ID of the user who set the score.
        /// </summary>
        [Key(3)]
        public int UserID { get; set; }

        /// <summary>
        /// The total score set by the player.
        /// </summary>
        [Key(4)]
        public long TotalScore { get; set; }

        /// <summary>
        /// If the set score is the user's new best on a playlist item, this member will contain the user's new rank in the room overall.
        /// Otherwise, it will contain <see langword="null"/>.
        /// </summary>
        [Key(5)]
        public int? NewRank { get; set; }
    }
}
