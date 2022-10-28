// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.Chat;

namespace osu.Game.Online.Notifications
{
    public class NotificationsClientConnector : SocketClientConnector
    {
        public event Action<Channel>? ChannelJoined;
        public event Action<List<Message>>? NewMessages;
        public event Action? PresenceReceived;

        private readonly IAPIProvider api;
        private bool chatStarted;

        public NotificationsClientConnector(IAPIProvider api)
            : base(api)
        {
            this.api = api;
        }

        public void StartChat()
        {
            chatStarted = true;

            if (CurrentConnection is NotificationsClient client)
                client.EnableChat = true;
        }

        protected override async Task<SocketClient> BuildConnectionAsync(CancellationToken cancellationToken)
        {
            var tcs = new TaskCompletionSource<string>();

            var req = new GetNotificationsRequest();
            req.Success += bundle => tcs.SetResult(bundle.Endpoint);
            req.Failure += ex => tcs.SetException(ex);
            api.Queue(req);

            string endpoint = await tcs.Task;

            ClientWebSocket socket = new ClientWebSocket();
            socket.Options.SetRequestHeader("Authorization", $"Bearer {api.AccessToken}");
            socket.Options.Proxy = WebRequest.DefaultWebProxy;
            if (socket.Options.Proxy != null)
                socket.Options.Proxy.Credentials = CredentialCache.DefaultCredentials;

            return new NotificationsClient(socket, endpoint, api)
            {
                ChannelJoined = c => ChannelJoined?.Invoke(c),
                NewMessages = m => NewMessages?.Invoke(m),
                PresenceReceived = () => PresenceReceived?.Invoke(),
                EnableChat = chatStarted
            };
        }
    }
}
