// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions;
using osu.Framework.Logging;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Notifications.WebSocket;

namespace osu.Game.Online.Chat
{
    public class WebSocketChatClient : IChatClient
    {
        public event Action<Channel>? ChannelJoined;
        public event Action<Channel>? ChannelParted;
        public event Action<List<Message>>? NewMessages;
        public event Action? PresenceReceived;

        private readonly IAPIProvider api;
        private readonly INotificationsClient client;
        private readonly ConcurrentDictionary<long, Channel> channelsMap = new ConcurrentDictionary<long, Channel>();

        public WebSocketChatClient(IAPIProvider api)
        {
            this.api = api;
            client = api.NotificationsClient;
            client.IsConnected.BindValueChanged(start, true);
        }

        private void start(ValueChangedEvent<bool> connected)
        {
            if (!connected.NewValue)
                return;

            client.MessageReceived += onMessageReceived;
            client.SendAsync(new StartChatRequest()).WaitSafely();
            RequestPresence();
        }

        public void RequestPresence()
        {
            var fetchReq = new GetUpdatesRequest(0);

            fetchReq.Success += updates =>
            {
                if (updates?.Presence != null)
                {
                    foreach (var channel in updates.Presence)
                        joinChannel(channel);

                    handleMessages(updates.Messages);
                }

                PresenceReceived?.Invoke();
            };

            api.Queue(fetchReq);
        }

        private void onMessageReceived(SocketMessage message)
        {
            switch (message.Event)
            {
                case @"chat.channel.join":
                    Debug.Assert(message.Data != null);

                    Channel? joinedChannel = JsonConvert.DeserializeObject<Channel>(message.Data.ToString());
                    Debug.Assert(joinedChannel != null);

                    joinChannel(joinedChannel);
                    break;

                case @"chat.channel.part":
                    Debug.Assert(message.Data != null);

                    Channel? partedChannel = JsonConvert.DeserializeObject<Channel>(message.Data.ToString());
                    Debug.Assert(partedChannel != null);

                    partChannel(partedChannel);
                    break;

                case @"chat.message.new":
                    Debug.Assert(message.Data != null);

                    NewChatMessageData? messageData = JsonConvert.DeserializeObject<NewChatMessageData>(message.Data.ToString());
                    Debug.Assert(messageData != null);

                    foreach (var msg in messageData.Messages)
                        postToChannel(msg);

                    break;
            }
        }

        private void postToChannel(Message message)
        {
            if (channelsMap.TryGetValue(message.ChannelId, out Channel? channel))
            {
                joinChannel(channel);
                NewMessages?.Invoke(new List<Message> { message });
                return;
            }

            var req = new GetChannelRequest(message.ChannelId);

            req.Success += response =>
            {
                joinChannel(channelsMap[message.ChannelId] = response.Channel);
                NewMessages?.Invoke(new List<Message> { message });
            };
            req.Failure += ex => Logger.Error(ex, "Failed to join channel");

            api.Queue(req);
        }

        private void joinChannel(Channel ch)
        {
            ch.Joined.Value = true;
            ChannelJoined?.Invoke(ch);
        }

        private void partChannel(Channel channel) => ChannelParted?.Invoke(channel);

        private void handleMessages(List<Message>? messages)
        {
            if (messages == null)
                return;

            NewMessages?.Invoke(messages);
        }

        public void Dispose()
        {
            client.IsConnected.ValueChanged -= start;
            client.MessageReceived -= onMessageReceived;
        }
    }
}
