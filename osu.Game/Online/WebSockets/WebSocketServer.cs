// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace osu.Game.Online.WebSockets
{
    public partial class WebSocketServer
    {
        private int connectionId = -1;
        private HttpListener? listener;
        private Task? process;
        private CancellationTokenSource? cts;
        private readonly ConcurrentDictionary<int, WebSocketConnection> connections = new ConcurrentDictionary<int, WebSocketConnection>();

        public void Start(Uri uri)
        {
            if (listener != null)
                return;

            cts = new CancellationTokenSource();

            listener = new HttpListener();
            listener.Prefixes.Add(uri.ToString());

            listener.Start();
            process = Task.Run(() => handleRequest(cts.Token), cts.Token);
        }

        public async Task StopAsync(CancellationToken token = default)
        {
            if (listener == null || process == null || cts == null)
                return;

            try
            {
                await Task.WhenAll(connections.Values.Select(connection => connection.StopAsync(token))).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }

            cts.Cancel();
            listener.Stop();

            try
            {
                await process.ConfigureAwait(false);
            }
            catch (HttpListenerException)
            {
            }

            listener = null;

            cts.Dispose();
            cts = null;

            process = null;
        }

        public async Task SendAsync(ReadOnlyMemory<byte> data, CancellationToken token = default)
        {
            await Task.WhenAll(connections.Values.Select(connection => connection.SendAsync(data, token))).ConfigureAwait(false);
        }

        public async Task SendAsync(string message, CancellationToken token = default)
        {
            await Task.WhenAll(connections.Values.Select(connection => connection.SendAsync(message, token))).ConfigureAwait(false);
        }

        protected virtual Task OnMessageReceived(int id, ReadOnlyMemory<byte> data, CancellationToken token) => Task.CompletedTask;

        private async Task handleRequest(CancellationToken token)
        {
            if (listener == null)
                return;

            while (!token.IsCancellationRequested)
            {
                var context = await listener.GetContextAsync().ConfigureAwait(false);

                if (!await handleWebSocketRequest(context, token).ConfigureAwait(false))
                {
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
            }
        }

        private async Task<bool> handleWebSocketRequest(HttpListenerContext context, CancellationToken token)
        {
            try
            {
                var wsContext = await context.AcceptWebSocketAsync(null).ConfigureAwait(false);
                int nextId = Interlocked.Increment(ref connectionId);

                var connection = new WebSocketConnection(this, nextId, wsContext.WebSocket);
                connections.TryAdd(nextId, connection);
                await connection.StartAsync(token).ConfigureAwait(false);

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private class WebSocketConnection : WebSocketClient
        {
            private readonly int id;
            private readonly WebSocketServer server;

            public WebSocketConnection(WebSocketServer server, int id, WebSocket socket)
                : base(socket)
            {
                this.id = id;
                this.server = server;
            }

            protected override Task OnMessageReceived(ReadOnlyMemory<byte> data, CancellationToken token)
            {
                return server.OnMessageReceived(id, data, token);
            }

            protected override async Task CloseAsync(WebSocket socket, CancellationToken token)
            {
                // When the close request is initiated by the server, CloseAsync must be called to notify the client
                // that the socket is being closed from the server's side.
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token).ConfigureAwait(false);
                server.connections.TryRemove(id, out _);
            }
        }
    }
}
