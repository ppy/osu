// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Framework.Bindables;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public partial class FriendOnlineStreamControl : OverlayStreamControl<OnlineStatus>
    {
        public readonly BindableInt CountAll = new BindableInt();
        public readonly BindableInt CountOnline = new BindableInt();
        public readonly BindableInt CountOffline = new BindableInt();

        public FriendOnlineStreamControl()
        {
            Items =
            [
                OnlineStatus.All,
                OnlineStatus.Online,
                OnlineStatus.Offline
            ];
        }

        protected override OverlayStreamItem<OnlineStatus> CreateStreamItem(OnlineStatus value)
        {
            switch (value)
            {
                case OnlineStatus.All:
                    return new FriendsOnlineStatusItem(value) { UserCount = { BindTarget = CountAll } };

                case OnlineStatus.Online:
                    return new FriendsOnlineStatusItem(value) { UserCount = { BindTarget = CountOnline } };

                case OnlineStatus.Offline:
                    return new FriendsOnlineStatusItem(value) { UserCount = { BindTarget = CountOffline } };

                default:
                    throw new ArgumentException(nameof(value));
            }
        }
    }
}
