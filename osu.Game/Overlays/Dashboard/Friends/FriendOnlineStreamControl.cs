// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System.Collections.Generic;
using System.Linq;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public partial class FriendOnlineStreamControl : OverlayStreamControl<FriendStream>
    {
        protected override OverlayStreamItem<FriendStream> CreateStreamItem(FriendStream value) => new FriendsOnlineStatusItem(value);

        public void Populate(List<APIUser> users)
        {
            Clear();

            int userCount = users.Count;
            int onlineUsersCount = users.Count(user => user.IsOnline);

            AddItem(new FriendStream(OnlineStatus.All, userCount));
            AddItem(new FriendStream(OnlineStatus.Online, onlineUsersCount));
            AddItem(new FriendStream(OnlineStatus.Offline, userCount - onlineUsersCount));

            Current.Value = Items.FirstOrDefault();
        }
    }
}
