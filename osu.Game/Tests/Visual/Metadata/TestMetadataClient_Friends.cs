// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;

namespace osu.Game.Tests.Visual.Metadata
{
    public partial class TestMetadataClient
    {
        public override event Action<int>? OnFriendConnected;

        public override event Action<int>? OnFriendDisconnected;

        public override Task FriendConnected(int userId)
        {
            OnFriendConnected?.Invoke(userId);
            return Task.CompletedTask;
        }

        public override Task FriendDisconnected(int userId)
        {
            OnFriendDisconnected?.Invoke(userId);
            return Task.CompletedTask;
        }
    }
}
