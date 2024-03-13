// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

namespace osu.Game.Overlays.Dashboard.Friends
{
    public class FriendStream
    {
        public OnlineStatus Status { get; }

        public int Count { get; }

        public FriendStream(OnlineStatus status, int count)
        {
            Status = status;
            Count = count;
        }
    }
}
