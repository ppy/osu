// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

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
            return new WebSocketNotificationsClient(api, endpoint);
        }
    }
}
