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

        public IReadOnlyList<MultiplayerRoomUser> Users => users;

        private List<MultiplayerRoomUser> users = new List<MultiplayerRoomUser>();

        public void Join(int user) => users.Add(new MultiplayerRoomUser(user));

        public void Leave(int user) => users.RemoveAll(u => u.UserID == user);
    }
}
