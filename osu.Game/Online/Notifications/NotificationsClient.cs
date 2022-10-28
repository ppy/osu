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
    public abstract class NotificationsClient : SocketClient
    {
        public Action<Channel>? ChannelJoined;
        public Action<List<Message>>? NewMessages;
        public Action? PresenceReceived;

        private readonly IAPIProvider api;

        private bool enableChat;
        private long lastMessageId;

        protected NotificationsClient(IAPIProvider api)
        {
            this.api = api;
        }

        public bool EnableChat
        {
            get => enableChat;
            set
            {
                if (enableChat == value)
                    return;

                enableChat = value;

                if (EnableChat)
                    Task.Run(StartChatAsync);
            }
        }

        public override async Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (EnableChat)
                await StartChatAsync();
        }

        protected virtual Task StartChatAsync()
        {
            api.Queue(CreateFetchMessagesRequest(0));
            return Task.CompletedTask;
        }

        protected APIRequest CreateFetchMessagesRequest(long? lastMessageId = null)
        {
            var fetchReq = new GetUpdatesRequest(lastMessageId ?? this.lastMessageId);

            fetchReq.Success += updates =>
            {
                if (updates?.Presence != null)
                {
                    foreach (var channel in updates.Presence)
                        HandleJoinedChannel(channel);

                    //todo: handle left channels

                    HandleMessages(updates.Messages);
                }

                PresenceReceived?.Invoke();
            };

            return fetchReq;
        }

        protected void HandleJoinedChannel(Channel channel)
        {
            // we received this from the server so should mark the channel already joined.
            channel.Joined.Value = true;
            ChannelJoined?.Invoke(channel);
        }

        protected void HandleMessages(List<Message> messages)
        {
            NewMessages?.Invoke(messages);
            lastMessageId = Math.Max(lastMessageId, messages.LastOrDefault()?.Id ?? 0);
        }
    }
}
