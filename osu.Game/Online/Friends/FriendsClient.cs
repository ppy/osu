// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Graphics;

namespace osu.Game.Online.Friends
{
    public class FriendsClient : Component, IFriendsClient
    {
        public event Action<int>? Connected;
        public event Action<int>? Disconnected;

        Task IFriendsClient.FriendConnected(int userId)
        {
            Schedule(() => Connected?.Invoke(userId));
            return Task.CompletedTask;
        }

        Task IFriendsClient.FriendDisconnected(int userId)
        {
            Schedule(() => Disconnected?.Invoke(userId));
            return Task.CompletedTask;
        }
    }
}
