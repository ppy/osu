// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;

namespace osu.Game.Online.Notifications.WebSocket
{
    public interface INotificationsClient
    {
        IBindable<bool> IsConnected { get; }
        event Action<SocketMessage>? MessageReceived;
        Task SendAsync(SocketMessage message, CancellationToken? cancellationToken = default);
    }
}
