// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Users;

namespace osu.Game.Online.RealtimeMultiplayer
{
    public class MultiplayerRoomUser
    {
        public MultiplayerRoomUser(in int userId)
        {
            UserID = userId;
        }

        public long UserID { get; set; }

        public MultiplayerUserState State { get; set; }

        public User User { get; set; }
    }
}
