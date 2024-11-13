// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Buffers;
using System.Collections.Concurrent;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Graphics.Containers;

namespace osu.Desktop.WebSockets
{
    public abstract partial class WebSocketServer : CompositeDrawable
    {
        /// <summary>
        /// Whether this is current listening.
        /// </summary>
        public bool IsListening { get; private set; }

        /// <summary>
        /// Gets the address to listen on.
        /// </summary>
        public virtual string Address => @"localhost";

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
        public int Connected => connections.Count;

        private readonly HttpListener httpListener = new HttpListener();
        private readonly ConcurrentDictionary<int, WebSocketConnection> connections = new ConcurrentDictionary<int, WebSocketConnection>();

        private int nextClientId = -1;
        private CancellationTokenSource cancellationTokenSource = null!;

        public void Start()
        {
            if (IsListening)
                return;

            cancellationTokenSource = new CancellationTokenSource();

            string endpoint = Endpoint.StartsWith('/') ? Endpoint : $"/{Endpoint}";

            if (!endpoint.EndsWith('/'))
                endpoint = $"{endpoint}/";

            httpListener.Prefixes.Add($@"http://{Address}:{Port}{endpoint}");

            httpListener.Start();
            IsListening = true;

            Task.Factory.StartNew(httpListenerLoop, TaskCreationOptions.LongRunning);
        }

        public async Task Stop()
        {
            if (!IsListening)
                return;

            await cancellationTokenSource.CancelAsync().ConfigureAwait(false);

            var disposeTasks = connections.Values.Select(c => c.DisposeAsync().AsTask());
            await Task.WhenAll(disposeTasks).ConfigureAwait(false);

            connections.Clear();
            httpListener.Stop();
            httpListener.Prefixes.Clear();

            IsListening = false;
        }

        /// <summary>
        /// Broadcasts a message as text to all connections.
        /// </summary>
        public void Broadcast(string message)
        {
            if (!IsListening)
                return;

            foreach (var connection in connections.Values)
                connection.Send(message);
        }

        /// <summary>
        /// Broadcasts a message as binary to all connections.
        /// </summary>
        public void Broadcast(ReadOnlyMemory<byte> message)
        {
            if (!IsListening)
                return;

            foreach (var connection in connections.Values)
                connection.Send(message);
        }

        private async Task httpListenerLoop()
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                var context = await httpListener.GetContextAsync().ConfigureAwait(false);

                if (!context.Request.IsWebSocketRequest)
                {
                    context.Response.Abort();
                    continue;
                }

                var webSocketContext = await context.AcceptWebSocketAsync(subProtocol: null).ConfigureAwait(false);

                var connection = new WebSocketConnection(Interlocked.Increment(ref nextClientId), webSocketContext.WebSocket);
                connection.OnStart += onConnectionStart;
                connection.OnClose += onConnectionClose;
                connection.OnReady += onConnectionReady;
                connection.OnMessage += onConnectionMessage;

                await connection.Start().ConfigureAwait(false);
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

        private void onConnectionStart(object? sender, EventArgs args)
        {
            ArgumentNullException.ThrowIfNull(sender);

            var connection = (WebSocketConnection)sender;

            connections.TryAdd(connection.ID, connection);

            OnConnectionStart(connection);
        }

        private void onConnectionClose(object? sender, bool requested)
        {
            ArgumentNullException.ThrowIfNull(sender);

            var connection = (WebSocketConnection)sender;

            if (!requested)
            {
                connections.TryRemove(connection.ID, out _);
            }

            OnConnectionClose(connection, requested);
        }

        private void onConnectionMessage(object? sender, Message args)
        {
            ArgumentNullException.ThrowIfNull(sender);

            OnConnectionMessage((WebSocketConnection)sender, args);
        }

        private void onConnectionReady(object? sender, EventArgs args)
        {
            ArgumentNullException.ThrowIfNull(sender);

            OnConnectionReady((WebSocketConnection)sender);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Task.Run(Stop);
        }

        protected class WebSocketConnection : IAsyncDisposable
        {
            public readonly int ID;
            public event EventHandler? OnStart;
            public event EventHandler? OnReady;
            public event EventHandler<bool>? OnClose;
            public event EventHandler<Message>? OnMessage;

            private bool isDisposed;
            private bool isReady;
            private bool hasStarted;
            private Task? processTask;
            private readonly WebSocket socket;
            private readonly TaskCompletionSource completionSource = new TaskCompletionSource();
            private readonly IMemoryOwner<byte> buffer = MemoryPool<byte>.Shared.Rent();
            private readonly CancellationTokenSource cts = new CancellationTokenSource();
            private readonly ConcurrentQueue<Message> queue = new ConcurrentQueue<Message>();

            public WebSocketConnection(int id, WebSocket socket)
            {
                ID = id;
                this.socket = socket;
            }

            public async Task Start()
            {
                if (hasStarted)
                    return;

                OnStart?.Invoke(this, EventArgs.Empty);
                processTask = Task.Factory.StartNew(() => Task.WhenAll(receive(cts.Token), send(cts.Token)), cts.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);

                hasStarted = true;

                await completionSource.Task.ConfigureAwait(false);
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

                    try
                    {
                        if (socket.State == WebSocketState.Open && queue.TryDequeue(out var item))
                            await socket.SendAsync(item.Content, item.Type, true, token).ConfigureAwait(false);
                    }
                    catch (WebSocketException webSocketException) when (webSocketException.InnerException is HttpListenerException httpListenerException && httpListenerException.ErrorCode == 995)
                    {
                        // SendAsync throws as the socket state changes to aborted when its token has requested cancellation
                    }

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

                    try
                    {
                        var msg = await socket.ReceiveAsync(buffer.Memory, token).ConfigureAwait(false);

                        if (msg.MessageType == WebSocketMessageType.Close && socket.State == WebSocketState.CloseReceived)
                        {
                            await disposeAsync(false).ConfigureAwait(false);
                            break;
                        }
                        else
                        {
                            OnMessage?.Invoke(this, new Message(buffer.Memory[..msg.Count], msg.MessageType));
                        }
                    }
                    catch (WebSocketException webSocketException) when (webSocketException.InnerException is HttpListenerException httpListenerException && httpListenerException.ErrorCode == 995)
                    {
                        // ReceiveAsync throws as the socket state changes to aborted when its token has requested cancellation.
                    }
                }
            }

            public async ValueTask DisposeAsync()
            {
                await disposeAsync(true).ConfigureAwait(false);
            }

            private async Task disposeAsync(bool requested)
            {
                if (isDisposed)
                    return;

                OnClose?.Invoke(this, requested);

                if (requested)
                {
                    try
                    {
                        var closeCancellationToken = new CancellationTokenSource(10000);
                        await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, closeCancellationToken.Token).ConfigureAwait(false);
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }
                else
                {
                    await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
                }

                await cts.CancelAsync().ConfigureAwait(false);

                if (processTask is not null)
                    await processTask.ConfigureAwait(false);

                completionSource.SetResult();

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
