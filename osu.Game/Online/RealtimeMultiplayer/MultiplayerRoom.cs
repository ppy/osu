// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Online.RealtimeMultiplayer
{
    [Serializable]
    public class MultiplayerRoom
    {
        public long RoomID { get; set; }

        public MultiplayerRoomState State { get; set; }

        public MultiplayerRoomSettings Settings { get; set; }

        private List<MultiplayerRoomUser> users = new List<MultiplayerRoomUser>();

        public IReadOnlyList<MultiplayerRoomUser> Users
        {
            get
            {
                lock (writeLock)
                    return users.ToArray();
            }
        }

        public MultiplayerRoomUser Join(int userId)
        {
            var user = new MultiplayerRoomUser(userId);
            PerformUpdate(_ => users.Add(user));
            return user;
        }

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
