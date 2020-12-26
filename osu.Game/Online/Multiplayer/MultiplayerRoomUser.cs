// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using Newtonsoft.Json;
using osu.Game.Users;

namespace osu.Game.Online.Multiplayer
{
    [Serializable]
    public class MultiplayerRoomUser : IEquatable<MultiplayerRoomUser>
    {
        public readonly int UserID;

        public MultiplayerUserState State { get; set; } = MultiplayerUserState.Idle;

        public User? User { get; set; }

        [JsonConstructor]
        public MultiplayerRoomUser(in int userId)
        {
            UserID = userId;
        }

        public bool Equals(MultiplayerRoomUser other)
        {
            if (ReferenceEquals(this, other)) return true;

            return UserID == other.UserID;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;

            return Equals((MultiplayerRoomUser)obj);
        }

        public override int GetHashCode() => UserID.GetHashCode();
    }
}
