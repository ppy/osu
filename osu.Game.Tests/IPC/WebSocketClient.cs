// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.IPC;

namespace osu.Game.Tests.IPC
{
    public sealed class WebSocketClient : IDisposable
    {
        public event Action<string>? MessageReceived;
        public event Action? Closed;

        private readonly int port;
        private WebSocketChannel? channel;

        public WebSocketClient(int port)
        {
            this.port = port;
        }

        public async Task Start(CancellationToken cancellationToken = default)
        {
            var webSocket = new ClientWebSocket();
            await webSocket.ConnectAsync(new Uri($@"ws://localhost:{port}/"), cancellationToken);
            channel = new WebSocketChannel(webSocket);
            channel.MessageReceived += msg => MessageReceived?.Invoke(msg);
            channel.ClosedPrematurely += () => Closed?.Invoke();
            channel.Start(cancellationToken);
        }

        public async Task SendAsync(string message)
        {
            if (channel == null)
                throw new InvalidOperationException($@"Must {nameof(Start)} first.");

            await channel.SendAsync(message);
        }

        public async Task StopAsync(CancellationToken stoppingToken = default)
        {
            try
            {
                if (channel != null)
                    await channel.StopAsync(stoppingToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // has to be caught manually because outer task isn't accepting `stoppingToken`.
            }
        }

        public void Dispose()
        {
            channel?.Dispose();
        }
    }
}
