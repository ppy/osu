// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace osu.Game.Online
{
    public abstract class PersistentEndpointClient : IAsyncDisposable
    {
        /// <summary>
        /// An event notifying the <see cref="PersistentEndpointClientConnector"/> that the connection has been closed
        /// </summary>
        public event Func<Exception?, Task>? Closed;

        /// <summary>
        /// Notifies the <see cref="PersistentEndpointClientConnector"/> that the connection has been closed.
        /// </summary>
        /// <param name="exception">The exception that the connection closed with.</param>
        protected Task InvokeClosed(Exception? exception) => Closed?.Invoke(exception) ?? Task.CompletedTask;

        /// <summary>
        /// Connects the client to the remote service to begin processing messages.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to stop processing messages.</param>
        public abstract Task ConnectAsync(CancellationToken cancellationToken);

        public virtual ValueTask DisposeAsync()
        {
            Closed = null;
            return new ValueTask(Task.CompletedTask);
        }
    }
}
