// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Bindables;

namespace osu.Game.Online.Notifications.WebSocket
{
    /// <summary>
    /// A client for asynchronous notifications sent by osu-web.
    /// </summary>
    public interface INotificationsClient
    {
        /// <summary>
        /// Whether this <see cref="INotificationsClient"/> is currently connected to a server.
        /// </summary>
        IBindable<bool> IsConnected { get; }

        /// <summary>
        /// Invoked when a new <see cref="SocketMessage"/> arrives for this client.
        /// </summary>
        event Action<SocketMessage>? MessageReceived;

        /// <summary>
        /// Sends a <see cref="SocketMessage"/> to the notification server.
        /// </summary>
        Task SendAsync(SocketMessage message, CancellationToken? cancellationToken = default);
    }
}
