// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using osu.Framework.Bindables;
using osu.Game.Online.API;

namespace osu.Game.Online
{
    /// <summary>
    /// A component that manages the life cycle of a connection to a SignalR Hub.
    /// Should generally be retrieved from an <see cref="IAPIProvider"/>.
    /// </summary>
    public interface IHubClientConnector : IDisposable
    {
        /// <summary>
        /// Whether the user is connected.
        /// </summary>
        IBindable<bool> IsConnected { get; }

        /// <summary>
        /// Invoked whenever a new hub connection is built, to configure it before it's started.
        /// </summary>
        Action<HubConnection>? ConfigureConnection { get; set; }

        /// <summary>
        /// Forcefully disconnects the client from the server.
        /// </summary>
        Task Disconnect();

        /// <summary>
        /// Reconnect if already connected.
        /// </summary>
        Task Reconnect();

        Task InvokeAsync(string name, object?[]? args = null, CancellationToken cancellationToken = default);
        Task<TResult> InvokeAsync<TResult>(string name, object?[]? args = null, CancellationToken cancellationToken = default);
        Task SendAsync(string name, object?[]? args = null, CancellationToken cancellationToken = default);
    }
}
