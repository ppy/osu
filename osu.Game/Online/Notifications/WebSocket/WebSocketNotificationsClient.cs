// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;

namespace osu.Game.Online.Notifications.WebSocket
{
    /// <summary>
    /// A notifications client which receives events via a websocket.
    /// </summary>
    public class WebSocketNotificationsClient : NotificationsClient
    {
        private readonly OsuClientWebSocket socket;
        private readonly ConcurrentDictionary<long, Channel> channelsMap = new ConcurrentDictionary<long, Channel>();

        public WebSocketNotificationsClient(IAPIProvider api, string endpoint)
            : base(api)
        {
            socket = new OsuClientWebSocket(api, endpoint);
            socket.MessageReceived += onMessageReceivedAsync;
            socket.Closed += InvokeClosed;
        }

        public override async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await socket.ConnectAsync(cancellationToken).ConfigureAwait(false);
            await socket.SendMessage(new StartChatRequest(), CancellationToken.None).ConfigureAwait(false);

            await base.ConnectAsync(cancellationToken).ConfigureAwait(false);
        }

        private async Task onMessageReceivedAsync(SocketMessage message)
        {
            switch (message.Event)
            {
                case @"chat.channel.join":
                    Debug.Assert(message.Data != null);

                    Channel? joinedChannel = JsonConvert.DeserializeObject<Channel>(message.Data.ToString());
                    Debug.Assert(joinedChannel != null);

                    HandleChannelJoined(joinedChannel);
                    break;

                case @"chat.channel.part":
                    Debug.Assert(message.Data != null);

                    Channel? partedChannel = JsonConvert.DeserializeObject<Channel>(message.Data.ToString());
                    Debug.Assert(partedChannel != null);

                    HandleChannelParted(partedChannel);
                    break;

                case @"chat.message.new":
                    Debug.Assert(message.Data != null);

                    NewChatMessageData? messageData = JsonConvert.DeserializeObject<NewChatMessageData>(message.Data.ToString());
                    Debug.Assert(messageData != null);

                    foreach (var msg in messageData.Messages)
                        HandleChannelJoined(await getChannel(msg.ChannelId).ConfigureAwait(false));

                    HandleMessages(messageData.Messages);
                    break;
            }
        }

        private async Task<Channel> getChannel(long channelId)
        {
            if (channelsMap.TryGetValue(channelId, out Channel? channel))
                return channel;

            var tsc = new TaskCompletionSource<Channel>();
            var req = new GetChannelRequest(channelId);

            req.Success += response =>
            {
                channelsMap[channelId] = response.Channel;
                tsc.SetResult(response.Channel);
            };

            req.Failure += ex => tsc.SetException(ex);

            API.Queue(req);

            return await tsc.Task.ConfigureAwait(false);
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);
            await socket.DisposeAsync().ConfigureAwait(false);
        }
    }
}
