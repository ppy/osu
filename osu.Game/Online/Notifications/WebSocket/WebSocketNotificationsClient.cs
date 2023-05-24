// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Logging;
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
        private readonly ClientWebSocket socket;
        private readonly string endpoint;
        private readonly ConcurrentDictionary<long, Channel> channelsMap = new ConcurrentDictionary<long, Channel>();

        public WebSocketNotificationsClient(ClientWebSocket socket, string endpoint, IAPIProvider api)
            : base(api)
        {
            this.socket = socket;
            this.endpoint = endpoint;
        }

        public override async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await socket.ConnectAsync(new Uri(endpoint), cancellationToken).ConfigureAwait(false);
            await sendMessage(new StartChatRequest(), CancellationToken.None).ConfigureAwait(false);

            runReadLoop(cancellationToken);

            await base.ConnectAsync(cancellationToken).ConfigureAwait(false);
        }

        private void runReadLoop(CancellationToken cancellationToken) => Task.Run(async () =>
        {
            byte[] buffer = new byte[1024];
            StringBuilder messageResult = new StringBuilder();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, cancellationToken).ConfigureAwait(false);

                    switch (result.MessageType)
                    {
                        case WebSocketMessageType.Text:
                            messageResult.Append(Encoding.UTF8.GetString(buffer[..result.Count]));

                            if (result.EndOfMessage)
                            {
                                SocketMessage? message = JsonConvert.DeserializeObject<SocketMessage>(messageResult.ToString());
                                messageResult.Clear();

                                Debug.Assert(message != null);

                                if (message.Error != null)
                                {
                                    Logger.Log($"{GetType().ReadableName()} error: {message.Error}", LoggingTarget.Network);
                                    break;
                                }

                                await onMessageReceivedAsync(message).ConfigureAwait(false);
                            }

                            break;

                        case WebSocketMessageType.Binary:
                            throw new NotImplementedException("Binary message type not supported.");

                        case WebSocketMessageType.Close:
                            throw new WebException("Connection closed by remote host.");
                    }
                }
                catch (Exception ex)
                {
                    await InvokeClosed(ex).ConfigureAwait(false);
                    return;
                }
            }
        }, cancellationToken);

        private async Task closeAsync()
        {
            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, @"Disconnecting", CancellationToken.None).ConfigureAwait(false);
            }
            catch
            {
                // Closure can fail if the connection is aborted. Don't really care since it's disposed anyway.
            }
        }

        private async Task sendMessage(SocketMessage message, CancellationToken cancellationToken)
        {
            if (socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)), WebSocketMessageType.Text, true, cancellationToken).ConfigureAwait(false);
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
            await closeAsync().ConfigureAwait(false);
            socket.Dispose();
        }
    }
}
