// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game.Online.API.Requests.Responses;
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
        private Exception? lastException;

        private IBindable<APIUser>? localUser;

        public SentryLogger(OsuGame game)
        {
            if (!game.IsDeployedBuild) return;

            var options = new SentryOptions
            {
                Dsn = "https://ad9f78529cef40ac874afb95a9aca04e@sentry.ppy.sh/2",
                AutoSessionTracking = true,
                IsEnvironmentUser = false,
                Release = game.Version
            };

            sentry = new SentryClient(options);
            sentryScope = new Scope(options);

            Logger.NewEntry += processLogEntry;
        }

        public void AttachUser(IBindable<APIUser> user)
        {
            Debug.Assert(localUser == null);

            localUser = user.GetBoundCopy();
            localUser.BindValueChanged(u =>
            {
                sentryScope.User = new User
                {
                    Username = u.NewValue.Username,
                    Id = u.NewValue.Id.ToString(),
                };
            }, true);
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

                sentry.CaptureEvent(new SentryEvent(exception)
                {
                    Message = entry.Message,
                    Level = getSentryLevel(entry.Level),
                }, sentryScope);
            }
            else
                sentryScope.AddBreadcrumb(DateTimeOffset.Now, entry.Message, entry.Target.ToString(), "navigation");
        }

        private SentryLevel? getSentryLevel(LogLevel entryLevel)
        {
            switch (entryLevel)
            {
                case LogLevel.Debug:
                    return SentryLevel.Debug;

                case LogLevel.Verbose:
                    return SentryLevel.Info;

                case LogLevel.Important:
                    return SentryLevel.Warning;

                case LogLevel.Error:
                    return SentryLevel.Error;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entryLevel), entryLevel, null);
            }
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
        }

        #endregion
    }
}
