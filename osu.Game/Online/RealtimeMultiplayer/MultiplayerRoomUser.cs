// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Users;

namespace osu.Game.Online.RealtimeMultiplayer
{
    public class MultiplayerRoomUser : IEquatable<MultiplayerRoomUser>
    {
        public MultiplayerRoomUser(in int userId)
        {
            UserID = userId;
        }

        public long UserID { get; }

        public MultiplayerUserState State { get; set; }

        public User User { get; set; }

        public bool Equals(MultiplayerRoomUser other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return UserID == other.UserID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;

            return Equals((MultiplayerRoomUser)obj);
        }

        public override int GetHashCode() => UserID.GetHashCode();
    }
}
