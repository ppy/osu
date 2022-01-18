// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.IO;
using System.Net;
using osu.Framework.Logging;
using Sentry;

namespace osu.Game.Utils
{
    /// <summary>
    /// Report errors to sentry.
    /// </summary>
    public class SentryLogger : IDisposable
    {
        private SentryClient sentry;
        private Scope sentryScope;
        private Exception lastException;

        public SentryLogger(OsuGame game)
        {
            if (!game.IsDeployedBuild) return;

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
        }

        private bool shouldSubmitException(Exception exception)
        {
            switch (exception)
            {
                case IOException ioe:
                    // disk full exceptions, see https://stackoverflow.com/a/9294382
                    const int hr_error_handle_disk_full = unchecked((int)0x80070027);
                    const int hr_error_disk_full = unchecked((int)0x80070070);

                    if (ioe.HResult == hr_error_handle_disk_full || ioe.HResult == hr_error_disk_full)
                        return false;

                    break;

                case WebException we:
                    switch (we.Status)
                    {
                        // more statuses may need to be blocked as we come across them.
                        case WebExceptionStatus.Timeout:
                            return false;
                    }

                    break;
            }

            return true;
        }

        #region Disposal

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool isDisposing)
        {
            Logger.NewEntry -= processLogEntry;
            sentry = null;
            sentryScope = null;
        }

        #endregion
    }
}
