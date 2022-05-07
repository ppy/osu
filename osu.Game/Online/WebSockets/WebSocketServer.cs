// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Online.WebSockets
{
    public abstract class WebSocketServer : CompositeDrawable
    {
        /// <summary>
        /// Whether this is current listening.
        /// </summary>
        public bool IsListening => listener?.IsListening ?? false;

        /// <summary>
        /// Gets the host name to listen on.
        /// </summary>
        public virtual string HostName => @"localhost";

        /// <summary>
        /// Gets the endpoint to listen on.
        /// </summary>
        public virtual string Endpoint => string.Empty;

        /// <summary>
        /// Gets the port to listen on.
        /// </summary>
        public virtual int Port => 7270;

        /// <summary>
        /// Gets the number of connections this server currently has.
        /// </summary>
        public int Connected
        {
            get
            {
                semaphore.Wait();
                int count = connections.Count;
                semaphore.Release();

                return count;
            }
        }

        private HttpListener listener;
        private readonly SemaphoreSlim semaphore = new SemaphoreSlim(1);
        private readonly List<WebSocketConnection> connections = new List<WebSocketConnection>();

        /// <summary>
        /// Starts the websocket server to listen for incoming connections.
        /// </summary>
        public void Start()
        {
            if (listener != null || !HttpListener.IsSupported)
                return;

            string path = Endpoint ?? string.Empty;
            path = path.EndsWith('/') ? path : path + '/';

            listener = new HttpListener();
            listener.Prefixes.Add(@$"http://{HostName}:{Port}/{path}");
            listener.Start();
            listener.BeginGetContext(handleRequest, null);
        }

        /// <summary>
        /// Closes the websocket server and stops listening for incoming connections.
        /// </summary>
        public async Task Close()
        {
            if (listener == null)
                return;

            listener.Stop();
            listener.Close();
            listener = null;

            var disposeTasks = connections.Select(c => c.DisposeAsync().AsTask());
            await Task.WhenAll(disposeTasks);
            connections.Clear();
        }

        /// <summary>
        /// Broadcasts a message as text to all connections.
        /// </summary>
        public void Broadcast(string message)
        {
            semaphore.Wait();

            foreach (var connection in connections)
                connection.Send(message);

            semaphore.Release();
        }

        /// <summary>
        /// Broadcasts a message as binary to all connections.
        /// </summary>
        public void Broadcast(ReadOnlyMemory<byte> message)
        {
            semaphore.Wait();

            foreach (var connection in connections)
                connection.Send(message);

            semaphore.Release();
        }

        private void handleRequest(IAsyncResult result)
        {
            if (!IsListening)
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
        /// <param name="requested">Whether this server initiated the close or not.</param>
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
        protected virtual void OnConnectionMessage(WebSocketConnection connection, Message message)
        {
        }

        private void onConnectionStart(object sender, EventArgs args)
        {
            var connection = (WebSocketConnection)sender;

            semaphore.Wait();
            connections.Add(connection);
            semaphore.Release();

            OnConnectionStart(connection);
        }

        private void onConnectionClose(object sender, bool requested)
        {
            var connection = (WebSocketConnection)sender;

            if (!requested)
            {
                semaphore.Wait();
                connections.Remove(connection);
                semaphore.Release();
            }

            OnConnectionClose(connection, requested);
        }

        private void onConnectionMessage(object sender, Message args)
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
            Task.Run(() => Close()).ContinueWith(t => semaphore.Dispose());
        }

        protected class WebSocketConnection : IAsyncDisposable
        {
            public event EventHandler OnStart;
            public event EventHandler OnReady;
            public event EventHandler<bool> OnClose;
            public event EventHandler<Message> OnMessage;

            private bool isDisposed;
            private bool isReady;
            private bool hasStarted;
            private Task processTask;
            private readonly WebSocket socket;
            private readonly IMemoryOwner<byte> buffer = MemoryPool<byte>.Shared.Rent();
            private readonly CancellationTokenSource cts = new CancellationTokenSource();
            private readonly ConcurrentQueue<Message> queue = new ConcurrentQueue<Message>();

            public WebSocketConnection(WebSocket socket)
            {
                this.socket = socket;
            }

            public void Start()
            {
                if (hasStarted)
                    return;

                OnStart?.Invoke(this, EventArgs.Empty);
                processTask = Task.Run(() => Task.WhenAll(receive(cts.Token), send(cts.Token)), cts.Token);

                hasStarted = true;
            }

            public void Send(string data)
                => queue.Enqueue(new Message(Encoding.UTF8.GetBytes(data).AsMemory(), WebSocketMessageType.Text));

            public void Send(ReadOnlyMemory<byte> data)
                => queue.Enqueue(new Message(data, WebSocketMessageType.Binary));

            private async Task send(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    if (socket.State == WebSocketState.Closed || socket.State == WebSocketState.Aborted)
                        break;

                    if (socket.State == WebSocketState.Open && queue.TryDequeue(out var item))
                        await socket.SendAsync(item.Content, item.Type, true, token).ConfigureAwait(false);

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

                    var msg = await socket.ReceiveAsync(buffer.Memory, token).ConfigureAwait(false);

                    if (msg.MessageType == WebSocketMessageType.Close)
                    {
                        await disposeAsync(false);
                        break;
                    }
                    else
                    {
                        OnMessage?.Invoke(this, new Message(buffer.Memory.Slice(0, msg.Count), msg.MessageType));
                    }
                }
            }

            public async ValueTask DisposeAsync()
            {
                await disposeAsync(true);
            }

            private async Task disposeAsync(bool requested)
            {
                if (isDisposed)
                    return;

                OnClose?.Invoke(this, requested);

                cts.Cancel();

                try
                {
                    await processTask;
                }
                catch (OperationCanceledException)
                {
                }
                catch (AggregateException)
                {
                }

                if (requested)
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                else
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);

                cts.Dispose();
                buffer.Dispose();
                socket.Dispose();

                isDisposed = true;
            }
        }

        protected readonly struct Message
        {
            public readonly ReadOnlyMemory<byte> Content;
            public readonly WebSocketMessageType Type;

            public Message(ReadOnlyMemory<byte> content, WebSocketMessageType type)
            {
                Content = content;
                Type = type;
            }
        }
    }
}
