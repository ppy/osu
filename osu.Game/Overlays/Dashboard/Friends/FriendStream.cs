// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Bindables;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public class FriendStream
    {
        public readonly BindableInt UserCount = new BindableInt();
        public readonly OnlineStatus Status;

        public FriendStream(OnlineStatus status)
        {
            Status = status;
        }
    }
}
