// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using Microsoft.AspNetCore.SignalR.Client;

namespace osu.Game.Online.Friends
{
    public partial class OnlineFriendsClient : FriendsClient
    {
        public void Configure(HubConnection connection)
        {
            connection.On<int>(nameof(IFriendsClient.FriendConnected), ((IFriendsClient)this).FriendConnected);
            connection.On<int>(nameof(IFriendsClient.FriendDisconnected), ((IFriendsClient)this).FriendDisconnected);
        }
    }
}
