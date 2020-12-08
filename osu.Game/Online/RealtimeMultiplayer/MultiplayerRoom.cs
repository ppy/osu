// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Online.RealtimeMultiplayer
{
    [Serializable]
    public class MultiplayerRoom
    {
        private object writeLock = new object();

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
            lock (writeLock) users.Add(user);
            return user;
        }

        public MultiplayerRoomUser Leave(int userId)
        {
            lock (writeLock)
            {
                var user = users.Find(u => u.UserID == userId);

                if (user == null)
                    return null;

                users.Remove(user);
                return user;
            }
        }
    }
}
