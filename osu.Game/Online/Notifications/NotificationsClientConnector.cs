// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Online.API;
using osu.Game.Online.Chat;

namespace osu.Game.Online.Notifications
{
    /// <summary>
    /// An abstract connector or <see cref="NotificationsClient"/>s.
    /// </summary>
    public abstract class NotificationsClientConnector : SocketClientConnector
    {
        public event Action<Channel>? ChannelJoined;
        public event Action<List<Message>>? NewMessages;
        public event Action? PresenceReceived;

        private bool chatStarted;

        protected NotificationsClientConnector(IAPIProvider api)
            : base(api)
        {
        }

        public void StartChat()
        {
            chatStarted = true;

            if (CurrentConnection is NotificationsClient client)
                client.EnableChat = true;
        }

        protected sealed override async Task<SocketClient> BuildConnectionAsync(CancellationToken cancellationToken)
        {
            var client = await BuildNotificationClientAsync(cancellationToken);

            client.ChannelJoined = c => ChannelJoined?.Invoke(c);
            client.NewMessages = m => NewMessages?.Invoke(m);
            client.PresenceReceived = () => PresenceReceived?.Invoke();
            client.EnableChat = chatStarted;

            return client;
        }

        protected abstract Task<NotificationsClient> BuildNotificationClientAsync(CancellationToken cancellationToken);
    }
}
