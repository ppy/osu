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
        private readonly object finalTaskLock = new object();
        private Task? finalTask;

        /// <summary>
        /// Adds a new task to the end of this <see cref="TaskChain"/>.
        /// </summary>
        /// <param name="action">The action to be executed.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/> for this task. Does not affect further tasks in the chain.</param>
        /// <returns>The awaitable <see cref="Task"/>.</returns>
        public async Task Add(Action action, CancellationToken cancellationToken = default)
        {
            Task? previousTask;
            Task currentTask;

            lock (finalTaskLock)
            {
                previousTask = finalTask;
                finalTask = currentTask = new Task(action, cancellationToken);
            }

            if (previousTask != null)
                await previousTask;

            currentTask.Start();
            await currentTask;
        }
    }
}
