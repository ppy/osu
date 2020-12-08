// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Online.RealtimeMultiplayer
{
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

        private List<MultiplayerRoomUser> users = new List<MultiplayerRoomUser>();

        private object writeLock = new object();

        /// <summary>
        /// All users which are currently in this room, in any state.
        /// </summary>
        public IReadOnlyList<MultiplayerRoomUser> Users
        {
            get
            {
                lock (writeLock)
                    return users.ToArray();
            }
        }

        /// <summary>
        /// Join a new user to this room.
        /// </summary>
        public MultiplayerRoomUser Join(int userId)
        {
            var user = new MultiplayerRoomUser(userId);
            PerformUpdate(_ => users.Add(user));
            return user;
        }

        /// <summary>
        /// Remove a user from this room.
        /// </summary>
        public MultiplayerRoomUser Leave(int userId)
        {
            MultiplayerRoomUser user = null;

            PerformUpdate(_ =>
            {
                user = users.Find(u => u.UserID == userId);

                if (user != null)
                    users.Remove(user);
            });

            return user;
        }

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
