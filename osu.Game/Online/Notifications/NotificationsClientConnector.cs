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
    public abstract class NotificationsClientConnector : PersistentEndpointClientConnector
    {
        public event Action<Channel>? ChannelJoined;
        public event Action<Channel>? ChannelParted;
        public event Action<List<Message>>? NewMessages;
        public event Action? PresenceReceived;

        protected NotificationsClientConnector(IAPIProvider api)
            : base(api)
        {
        }

        protected sealed override async Task<PersistentEndpointClient> BuildConnectionAsync(CancellationToken cancellationToken)
        {
            var client = await BuildNotificationClientAsync(cancellationToken).ConfigureAwait(false);

            client.ChannelJoined = c => ChannelJoined?.Invoke(c);
            client.ChannelParted = c => ChannelParted?.Invoke(c);
            client.NewMessages = m => NewMessages?.Invoke(m);
            client.PresenceReceived = () => PresenceReceived?.Invoke();

            return client;
        }

        protected abstract Task<NotificationsClient> BuildNotificationClientAsync(CancellationToken cancellationToken);
    }
}
