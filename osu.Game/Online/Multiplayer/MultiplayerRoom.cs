// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Collections.Generic;
using MessagePack;
using Newtonsoft.Json;
using osu.Game.Online.Rooms;

namespace osu.Game.Online.Multiplayer
{
    /// <summary>
    /// A multiplayer room.
    /// </summary>
    [Serializable]
    [MessagePackObject]
    public class MultiplayerRoom
    {
        /// <summary>
        /// The ID of the room, used for database persistence.
        /// </summary>
        [Key(0)]
        public readonly long RoomID;

        /// <summary>
        /// The current state of the room (ie. whether it is in progress or otherwise).
        /// </summary>
        [Key(1)]
        public MultiplayerRoomState State { get; set; }

        /// <summary>
        /// All currently enforced game settings for this room.
        /// </summary>
        [Key(2)]
        public MultiplayerRoomSettings Settings { get; set; } = new MultiplayerRoomSettings();

        /// <summary>
        /// All users currently in this room.
        /// </summary>
        [Key(3)]
        public IList<MultiplayerRoomUser> Users { get; set; } = new List<MultiplayerRoomUser>();

        /// <summary>
        /// The host of this room, in control of changing room settings.
        /// </summary>
        [Key(4)]
        public MultiplayerRoomUser? Host { get; set; }

        [Key(5)]
        public MatchRoomState? MatchState { get; set; }

        [Key(6)]
        public IList<MultiplayerPlaylistItem> Playlist { get; set; } = new List<MultiplayerPlaylistItem>();

        [JsonConstructor]
        [SerializationConstructor]
        public MultiplayerRoom(long roomId)
        {
            RoomID = roomId;
        }

        public override string ToString() => $"RoomID:{RoomID} Host:{Host?.UserID} Users:{Users.Count} State:{State} Settings: [{Settings}]";
    }
}
