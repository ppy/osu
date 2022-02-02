// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Database;

#nullable enable

namespace osu.Game.Users
{
    public interface IUser : IHasOnlineID<int>, IEquatable<IUser>
    {
        string Username { get; }

        bool IsBot { get; }

        bool IEquatable<IUser>.Equals(IUser? other)
        {
            if (other == null)
                return false;

            return OnlineID == other.OnlineID && Username == other.Username;
        }
    }
}
