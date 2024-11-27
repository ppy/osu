// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
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
        /// The current connection opened by this connector.
        /// </summary>
        HubConnection? CurrentConnection { get; }

        /// <summary>
        /// Whether this is connected to the hub, use <see cref="CurrentConnection"/> to access the connection, if this is <c>true</c>.
        /// </summary>
        IBindable<bool> IsConnected { get; }

        /// <summary>
        /// Invoked whenever a new hub connection is built, to configure it before it's started.
        /// </summary>
        public Action<HubConnection>? ConfigureConnection { get; set; }

        /// <summary>
        /// Forcefully disconnects the client from the server.
        /// </summary>
        Task Disconnect();

        /// <summary>
        /// Reconnect if already connected.
        /// </summary>
        Task Reconnect();
    }
}
