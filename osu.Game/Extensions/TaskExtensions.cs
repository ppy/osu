// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Extensions.ExceptionExtensions;
using osu.Framework.Logging;

namespace osu.Game.Extensions
{
    public static class TaskExtensions
    {
        /// <summary>
        /// Denote a task which is to be run without local error handling logic, where failure is not catastrophic.
        /// Avoids unobserved exceptions from being fired.
        /// </summary>
        /// <param name="task">The task.</param>
        /// <param name="logAsError">
        /// Whether errors should be logged as errors visible to users, or as debug messages.
        /// Logging as debug will essentially silence the errors on non-release builds.
        /// </param>
        public static Task CatchUnobservedExceptions(this Task task, bool logAsError = false)
        {
            return task.ContinueWith(t =>
            {
                Exception? exception = t.Exception?.AsSingular();
                if (logAsError)
                    Logger.Error(exception, $"Error running task: {exception?.Message ?? "(unknown)"}", LoggingTarget.Runtime, true);
                else
                    Logger.Log($"Error running task: {exception}", LoggingTarget.Runtime, LogLevel.Debug);
            }, TaskContinuationOptions.NotOnRanToCompletion);
        }

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

            task.ContinueWith(t =>
            {
                // the previous task has finished execution or been cancelled, so we can run the provided continuation.

                if (cancellationToken.IsCancellationRequested)
                {
                    tcs.SetCanceled();
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
                            tcs.TrySetException(continuationTask.Exception);
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
