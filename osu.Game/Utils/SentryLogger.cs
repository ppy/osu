// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Utils
{
    /// <summary>
    /// Report errors to sentry.
    /// </summary>
    public class SentryLogger : IDisposable
    {
        //private Scope sentryScope;

        public SentryLogger(OsuGame game)
        {
            if (!game.IsDeployedBuild) return;
/*
            var options = new SentryOptions
            {
                Dsn = "https://5e342cd55f294edebdc9ad604d28bbd3@sentry.io/1255255",
                Release = game.Version
            };

            sentry = new SentryClient(options);
            sentryScope = new Scope(options);

            Logger.NewEntry += processLogEntry;
        }

        private void processLogEntry(LogEntry entry)
        {
            if (entry.Level < LogLevel.Verbose) return;

            var exception = entry.Exception;

            if (exception != null)
            {
                if (!shouldSubmitException(exception)) return;

                // since we let unhandled exceptions go ignored at times, we want to ensure they don't get submitted on subsequent reports.
                if (lastException != null && lastException.Message == exception.Message && exception.StackTrace.StartsWith(lastException.StackTrace, StringComparison.Ordinal)) return;

                    lastException = exception;
                    sentry.CaptureEvent(new SentryEvent(exception) { Message = entry.Message }, sentryScope);
                }
                else
                    sentryScope.AddBreadcrumb(DateTimeOffset.Now, entry.Message, entry.Target.ToString(), "navigation");
            };
            */
        }

        #region Disposal

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
