// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;

namespace osu.Game.Online.Metadata
{
    public partial class OnlineMetadataClient
    {
        public override event Action<int>? OnFriendConnected;

        public override event Action<int>? OnFriendDisconnected;

        private void configureConnection_Friends(HubConnection connection)
        {
            connection.On<int>(nameof(IMetadataClient.FriendConnected), ((IMetadataClient)this).FriendConnected);
            connection.On<int>(nameof(IMetadataClient.FriendDisconnected), ((IMetadataClient)this).FriendDisconnected);
        }

        public override Task FriendConnected(int userId)
        {
            Schedule(() => OnFriendConnected?.Invoke(userId));
            return Task.CompletedTask;
        }

        public override Task FriendDisconnected(int userId)
        {
            Schedule(() => OnFriendDisconnected?.Invoke(userId));
            return Task.CompletedTask;
        }
    }
}
