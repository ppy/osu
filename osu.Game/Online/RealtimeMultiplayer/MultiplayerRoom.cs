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

        private List<MultiplayerRoomUser> users = new List<MultiplayerRoomUser>();

        public IReadOnlyList<MultiplayerRoomUser> Users
        {
            get
            {
                lock (writeLock)
                    return users.ToArray();
            }
        }

        public void Join(int user)
        {
            lock (writeLock)
                users.Add(new MultiplayerRoomUser(user));
        }

        public void Leave(int user)
        {
            lock (writeLock)
                users.RemoveAll(u => u.UserID == user);
        }
    }
}
