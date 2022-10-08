// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;

namespace osu.Game.Utils
{
    /// <summary>
    /// Report errors to sentry.
    /// </summary>
    //不要给上游发日志
    public class SentryLogger : IDisposable
    {
        //private Scope sentryScope;

        public SentryLogger(OsuGame game)
        {
            if (!game.IsDeployedBuild) return;
/*
            var options = new SentryOptions
            {
                // Not setting the dsn will completely disable sentry.
                if (game.IsDeployedBuild && game.CreateEndpoints().WebsiteRootUrl.EndsWith(@".ppy.sh", StringComparison.Ordinal))
                    options.Dsn = "https://ad9f78529cef40ac874afb95a9aca04e@sentry.ppy.sh/2";

                options.AutoSessionTracking = true;
                options.IsEnvironmentUser = false;
                // The reported release needs to match version as reported to Sentry in .github/workflows/sentry-release.yml
                options.Release = $"osu@{game.Version.Replace($@"-{OsuGameBase.BUILD_SUFFIX}", string.Empty)}";
            });

            Logger.NewEntry += processLogEntry;
        }

        ~SentryLogger() => Dispose(false);

        public void AttachUser(IBindable<APIUser> user)
        {
            Debug.Assert(localUser == null);

            localUser = user.GetBoundCopy();
            localUser.BindValueChanged(u =>
            {
                SentrySdk.ConfigureScope(scope => scope.User = new User
                {
                    Username = u.NewValue.Username,
                    Id = u.NewValue.Id.ToString(),
                });
            }, true);
        }

        private void processLogEntry(LogEntry entry)
        {
            if (entry.Level < LogLevel.Verbose) return;

            var exception = entry.Exception;

            if (exception != null)
            {
                if (!shouldSubmitException(exception)) return;

                // framework does some weird exception redirection which means sentry does not see unhandled exceptions using its automatic methods.
                // but all unhandled exceptions still arrive via this pathway. we just need to mark them as unhandled for tagging purposes.
                // easiest solution is to check the message matches what the framework logs this as.
                // see https://github.com/ppy/osu-framework/blob/f932f8df053f0011d755c95ad9a2ed61b94d136b/osu.Framework/Platform/GameHost.cs#L336
                bool wasUnhandled = entry.Message == @"An unhandled error has occurred.";
                bool wasUnobserved = entry.Message == @"An unobserved error has occurred.";

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
