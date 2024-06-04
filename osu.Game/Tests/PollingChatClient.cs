// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Online;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;

namespace osu.Game.Tests
{
    public class PollingChatClient : PersistentEndpointClient
    {
        public event Action<Channel>? ChannelJoined;
        public event Action<List<Message>>? NewMessages;
        public event Action? PresenceReceived;

        private readonly IAPIProvider api;

        private long lastMessageId;

        public PollingChatClient(IAPIProvider api)
        {
            this.api = api;
        }

        public override Task ConnectAsync(CancellationToken cancellationToken)
        {
            Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    await api.PerformAsync(CreateInitialFetchRequest()).ConfigureAwait(true);
                    await Task.Delay(1000, cancellationToken).ConfigureAwait(true);
                }
            }, cancellationToken);

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
                        handleChannelJoined(channel);

                    //todo: handle left channels

                    handleMessages(updates.Messages);
                }

                PresenceReceived?.Invoke();
            };

            return fetchReq;
        }

        private void handleChannelJoined(Channel channel)
        {
            channel.Joined.Value = true;
            ChannelJoined?.Invoke(channel);
        }

        private void handleMessages(List<Message>? messages)
        {
            if (messages == null)
                return;

            NewMessages?.Invoke(messages);
            lastMessageId = Math.Max(lastMessageId, messages.LastOrDefault()?.Id ?? 0);
        }
    }
}
