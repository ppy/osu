// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Graphics;
using osu.Game.IPC.Messages;

namespace osu.Game.IPC
{
    public partial class WebSocketDataSource : Component
    {
        private readonly IWebSocketProvider provider;

        public event Action<OsuWebSocketMessage>? BroadcastMessageReceived;
        public event Action<int, OsuWebSocketMessage>? ClientMessageReceived;

        public WebSocketDataSource(IWebSocketProvider provider)
        {
            this.provider = provider;
            provider.Register(this);
        }

        public void BroadcastMessage(OsuWebSocketMessage message)
            => BroadcastMessageReceived?.Invoke(message);

        public void SendMessage(int clientId, OsuWebSocketMessage message)
            => ClientMessageReceived?.Invoke(clientId, message);

        public virtual Task OnClientConnected(int clientId)
            => Task.CompletedTask;

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            provider.Unregister(this);
        }
    }
}
