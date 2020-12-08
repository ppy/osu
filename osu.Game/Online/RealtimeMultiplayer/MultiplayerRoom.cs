// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Online.RealtimeMultiplayer
{
    /// <summary>
    /// A multiplayer room.
    /// </summary>
    [Serializable]
    public class MultiplayerRoom
    {
        /// <summary>
        /// The ID of the room, used for database persistence.
        /// </summary>
        public long RoomID { get; set; }

        /// <summary>
        /// The current state of the room (ie. whether it is in progress or otherwise).
        /// </summary>
        public MultiplayerRoomState State { get; set; }

        /// <summary>
        /// All currently enforced game settings for this room.
        /// </summary>
        public MultiplayerRoomSettings Settings { get; set; }

        /// <summary>
        /// All users currently in this room.
        /// </summary>
        public List<MultiplayerRoomUser> Users { get; set; } = new List<MultiplayerRoomUser>();

        private object writeLock = new object();

        /// <summary>
        /// Perform an update on this room in a thread-safe manner.
        /// </summary>
        /// <param name="action">The action to perform.</param>
        public void PerformUpdate(Action<MultiplayerRoom> action)
        {
            lock (writeLock) action(this);
        }
    }
}
