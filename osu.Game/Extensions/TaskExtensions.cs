// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions.ObjectExtensions;

namespace osu.Game.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Add a continuation to be performed only after the attached task has completed.
        /// </summary>
        /// <param name="task">The previous task to be awaited on.</param>
        /// <param name="action">The action to run.</param>
        /// <param name="cancellationToken">An optional cancellation token. Will only cancel the provided action, not the sequence.</param>
        /// <returns>A task representing the provided action.</returns>
        public static Task ContinueWithSequential(this Task task, Action action, CancellationToken cancellationToken = default) =>
            task.ContinueWithSequential(() => Task.Run(action, cancellationToken), cancellationToken);

        /// <summary>
        /// Add a continuation to be performed only after the attached task has completed.
        /// </summary>
        /// <param name="task">The previous task to be awaited on.</param>
        /// <param name="continuationFunction">The continuation to run. Generally should be an async function.</param>
        /// <param name="cancellationToken">An optional cancellation token. Will only cancel the provided action, not the sequence.</param>
        /// <returns>A task representing the provided action.</returns>
        public static Task ContinueWithSequential(this Task task, Func<Task> continuationFunction, CancellationToken cancellationToken = default)
        {
            var tcs = new TaskCompletionSource<bool>();

            task.ContinueWith(_ =>
            {
                // the previous task has finished execution or been cancelled, so we can run the provided continuation.

                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.SetCanceled(cancellationToken);
                }
                else
                {
                    continuationFunction().ContinueWith(continuationTask =>
                    {
                        if (cancellationToken.IsCancellationRequested || continuationTask.IsCanceled)
                        {
                            tcs.TrySetCanceled();
                        }
                        else if (continuationTask.IsFaulted)
                        {
                            tcs.TrySetException(continuationTask.Exception.AsNonNull());
                        }
                        else
                        {
                            tcs.TrySetResult(true);
                        }
                    }, cancellationToken: default);
                }
            }, cancellationToken: default);

            // importantly, we are not returning the continuation itself but rather a task which represents its status in sequential execution order.
            // this will not be cancelled or completed until the previous task has also.
            return tcs.Task;
        }
    }
}
