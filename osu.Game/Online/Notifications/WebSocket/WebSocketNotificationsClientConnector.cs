// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
    public class WebSocketNotificationsClientConnector : NotificationsClientConnector
    {
        private readonly IAPIProvider api;

        public WebSocketNotificationsClientConnector(IAPIProvider api)
            : base(api)
        {
            this.api = api;
        }

        protected override async Task<NotificationsClient> BuildNotificationClientAsync(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<string>();

            var req = new GetNotificationsRequest();
            req.Success += bundle => tcs.SetResult(bundle.Endpoint);
            req.Failure += ex => tcs.SetException(ex);
            api.Queue(req);

            string endpoint = await tcs.Task.ConfigureAwait(false);

            ClientWebSocket socket = new ClientWebSocket();
            socket.Options.SetRequestHeader(@"Authorization", @$"Bearer {api.AccessToken}");
            socket.Options.Proxy = WebRequest.DefaultWebProxy;
            if (socket.Options.Proxy != null)
                socket.Options.Proxy.Credentials = CredentialCache.DefaultCredentials;

            return new WebSocketNotificationsClient(socket, endpoint, api);
        }
    }
}
