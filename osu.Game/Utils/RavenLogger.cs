// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using osu.Framework.Logging;
using SharpRaven;
using SharpRaven.Data;

namespace osu.Game.Utils
{
    /// <summary>
    /// Report errors to sentry.
    /// </summary>
    public class RavenLogger : IDisposable
    {
        private readonly RavenClient raven = new RavenClient("https://5e342cd55f294edebdc9ad604d28bbd3@sentry.io/1255255");

        private readonly List<Task> tasks = new List<Task>();

        public RavenLogger(OsuGame game)
        {
            raven.Release = game.Version;

            if (!game.IsDeployedBuild) return;

            Exception lastException = null;

            Logger.NewEntry += entry =>
            {
                if (entry.Level < LogLevel.Verbose) return;

                var exception = entry.Exception;

                if (exception != null)
                {
                    if (!shouldSubmitException(exception))
                        return;

                    // since we let unhandled exceptions go ignored at times, we want to ensure they don't get submitted on subsequent reports.
                    if (lastException != null &&
                        lastException.Message == exception.Message && exception.StackTrace.StartsWith(lastException.StackTrace))
                        return;

                    lastException = exception;
                    queuePendingTask(raven.CaptureAsync(new SentryEvent(exception) { Message = entry.Message }));
                }
                else
                    raven.AddTrail(new Breadcrumb(entry.Target.ToString(), BreadcrumbType.Navigation) { Message = entry.Message });
            };
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

        private void queuePendingTask(Task<string> task)
        {
            lock (tasks) tasks.Add(task);
            task.ContinueWith(_ =>
            {
                lock (tasks)
                    tasks.Remove(task);
            });
        }

        #region Disposal

        ~RavenLogger()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private bool isDisposed;

        protected virtual void Dispose(bool isDisposing)
        {
            if (isDisposed)
                return;

            isDisposed = true;
            lock (tasks) Task.WaitAll(tasks.ToArray(), 5000);
        }

        #endregion
    }
}
