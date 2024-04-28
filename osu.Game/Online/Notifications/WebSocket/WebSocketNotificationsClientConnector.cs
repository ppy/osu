// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;

namespace osu.Game.Online.Notifications.WebSocket
{
    /// <summary>
    /// A connector for <see cref="WebSocketNotificationsClient"/>s that receive events via a websocket.
    /// </summary>
    public class WebSocketNotificationsClientConnector : PersistentEndpointClientConnector, INotificationsClient
    {
        public event Action<SocketMessage>? MessageReceived;

        private readonly IAPIProvider api;

        public WebSocketNotificationsClientConnector(IAPIProvider api)
            : base(api)
        {
            this.api = api;
            Start();
        }

        protected override async Task<PersistentEndpointClient> BuildConnectionAsync(CancellationToken cancellationToken)
        {
            var req = new GetNotificationsRequest();
            // must use `PerformAsync()`, since we may not be fully online yet
            // (see `APIState.RequiresSecondFactorAuth` - in this state queued requests will not execute).
            await api.PerformAsync(req).ConfigureAwait(false);
            string endpoint = req.Response!.Endpoint;

            ClientWebSocket socket = new ClientWebSocket();
            socket.Options.SetRequestHeader(@"Authorization", @$"Bearer {api.AccessToken}");
            socket.Options.Proxy = WebRequest.DefaultWebProxy;
            if (socket.Options.Proxy != null)
                socket.Options.Proxy.Credentials = CredentialCache.DefaultCredentials;

            var client = new WebSocketNotificationsClient(socket, endpoint);
            client.MessageReceived += msg => MessageReceived?.Invoke(msg);
            return client;
        }

        public Task SendAsync(SocketMessage message, CancellationToken? cancellationToken = default)
        {
            if (CurrentConnection is not WebSocketNotificationsClient webSocketClient)
                return Task.CompletedTask;

            return webSocketClient.SendAsync(message, cancellationToken);
        }
    }
}
