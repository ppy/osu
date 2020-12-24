// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Threading.Tasks;
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
        /// <param name="logOnError">Whether errors should be logged as important, or silently ignored.</param>
        public static void CatchUnobservedExceptions(this Task task, bool logOnError = false)
        {
            task.ContinueWith(t =>
            {
                if (logOnError)
                    Logger.Log($"Error running task: {t.Exception?.Message ?? "unknown"}", LoggingTarget.Runtime, LogLevel.Important);
            }, TaskContinuationOptions.NotOnRanToCompletion);
        }
    }
}
