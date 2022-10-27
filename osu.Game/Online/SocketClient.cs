// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace osu.Game.Online
{
    public abstract class SocketClient : IAsyncDisposable
    {
        public event Func<Exception?, Task>? Closed;

        protected Task InvokeClosed(Exception? exception) => Closed?.Invoke(exception) ?? Task.CompletedTask;

        public abstract Task StartAsync(CancellationToken cancellationToken);

        public virtual ValueTask DisposeAsync()
        {
            Closed = null;
            return new ValueTask(Task.CompletedTask);
        }
    }
}
