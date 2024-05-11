// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Database;
using osu.Game.Users;
using osu.Game.Utils;
using Realms;

namespace osu.Game.Models
{
    public class RealmUser : EmbeddedObject, IUser, IEquatable<RealmUser>, IDeepCloneable<RealmUser>
    {
        public int OnlineID { get; set; } = 1;

        public string Username { get; set; } = string.Empty;

        [Ignored]
        public CountryCode CountryCode
        {
            get => Enum.TryParse(CountryString, out CountryCode country) ? country : CountryCode.Unknown;
            set => CountryString = value.ToString();
        }

        [MapTo(nameof(CountryCode))]
        public string CountryString { get; set; } = default(CountryCode).ToString();

        public bool IsBot => false;

        public bool Equals(RealmUser? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return OnlineID == other.OnlineID && Username == other.Username;
        }

        public RealmUser DeepClone() => (RealmUser)this.Detach().MemberwiseClone();
    }
}
