// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;

namespace osu.Game.Utils
{
    /// <summary>
    /// A chain of <see cref="Task"/>s that run sequentially.
    /// </summary>
    public class TaskChain
    {
        private readonly object currentTaskLock = new object();
        private Task? currentTask;

        /// <summary>
        /// Adds a new task to the end of this <see cref="TaskChain"/>.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for this task. Does not affect further tasks in the chain.</param>
        /// <returns>The awaitable <see cref="Task"/>.</returns>
        public Task Add(Action action, CancellationToken cancellationToken = default)
        {
            lock (currentTaskLock)
            {
                // Note: Attaching the cancellation token to the continuation could lead to re-ordering of tasks in the chain.
                // Therefore, the cancellation token is not used to cancel the continuation but only the run of each task.
                if (currentTask == null)
                {
                    currentTask = Task.Run(() =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        action();
                    }, CancellationToken.None);
                }
                else
                {
                    currentTask = currentTask.ContinueWith(_ =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        action();
                    }, CancellationToken.None);
                }

                return currentTask;
            }
        }
    }
}
