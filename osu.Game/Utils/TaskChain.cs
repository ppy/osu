// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Game.Extensions;

namespace osu.Game.Utils
{
    /// <summary>
    /// A chain of <see cref="Task"/>s that run sequentially.
    /// </summary>
    public class TaskChain
    {
        private readonly object taskLock = new object();

        private Task lastTaskInChain = Task.CompletedTask;

        /// <summary>
        /// Adds a new task to the end of this <see cref="TaskChain"/>.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for this task. Does not affect further tasks in the chain.</param>
        /// <returns>The awaitable <see cref="Task"/>.</returns>
        public Task Add(Action action, CancellationToken cancellationToken = default)
        {
            lock (taskLock)
                return lastTaskInChain = lastTaskInChain.ContinueWithSequential(action, cancellationToken);
        }

        /// <summary>
        /// Adds a new task to the end of this <see cref="TaskChain"/>.
        /// </summary>
        /// <param name="task">The task to be executed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for this task. Does not affect further tasks in the chain.</param>
        /// <returns>The awaitable <see cref="Task"/>.</returns>
        public Task Add(Func<Task> task, CancellationToken cancellationToken = default)
        {
            lock (taskLock)
                return lastTaskInChain = lastTaskInChain.ContinueWithSequential(task, cancellationToken);
        }
    }
}
