// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Users;

namespace osu.Game.Overlays.Home.Friends
{
    public class FriendsOnlineStatusControl : OverlayStreamControl<FriendsBundle>
    {
        protected override OverlayStreamItem<FriendsBundle> CreateStreamItem(FriendsBundle value) => new FriendsOnlineStatusItem(value);

        public void Populate(List<User> users)
        {
            var userCount = users.Count;
            var onlineUsersCount = users.Count(user => user.IsOnline);

            AddItem(new FriendsBundle(FriendsOnlineStatus.All, userCount));
            AddItem(new FriendsBundle(FriendsOnlineStatus.Online, onlineUsersCount));
            AddItem(new FriendsBundle(FriendsOnlineStatus.Offline, userCount - onlineUsersCount));

            Current.Value = Items.FirstOrDefault();
        }
    }
}
