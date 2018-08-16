// Copyright (c) 2007-2018 ppy Pty Ltd <contact@ppy.sh>.
// Licensed under the MIT Licence - https://raw.githubusercontent.com/ppy/osu/master/LICENCE

using System;
using System.Collections.Generic;
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

            Logger.NewEntry += entry =>
            {
                if (entry.Level < LogLevel.Verbose) return;

                if (entry.Exception != null)
                    queuePendingTask(raven.CaptureAsync(new SentryEvent(entry.Exception)));
                else
                    raven.AddTrail(new Breadcrumb(entry.Target.ToString(), BreadcrumbType.Navigation) { Message = entry.Message });
            };
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
