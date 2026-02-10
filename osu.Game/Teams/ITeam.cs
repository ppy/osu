// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Database;

namespace osu.Game.Teams
{
    public interface ITeam : IHasOnlineID<int>, IEquatable<ITeam>
    {
        string ShortName { get; }

        bool IEquatable<ITeam>.Equals(ITeam? other)
        {
            if (other == null)
                return false;

            return OnlineID == other.OnlineID && ShortName == other.ShortName;
        }
    }
}
