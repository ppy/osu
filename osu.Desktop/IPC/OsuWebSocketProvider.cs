// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Extensions;
using osu.Framework.Graphics;
using osu.Framework.Logging;
using osu.Game.IPC;
using osu.Game.IPC.Messages;
using osu.Game.Online.Multiplayer;

namespace osu.Desktop.IPC
{
    public partial class OsuWebSocketProvider : Component, IWebSocketProvider
    {
        private WebSocketServer? server;
        private readonly List<WebSocketDataSource> dataSources = [];

        [BackgroundDependencyLoader]
        private void load()
        {
            int port = 49727;

            if (int.TryParse(Environment.GetEnvironmentVariable("OSU_WEBSOCKET_SERVER_PORT"), out int portOverride))
                port = portOverride;

            server = new WebSocketServer(port);
            server.ClientConnected += onClientConnected;
            server.StartAsync().FireAndForget(onError: ex => Logger.Error(ex, "Failed to start websocket"));
        }

        public void Register(WebSocketDataSource dataSource)
        {
            dataSources.Add(dataSource);
            dataSource.BroadcastMessageReceived += onDataSourceBroadcastMessageReceived;
            dataSource.ClientMessageReceived += onDataSourceClientMessageReceived;
        }

        public void Unregister(WebSocketDataSource dataSource)
        {
            dataSource.ClientMessageReceived -= onDataSourceClientMessageReceived;
            dataSource.BroadcastMessageReceived -= onDataSourceBroadcastMessageReceived;
            dataSources.Remove(dataSource);
        }

        private void onClientConnected(int clientId)
        {
            foreach (WebSocketDataSource dataSource in dataSources)
                dataSource.OnClientConnected(clientId).FireAndForget();
        }

        private void onDataSourceClientMessageReceived(int clientId, OsuWebSocketMessage message) =>
            Task.Run(async () =>
                {
                    if (server?.IsRunning != true)
                        return;

                    string messageString = JsonSerializer.Serialize(message, message.GetType());
                    await server.SendAsync(clientId, messageString).ConfigureAwait(false);
                })
                .FireAndForget();

        private void onDataSourceBroadcastMessageReceived(OsuWebSocketMessage message) =>
            Task.Run(async () =>
                {
                    if (server?.IsRunning != true)
                        return;

                    string messageString = JsonSerializer.Serialize(message, message.GetType());
                    await server.BroadcastAsync(messageString).ConfigureAwait(false);
                })
                .FireAndForget();

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (server?.IsRunning == true)
            {
                var cts = new CancellationTokenSource();
                cts.CancelAfter(TimeSpan.FromSeconds(10));
                server.StopAsync(cts.Token).WaitSafely();
                server.ClientConnected -= onClientConnected;
                server = null;
            }
        }
    }
}
