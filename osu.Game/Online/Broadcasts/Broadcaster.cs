// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Containers;

namespace osu.Game.Online.Broadcasts
{
    [Cached]
    public class Broadcaster : CompositeDrawable
    {
        public bool IsListening => listener?.IsListening ?? false;
        public int Connected => clients.Count;

        private HttpListener listener;
        private readonly List<WebSocketClient> clients = new List<WebSocketClient>();
        private static readonly string prefix = @"http://localhost:7270/";

        public void Start()
        {
            if (listener != null || !HttpListener.IsSupported)
                return;

            listener = new HttpListener();
            listener.Prefixes.Add(prefix);
            listener.Start();
            listener.BeginGetContext(handleRequest, listener);
        }

        public void Close()
        {
            if (listener == null)
                return;

            Task.Run(async () =>
            {
                var prev = listener;
                listener = null;

                prev.Stop();

                foreach (var client in clients)
                    await client.DisposeAsync().ConfigureAwait(false);

                clients.Clear();
                prev.Close();
            });
        }

        public void Broadcast(string message)
        {
            lock (clients)
            {
                foreach (var client in clients)
                    client.Send(message);
            }
        }

        private void handleRequest(IAsyncResult result)
        {
            var listener = (HttpListener)result.AsyncState;

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

                var client = new WebSocketClient(request.WebSocket);
                client.OnStart += onClientStart;
                client.OnClose += onClientClose;

                client.Start();
            }
            catch (HttpListenerException)
            {
            }
        }

        private void onClientStart(object sender, EventArgs args)
        {
            lock (clients)
                clients.Add((WebSocketClient)sender);
        }

        private void onClientClose(object sender, EventArgs args)
        {
            lock (clients)
                clients.Remove((WebSocketClient)sender);
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);
            Close();
        }

        private class WebSocketClient : IAsyncDisposable
        {
            public event EventHandler OnClose;
            public event EventHandler OnStart;

            private bool isDisposed;
            private WebSocket socket;
            private readonly ConcurrentQueue<string> queue = new ConcurrentQueue<string>();
            private readonly CancellationTokenSource cts = new CancellationTokenSource();

            public WebSocketClient(WebSocket socket)
            {
                this.socket = socket;
            }

            public void Start()
            {
                OnStart?.Invoke(this, EventArgs.Empty);
                Task.WhenAll(send(cts.Token), receive(cts.Token));
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
                        await socket.SendAsync(new ReadOnlyMemory<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, token).ConfigureAwait(false);
                }
            }

            private async Task receive(CancellationToken token)
            {
                while (!token.IsCancellationRequested)
                {
                    if (socket.State == WebSocketState.Closed || socket.State == WebSocketState.Aborted)
                        break;

                    var buf = WebSocket.CreateServerBuffer(4096);
                    var msg = await socket.ReceiveAsync(buf, token).ConfigureAwait(false);

                    if (msg.MessageType == WebSocketMessageType.Close)
                    {
                        OnClose?.Invoke(this, EventArgs.Empty);
                        await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                        dispose();
                        break;
                    }
                }
            }

            public async ValueTask DisposeAsync()
            {
                dispose();

                if (socket != null)
                {
                    await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
                    socket.Dispose();
                    socket = null;
                }
            }

            private void dispose()
            {
                if (isDisposed)
                    return;

                cts.Cancel();
                cts.Dispose();

                socket.Dispose();

                isDisposed = true;
            }
        }
    }
}
