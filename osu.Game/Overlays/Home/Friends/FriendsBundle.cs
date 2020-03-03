// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays.Home.Friends
{
    public class FriendsBundle
    {
        public FriendsOnlineStatus Status { get; }

        public int Amount { get; }

        public FriendsBundle(FriendsOnlineStatus status, int amount)
        {
            Status = status;
            Amount = amount;
        }
    }

    public enum FriendsOnlineStatus
    {
        All,
        Online,
        Offline
    }
}
