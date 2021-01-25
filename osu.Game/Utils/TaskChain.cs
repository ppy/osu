// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
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
        /// <param name="taskFunc">The task creation function.</param>
        /// <returns>The awaitable <see cref="Task"/>.</returns>
        public Task Add(Func<Task> taskFunc)
        {
            lock (currentTaskLock)
            {
                currentTask = currentTask == null
                    ? taskFunc()
                    : currentTask.ContinueWith(_ => taskFunc()).Unwrap();
                return currentTask;
            }
        }
    }
}
