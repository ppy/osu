// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using MessagePack;
using osu.Game.Users;

namespace osu.Game.Online.Spectator
{
    [Serializable]
    [MessagePackObject]
    public class SpectatorUser : IUser, IEquatable<SpectatorUser>
    {
        [Key(0)]
        public int OnlineID { get; set; }

        [Key(1)]
        public string Username { get; set; } = string.Empty;

        [IgnoreMember]
        public CountryCode CountryCode => CountryCode.Unknown;

        [IgnoreMember]
        public bool IsBot => false;

        public bool Equals(SpectatorUser? other)
        {
            if (other is null) return false;
            if (ReferenceEquals(this, other)) return true;

            return OnlineID == other.OnlineID;
        }

        public override bool Equals(object? obj) => Equals(obj as SpectatorUser);

        // ReSharper disable once NonReadonlyMemberInGetHashCode
        public override int GetHashCode() => OnlineID;
    }
}
