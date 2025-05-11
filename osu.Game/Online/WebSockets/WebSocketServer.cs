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
        public int Connected => connections.Count;

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

            listener.Stop();

            await cts.CancelAsync().ConfigureAwait(false);
            await process.ConfigureAwait(false);

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

        protected virtual Task OnMessage(int id, ReadOnlyMemory<byte> data, CancellationToken token = default) => Task.CompletedTask;

        protected virtual Task OnConnect(int id, CancellationToken token = default) => Task.CompletedTask;

        protected virtual Task OnDisconnect(int id, CancellationToken token = default) => Task.CompletedTask;

        private async Task handleRequest(CancellationToken token)
        {
            if (listener == null)
                return;

            int id = 0;

            try
            {
                while (!token.IsCancellationRequested)
                {
                    var context = await listener.GetContextAsync().ConfigureAwait(false);

                    if (context.Request.IsWebSocketRequest)
                    {
                        var wsContext = await context.AcceptWebSocketAsync(null).ConfigureAwait(false);
                        int next = Interlocked.Increment(ref id);

                        var connection = new WebSocketConnection(this, id, wsContext.WebSocket);
                        connections.TryAdd(next, connection);

                        await connection.StartAsync(token).ConfigureAwait(false);
                        await OnConnect(next, token).ConfigureAwait(false);
                    }
                    else
                    {
                        context.Response.StatusCode = 500;
                        context.Response.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                // An HttpListenerException with the error code 995 (ERROR_OPERATION_ABORTED) is thrown
                // when we call HttpListener.Stop on another thread while HttpListener.GetContextAsync is
                // blocking in this thread. It is safe to ignore.
                if (!(ex is HttpListenerException hx && hx.ErrorCode == 995))
                {
                    throw;
                }
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

            protected override Task OnMessage(ReadOnlyMemory<byte> data, CancellationToken token = default)
            {
                return server.OnMessage(id, data, token);
            }

            protected override async Task OnClosing(CancellationToken token = default)
            {
                server.connections.TryRemove(id, out _);
                await server.OnDisconnect(id, token).ConfigureAwait(false);
                await base.OnClosing(token).ConfigureAwait(false);
            }

            protected override async Task CloseAsync(WebSocket socket, CancellationToken token)
            {
                // When the close request is initiated by the server, CloseAsync must be called to notify the client
                // that the socket is being closed from the server's side.
                // See also: https://mcguirev10.com/2019/08/17/how-to-close-websocket-correctly.html
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, token).ConfigureAwait(false);
            }
        }
    }
}
