// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Overlays.Dashboard.Friends
{
    public class FriendsOnlineStatusControl : OverlayStreamControlCN<FriendsBundle>
    {
        protected override OverlayStreamItemCN<FriendsBundle> CreateStreamItem(FriendsBundle value) => new FriendsOnlineStatusItem(value);

        public void Populate(List<APIFriend> users)
        {
            Clear();

            AddItem(new FriendsBundle(FriendsOnlineStatus.All, users));
            AddItem(new FriendsBundle(FriendsOnlineStatus.Online, users.Where(u => u.IsOnline).ToList()));
            AddItem(new FriendsBundle(FriendsOnlineStatus.Offline, users.Where(u => !u.IsOnline).ToList()));

            Current.Value = Items.FirstOrDefault();
        }
    }
}