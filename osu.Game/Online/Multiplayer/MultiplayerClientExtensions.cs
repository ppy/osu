// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using osu.Framework.Logging;

namespace osu.Game.Online.Multiplayer
{
    public static class MultiplayerClientExtensions
    {
        public static void FireAndForget(this Task task, Action? onSuccess = null, Action<Exception>? onError = null) =>
            task.ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    Exception? exception = t.Exception;

                    if (exception is AggregateException ae)
                        exception = ae.InnerException;

                    Debug.Assert(exception != null);

                    if (exception.GetHubExceptionMessage() is string message)
                        // Hub exceptions generally contain something we can show the user directly.
                        Logger.Log(message, level: LogLevel.Important);
                    else
                        Logger.Error(exception, $"Unobserved exception occurred via {nameof(FireAndForget)} call: {exception.Message}");

                    onError?.Invoke(exception);
                }
                else
                {
                    onSuccess?.Invoke();
                }
            });

        public static string? GetHubExceptionMessage(this Exception exception)
        {
            if (exception is HubException hubException)
                // HubExceptions arrive with additional message context added, but we want to display the human readable message:
                // "An unexpected error occurred invoking 'AddPlaylistItem' on the server.InvalidStateException: Can't enqueue more than 3 items at once."
                // We generally use the message field for a user-parseable error (eventually to be replaced), so drop the first part for now.
                return hubException.Message.Substring(exception.Message.IndexOf(':') + 1).Trim();

            return null;
        }
    }
}
