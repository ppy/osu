// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Logging;

namespace osu.Game.Online.Notifications.WebSocket
{
    /// <summary>
    /// A notifications client which receives events via a websocket.
    /// </summary>
    public class WebSocketNotificationsClient : PersistentEndpointClient
    {
        public event Action<SocketMessage>? MessageReceived;

        private readonly ClientWebSocket socket;
        private readonly string endpoint;

        public WebSocketNotificationsClient(ClientWebSocket socket, string endpoint)
        {
            this.socket = socket;
            this.endpoint = endpoint;
        }

        public override async Task ConnectAsync(CancellationToken cancellationToken)
        {
            await socket.ConnectAsync(new Uri(endpoint), cancellationToken).ConfigureAwait(false);
            runReadLoop(cancellationToken);
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

                                MessageReceived?.Invoke(message);
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

        public async Task SendAsync(SocketMessage message, CancellationToken? cancellationToken = default)
        {
            if (socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)), WebSocketMessageType.Text, true, cancellationToken ?? CancellationToken.None).ConfigureAwait(false);
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync().ConfigureAwait(false);
            await closeAsync().ConfigureAwait(false);
            socket.Dispose();
        }
    }
}
