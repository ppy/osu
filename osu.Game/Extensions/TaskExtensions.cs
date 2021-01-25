// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
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
        public static void CatchUnobservedExceptions(this Task task, bool logAsError = false)
        {
            task.ContinueWith(t =>
            {
                Exception? exception = t.Exception?.AsSingular();
                if (logAsError)
                    Logger.Error(exception, $"Error running task: {exception?.Message ?? "(unknown)"}", LoggingTarget.Runtime, true);
                else
                    Logger.Log($"Error running task: {exception}", LoggingTarget.Runtime, LogLevel.Debug);
            }, TaskContinuationOptions.NotOnRanToCompletion);
        }
    }
}
