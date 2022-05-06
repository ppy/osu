// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Online.WebSockets
{
    [Cached]
    public abstract class WebSocketClient : CompositeDrawable
    {
        /// <summary>
        /// Whether this is current listening on the provided endpoint.
        /// </summary>
        public bool IsListening => listener?.IsListening ?? false;

        /// <summary>
        /// Gets the number of connections this client currently has.
        /// </summary>
        public int Connected => connections.Count;

        /// <summary>
        /// Gets the endpoint to listen on.
        /// </summary>
        public virtual string Endpoint => string.Empty;

        private HttpListener listener;
        private readonly List<WebSocketConnection> connections = new List<WebSocketConnection>();

        /// <summary>
        /// Starts the websocket client to listen for incoming connections.
        /// </summary>
        public void Start()
        {
            if (listener != null || !HttpListener.IsSupported)
                return;

            string path = Endpoint ?? string.Empty;
            path = path.EndsWith('/') ? path : path + '/';

            listener = new HttpListener();
            listener.Prefixes.Add(@$"http://localhost:7270/{path}");
            listener.Start();
            listener.BeginGetContext(handleRequest, null);
        }

        /// <summary>
        /// Closes the websocket client and stops listenning for incoming connections.
        /// </summary>
        public void Close()
        {
            if (listener == null)
                return;

            Task.Run(async () =>
            {
                var prev = listener;
                listener = null;

                prev.Stop();

                foreach (var connection in connections)
                    await connection.DisposeAsync().ConfigureAwait(false);

                connections.Clear();
                prev.Close();
            });
        }

        /// <summary>
        /// Broadcasts a message to all connections.
        /// </summary>
        public void Broadcast(string message)
        {
            lock (connections)
            {
                foreach (var connection in connections)
                    connection.Send(message);
            }
        }

        private void handleRequest(IAsyncResult result)
        {
            if (!listener.IsListening)
                return;

            try
            {
                var context = listener.EndGetContext(result);

                listener.BeginGetContext(handleRequest, listener);

                if (!context.Request.IsWebSocketRequest)
                {
                    context.Response.StatusCode = 400;
                    context.Response.Close();
                    return;
                }

                var request = context
                    .AcceptWebSocketAsync(null)
                    .ConfigureAwait(false)
                    .GetAwaiter()
                    .GetResult();

                var connection = new WebSocketConnection(request.WebSocket);
                connection.OnStart += onConnectionStart;
                connection.OnClose += onConnectionClose;
                connection.OnReady += onConnectionReady;
                connection.OnMessage += onConnectionMessage;

                connection.Start();
            }
            catch (HttpListenerException)
            {
            }
        }

        /// <summary>
        /// Called when a request for a websocket connection has been accepted.
        /// </summary>
        /// <param name="connection">The websocket connection that has been accepted.</param>
        protected virtual void OnConnectionStart(WebSocketConnection connection)
        {
        }

        /// <summary>
        /// Called when a websocket connection has been closed on request.
        /// </summary>
        /// <param name="connection">The websocket connection that closed.</param>
        /// <param name="requested">Whether this client initiated the close or not.</param>
        protected virtual void OnConnectionClose(WebSocketConnection connection, bool requested)
        {
        }

        /// <summary>
        /// Called when a websocket connection is ready to send and receive messages.
        /// </summary>
        /// <param name="connection">The websocket connection that is ready.</param>
        protected virtual void OnConnectionReady(WebSocketConnection connection)
        {
        }

        /// <summary>
        /// Called when a websocket connection sent a message.
        /// </summary>
        /// <param name="connection">The websocket connection that sent a message.</param>
        /// <param name="message">The message received.</param>
        protected virtual void OnConnectionMessage(WebSocketConnection connection, string message)
        {
        }

        private void onConnectionStart(object sender, EventArgs args)
        {
            var connection = (WebSocketConnection)sender;

            lock (connections)
                connections.Add(connection);

            OnConnectionStart(connection);
        }

        private void onConnectionClose(object sender, bool args)
        {
            var connection = (WebSocketConnection)sender;

            if (!args)
            {
                lock (connections)
                    connections.Remove(connection);
            }

            OnConnectionClose(connection, args);
        }

        private void onConnectionMessage(object sender, string args)
        {
            OnConnectionMessage((WebSocketConnection)sender, args);
        }

        private void onConnectionReady(object sender, EventArgs args)
        {
            OnConnectionReady((WebSocketConnection)sender);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Close();
        }

        protected class WebSocketConnection : IAsyncDisposable
        {
            public event EventHandler OnStart;
            public event EventHandler OnReady;
            public event EventHandler<bool> OnClose;
            public event EventHandler<string> OnMessage;

            private bool isDisposed;
            private bool isReady;
            private bool hasStarted;
            private readonly WebSocket socket;
            private readonly IMemoryOwner<byte> owner = MemoryPool<byte>.Shared.Rent();
            private readonly ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
            private readonly CancellationTokenSource cts = new CancellationTokenSource();

            public WebSocketConnection(WebSocket socket)
            {
                this.socket = socket;
            }

            public void Start()
            {
                if (hasStarted)
                    return;

                OnStart?.Invoke(this, EventArgs.Empty);
                Task.WhenAll(receive(cts.Token), send(cts.Token));

                hasStarted = true;
            }

            public void Send(string message)
                => queue.Enqueue(message);

            private async Task send(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    if (socket.State == WebSocketState.Closed || socket.State == WebSocketState.Aborted)
                        break;

                    if (socket.State == WebSocketState.Open && queue.TryDequeue(out string message))
                        await socket.SendAsync(Encoding.UTF8.GetBytes(message).AsMemory(), WebSocketMessageType.Text, true, token).ConfigureAwait(false);

                    if (!isReady)
                    {
                        OnReady?.Invoke(this, EventArgs.Empty);
                        isReady = true;
                    }
                }
            }

            private async Task receive(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    if (socket.State == WebSocketState.Closed || socket.State == WebSocketState.Aborted)
                        break;

                    var msg = await socket.ReceiveAsync(owner.Memory, token).ConfigureAwait(false);

                    if (msg.MessageType == WebSocketMessageType.Close)
                    {
                        await disposeAsync(false);
                        break;
                    }

                    if (msg.MessageType == WebSocketMessageType.Text)
                        OnMessage?.Invoke(this, Encoding.UTF8.GetString(owner.Memory.Slice(0, msg.Count).ToArray()));
                }
            }

            public async ValueTask DisposeAsync()
            {
                await disposeAsync(true);
            }

            private async Task disposeAsync(bool notifyConnection)
            {
                if (isDisposed)
                    return;

                OnClose?.Invoke(this, notifyConnection);

                if (notifyConnection)
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                else
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);

                cts.Cancel();
                cts.Dispose();

                owner.Dispose();
                socket.Dispose();

                isDisposed = true;
            }
        }
    }
}
