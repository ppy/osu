// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Logging;
using osu.Game.Online.API;

namespace osu.Game.Online.Notifications
{
    public partial class NotificationsClient : SocketClient
    {
        private readonly ClientWebSocket socket;
        private readonly string endpoint;
        private readonly IAPIProvider api;

        public NotificationsClient(ClientWebSocket socket, string endpoint, IAPIProvider api)
        {
            this.socket = socket;
            this.endpoint = endpoint;
            this.api = api;
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await socket.ConnectAsync(new Uri(endpoint), cancellationToken).ConfigureAwait(false);
            await onConnectedAsync();
            runReadLoop(cancellationToken);
        }

        private void runReadLoop(CancellationToken cancellationToken) => Task.Run((Func<Task>)(async () =>
        {
            byte[] buffer = new byte[1024];
            StringBuilder messageResult = new StringBuilder();

            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, cancellationToken);

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

                                await onMessageReceivedAsync(message);
                            }

                            break;

                        case WebSocketMessageType.Binary:
                            throw new NotImplementedException();

                        case WebSocketMessageType.Close:
                            throw new Exception("Connection closed by remote host.");
                    }
                }
                catch (Exception ex)
                {
                    await InvokeClosed(ex);
                    return;
                }
            }
        }), cancellationToken);

        private async Task closeAsync()
        {
            try
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None).ConfigureAwait(false);
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

            await socket.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)), WebSocketMessageType.Text, true, cancellationToken);
        }

        public override async ValueTask DisposeAsync()
        {
            await base.DisposeAsync();
            await closeAsync();
            socket.Dispose();
        }
    }
}
