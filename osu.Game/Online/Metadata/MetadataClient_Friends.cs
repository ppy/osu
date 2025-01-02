// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;

namespace osu.Game.Online.Metadata
{
    public partial class MetadataClient
    {
        public abstract event Action<int>? OnFriendConnected;

        public abstract event Action<int>? OnFriendDisconnected;

        /// <inheritdoc/>
        public abstract Task FriendConnected(int userId);

        /// <inheritdoc/>
        public abstract Task FriendDisconnected(int userId);
    }
}
