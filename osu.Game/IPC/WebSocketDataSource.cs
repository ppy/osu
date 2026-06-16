// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.IPC.Messages;

namespace osu.Game.IPC
{
    public class WebSocketDataSource : IDisposable
    {
        private readonly IWebSocketProvider provider;

        public event Action<OsuWebSocketMessage>? MessageReceived;

        public WebSocketDataSource(IWebSocketProvider provider)
        {
            this.provider = provider;
            provider.Register(this);
        }

        public void BroadcastMessage(OsuWebSocketMessage message)
            => MessageReceived?.Invoke(message);

        public void Dispose()
        {
            provider.Unregister(this);
        }
    }
}
