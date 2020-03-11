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
            AddItem(new FriendsBundle(FriendsOnlineStatus.All, users));
            AddItem(new FriendsBundle(FriendsOnlineStatus.Online, users.Where(u => u.IsOnline).ToList()));
            AddItem(new FriendsBundle(FriendsOnlineStatus.Offline, users.Where(u => !u.IsOnline).ToList()));

            Current.Value = Items.FirstOrDefault();
        }
    }
}
