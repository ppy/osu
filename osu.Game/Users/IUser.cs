// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Game.Database;

namespace osu.Game.Users
{
    public interface IUser : IHasOnlineID<int>
    {
        string Username { get; }

        bool IsBot { get; }
    }
}
