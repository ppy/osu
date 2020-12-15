// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Users;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public class FriendOnlineStreamControl : OverlayStreamControl<FriendStream>
    {
        protected override OverlayStreamItem<FriendStream> CreateStreamItem(FriendStream value) => new FriendsOnlineStatusItem(value);

        public void Populate(List<User> users)
        {
            Clear();

            var userCount = users.Count;
            var onlineUsersCount = users.Count(user => user.IsOnline);

            AddItem(new FriendStream(OnlineStatus.All, userCount));
            AddItem(new FriendStream(OnlineStatus.Online, onlineUsersCount));
            AddItem(new FriendStream(OnlineStatus.Offline, userCount - onlineUsersCount));

            Current.Value = Items.FirstOrDefault();
        }
    }
}
