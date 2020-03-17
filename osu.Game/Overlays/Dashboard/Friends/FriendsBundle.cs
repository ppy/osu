// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Users;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public class FriendsBundle
    {
        public FriendsOnlineStatus Status { get; }

        public int Count => Users.Count;

        public List<User> Users { get; }

        public FriendsBundle(FriendsOnlineStatus status, List<User> users)
        {
            Status = status;
            Users = users;
        }
    }

    public enum FriendsOnlineStatus
    {
        All,
        Online,
        Offline
    }
}
