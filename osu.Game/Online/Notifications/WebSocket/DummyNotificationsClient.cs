// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;

namespace osu.Game.Online.Notifications.WebSocket
{
    public class DummyNotificationsClient : INotificationsClient
    {
        public IBindable<bool> IsConnected => new BindableBool(true);

        public event Action<SocketMessage>? MessageReceived;

        public Func<SocketMessage, bool>? HandleMessage;

        public Task SendAsync(SocketMessage message, CancellationToken? cancellationToken = default)
        {
            if (HandleMessage?.Invoke(message) != true)
                throw new InvalidOperationException($@"{nameof(DummyNotificationsClient)} cannot process this message.");

            return Task.CompletedTask;
        }

        public void Receive(SocketMessage message) => MessageReceived?.Invoke(message);
    }
}
