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
using osu.Game.Online.API;

namespace osu.Game.Online.Notifications.WebSocket
{
    public class OsuClientWebSocket : IAsyncDisposable
    {
        public event Func<SocketMessage, Task>? MessageReceived;
        public event Func<Exception, Task>? Closed;

        private readonly string endpoint;
        private readonly ClientWebSocket socket;

        private CancellationTokenSource? linkedTokenSource = null;

        public OsuClientWebSocket(IAPIProvider api, string endpoint)
        {
            socket = new ClientWebSocket();
            socket.Options.SetRequestHeader(@"Authorization", @$"Bearer {api.AccessToken}");
            socket.Options.Proxy = WebRequest.DefaultWebProxy;
            if (socket.Options.Proxy != null)
                socket.Options.Proxy.Credentials = CredentialCache.DefaultCredentials;

            this.endpoint = endpoint;
        }

        public async Task ConnectAsync(CancellationToken cancellationToken)
        {
            if (socket.State == WebSocketState.Connecting || socket.State == WebSocketState.Open)
                throw new InvalidOperationException("Connection is already opened");

            Debug.Assert(linkedTokenSource == null);
            linkedTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await socket.ConnectAsync(new Uri(endpoint), linkedTokenSource.Token).ConfigureAwait(false);
            runReadLoop(linkedTokenSource.Token);
        }

        private void runReadLoop(CancellationToken cancellationToken) => Task.Factory.StartNew(async () =>
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

                                await invokeMessageReceived(message).ConfigureAwait(false);
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
                    await invokeClosed(ex).ConfigureAwait(false);
                    return;
                }
            }
        }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);

        private async Task invokeMessageReceived(SocketMessage message)
        {
            if (MessageReceived == null)
                return;

            var invocationList = MessageReceived.GetInvocationList();

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (Func<SocketMessage, Task> handler in invocationList)
                await handler.Invoke(message).ConfigureAwait(false);
        }

        private async Task invokeClosed(Exception ex)
        {
            if (Closed == null)
                return;

            var invocationList = Closed.GetInvocationList();

            // ReSharper disable once PossibleInvalidCastExceptionInForeachLoop
            foreach (Func<Exception, Task> handler in invocationList)
                await handler.Invoke(ex).ConfigureAwait(false);
        }

        public Task SendMessage(SocketMessage message, CancellationToken cancellationToken)
        {
            if (socket.State != WebSocketState.Open)
                return Task.CompletedTask;

            return socket.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)), WebSocketMessageType.Text, true, cancellationToken);
        }

        public async Task DisconnectAsync()
        {
            linkedTokenSource?.Cancel();
            await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, @"Disconnecting", CancellationToken.None).ConfigureAwait(false);
            linkedTokenSource?.Dispose();
            linkedTokenSource = null;
        }

        public async ValueTask DisposeAsync()
        {
            try
            {
                await DisconnectAsync().ConfigureAwait(false);
            }
            catch
            {
                // Closure can fail if the connection is aborted. Don't really care since it's disposed anyway.
            }

            socket.Dispose();
        }
    }
}
