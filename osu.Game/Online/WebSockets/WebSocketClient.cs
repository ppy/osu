// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace osu.Game.Online.WebSockets
{
    public class WebSocketClient
    {
        private readonly WebSocket socket;
        private readonly Uri? uri;

        private Task? process;
        private CancellationTokenSource? cts;
        private bool stopping;

        public WebSocketClient(WebSocket socket)
        {
            this.socket = socket;
        }

        public WebSocketClient(Uri uri, IDictionary<string, string>? headers = null)
            : this(createClientWebSocket(headers))
        {
            this.uri = uri;
        }

        public async Task StartAsync(CancellationToken token = default)
        {
            if (stopping || cts != null || process != null)
                throw new InvalidOperationException(@"Can't start WebSocketClient a second time");

            if (socket is ClientWebSocket client && uri != null)
                await client.ConnectAsync(uri, token).ConfigureAwait(false);

            cts = new CancellationTokenSource();
            process = Task.Run(() => processIncomingMessages(cts.Token), cts.Token);
        }

        public async Task StopAsync(CancellationToken token = default)
        {
            if (cts == null || process == null || stopping)
                return;

            stopping = true;

            await OnClosing(token).ConfigureAwait(false);
            await CloseAsync(token).ConfigureAwait(false);
            await cts.CancelAsync().ConfigureAwait(false);
            await process.ConfigureAwait(false);

            socket.Dispose();

            cts?.Dispose();
            cts = null;

            process = null;
        }

        public async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken token = default)
        {
            await sendAsync(data, WebSocketMessageType.Binary, token).ConfigureAwait(false);
        }

        public async Task SendAsync(string message, CancellationToken token = default)
        {
            if (string.IsNullOrEmpty(message))
                return;

            using var buffer = MemoryPool<byte>.Shared.Rent(Encoding.UTF8.GetByteCount(message));
            int written = Encoding.UTF8.GetBytes(message, buffer.Memory.Span);

            if (written <= 0)
                return;

            await sendAsync(buffer.Memory.Slice(0, written), WebSocketMessageType.Text, token).ConfigureAwait(false);
        }

        protected virtual Task OnMessage(ReadOnlyMemory<byte> data, CancellationToken token = default) => Task.CompletedTask;

        protected virtual Task OnClosing(CancellationToken token = default) => Task.CompletedTask;

        protected virtual Task CloseAsync(CancellationToken token)
        {
            // When the close request is initiated by the client, WebSocket.CloseOutputAsync must be called so the websocket can transition
            // to the CloseSent state which is required to transition to the Closed state when the server acknowledges our close message.
            // See also: https://mcguirev10.com/2019/08/17/how-to-close-websocket-correctly.html
            if (socket.State == WebSocketState.Closed || socket.State == WebSocketState.CloseSent || socket.State == WebSocketState.Aborted)
                return Task.CompletedTask;

            return socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token);
        }

        private async Task sendAsync(ReadOnlyMemory<byte> data, WebSocketMessageType type, CancellationToken token = default)
        {
            if (data.IsEmpty || cts == null || cts.IsCancellationRequested || socket.State != WebSocketState.Open)
                return;

            await socket.SendAsync(data, type, true, token).ConfigureAwait(false);
        }

        private async Task processIncomingMessages(CancellationToken token)
        {
            int bytesRead = 0;
            var buffer = MemoryPool<byte>.Shared.Rent(ushort.MaxValue);

            try
            {
                while (!token.IsCancellationRequested)
                {
                    if (socket.State != WebSocketState.Open)
                        break;

                    var result = await socket.ReceiveAsync(buffer.Memory.Slice(bytesRead), token).ConfigureAwait(false);
                    bytesRead += result.Count;

                    // If the token is cancelled while ReceiveAsync is blocking, the socket state changes to aborted.
                    if (socket.State == WebSocketState.Aborted)
                        break;

                    // The server is notifying us that the connection will close.
                    if (socket.State == WebSocketState.CloseReceived && result.MessageType == WebSocketMessageType.Close)
                    {
                        await OnClosing(token).ConfigureAwait(false);
                        await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
                    }

                    // Data is sent in chunks. Only submit received data when an "end of message" packet has been received.
                    if (socket.State == WebSocketState.Open && result.MessageType != WebSocketMessageType.Close && result.EndOfMessage)
                    {
                        await OnMessage(buffer.Memory.Slice(0, bytesRead), token).ConfigureAwait(false);
                        bytesRead = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!(ex is OperationCanceledException || ex is ObjectDisposedException || ex is WebSocketException))
                {
                    throw;
                }
            }
            finally
            {
                buffer.Dispose();
            }
        }

        private static ClientWebSocket createClientWebSocket(IDictionary<string, string>? headers = null)
        {
            var client = new ClientWebSocket();

            if (headers != null)
            {
                foreach (var (key, value) in headers)
                    client.Options.SetRequestHeader(key, value);
            }

            return client;
        }
    }
}
