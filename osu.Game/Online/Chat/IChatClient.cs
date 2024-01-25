// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;

namespace osu.Game.Online.Chat
{
    public interface IChatClient : IDisposable
    {
        event Action<Channel>? ChannelJoined;
        event Action<Channel>? ChannelParted;
        event Action<List<Message>>? NewMessages;
        event Action? PresenceReceived;

        void FetchInitialMessages();
    }
}
