// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;

namespace osu.Game.Online.Notifications
{
    /// <summary>
    /// An abstract client which receives notification-related events (chat/notifications).
    /// </summary>
    public abstract class NotificationsClient : PersistentEndpointClient
    {
        public Action<Channel>? ChannelJoined;
        public Action<Channel>? ChannelParted;
        public Action<List<Message>>? NewMessages;
        public Action? PresenceReceived;

        protected readonly IAPIProvider API;

        private long lastMessageId;

        protected NotificationsClient(IAPIProvider api)
        {
            API = api;
        }

        public override Task ConnectAsync(CancellationToken cancellationToken)
        {
            API.Queue(CreateInitialFetchRequest(0));
            return Task.CompletedTask;
        }

        protected APIRequest CreateInitialFetchRequest(long? lastMessageId = null)
        {
            var fetchReq = new GetUpdatesRequest(lastMessageId ?? this.lastMessageId);

            fetchReq.Success += updates =>
            {
                if (updates?.Presence != null)
                {
                    foreach (var channel in updates.Presence)
                        HandleChannelJoined(channel);

                    //todo: handle left channels

                    HandleMessages(updates.Messages);
                }

                PresenceReceived?.Invoke();
            };

            return fetchReq;
        }

        protected void HandleChannelJoined(Channel channel)
        {
            channel.Joined.Value = true;
            ChannelJoined?.Invoke(channel);
        }

        protected void HandleChannelParted(Channel channel) => ChannelParted?.Invoke(channel);

        protected void HandleMessages(List<Message>? messages)
        {
            if (messages == null)
                return;

            NewMessages?.Invoke(messages);
            lastMessageId = Math.Max(lastMessageId, messages.LastOrDefault()?.Id ?? 0);
        }
    }
}
