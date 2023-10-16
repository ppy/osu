// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Configuration;
using osu.Framework.Logging;
using osu.Framework.Statistics;
using osu.Game.Beatmaps;
using osu.Game.Configuration;
using osu.Game.Database;
using osu.Game.Models;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Rulesets;
using osu.Game.Skinning;
using Sentry;
using Sentry.Protocol;

namespace osu.Game.Utils
{
    /// <summary>
    /// Report errors to sentry.
    /// </summary>
    public class SentryLogger : IDisposable
    {
        private IBindable<APIUser>? localUser;

        private readonly IDisposable? sentrySession;

        private readonly OsuGame game;

        public SentryLogger(OsuGame game)
        {
            this.game = game;
            sentrySession = SentrySdk.Init(options =>
            {
                // Not setting the dsn will completely disable sentry.
                if (game.IsDeployedBuild && game.CreateEndpoints().WebsiteRootUrl.EndsWith(@".ppy.sh", StringComparison.Ordinal))
                    options.Dsn = "https://ad9f78529cef40ac874afb95a9aca04e@sentry.ppy.sh/2";

                options.AutoSessionTracking = true;
                options.IsEnvironmentUser = false;
                options.IsGlobalModeEnabled = true;
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

                if (wasUnobserved)
                {
                    // see https://github.com/getsentry/sentry-dotnet/blob/c6a660b1affc894441c63df2695a995701671744/src/Sentry/Integrations/TaskUnobservedTaskExceptionIntegration.cs#L39
                    exception.Data[Mechanism.MechanismKey] = @"UnobservedTaskException";
                }

                if (wasUnhandled)
                {
                    // see https://github.com/getsentry/sentry-dotnet/blob/main/src/Sentry/Integrations/AppDomainUnhandledExceptionIntegration.cs#L38-L39
                    exception.Data[Mechanism.MechanismKey] = @"AppDomain.UnhandledException";
                }

                exception.Data[Mechanism.HandledKey] = !wasUnhandled;

                SentrySdk.CaptureEvent(new SentryEvent(exception)
                {
                    Message = entry.Message,
                    Level = getSentryLevel(entry.Level),
                }, scope =>
                {
                    var beatmap = game.Dependencies.Get<IBindable<WorkingBeatmap>>().Value.BeatmapInfo;
                    var ruleset = game.Dependencies.Get<IBindable<RulesetInfo>>().Value;

                    scope.Contexts[@"config"] = new
                    {
                        Game = game.Dependencies.Get<OsuConfigManager>().GetCurrentConfigurationForLogging(),
                        Framework = game.Dependencies.Get<FrameworkConfigManager>().GetCurrentConfigurationForLogging(),
                    };

                    game.Dependencies.Get<RealmAccess>().Run(realm =>
                    {
                        scope.Contexts[@"realm"] = new
                        {
                            Counts = new
                            {
                                BeatmapSets = realm.All<BeatmapSetInfo>().Count(),
                                Beatmaps = realm.All<BeatmapInfo>().Count(),
                                Files = realm.All<RealmFile>().Count(),
                                Rulesets = realm.All<RulesetInfo>().Count(),
                                RulesetsAvailable = realm.All<RulesetInfo>().Count(r => r.Available),
                                Skins = realm.All<SkinInfo>().Count(),
                            }
                        };
                    });

                    scope.Contexts[@"global statistics"] = GlobalStatistics.GetStatistics()
                                                                           .GroupBy(s => s.Group)
                                                                           .ToDictionary(g => g.Key, items => items.ToDictionary(i => i.Name, g => g.DisplayValue));

                    scope.Contexts[@"beatmap"] = new
                    {
                        Name = beatmap.ToString(),
                        Ruleset = beatmap.Ruleset.InstantiationInfo,
                        beatmap.OnlineID,
                    };

                    scope.Contexts[@"ruleset"] = new
                    {
                        ruleset.ShortName,
                        ruleset.Name,
                        ruleset.InstantiationInfo,
                        ruleset.OnlineID
                    };

                    scope.Contexts[@"clocks"] = new
                    {
                        Audio = game.Dependencies.Get<MusicController>().CurrentTrack.CurrentTime,
                        Game = game.Clock.CurrentTime,
                    };

                    scope.SetTag(@"beatmap", $"{beatmap.OnlineID}");
                    scope.SetTag(@"ruleset", ruleset.ShortName);
                    scope.SetTag(@"os", $"{RuntimeInfo.OS} ({Environment.OSVersion})");
                    scope.SetTag(@"processor count", Environment.ProcessorCount.ToString());
                });
            }
            else
                SentrySdk.AddBreadcrumb(entry.Message, entry.Target.ToString(), "navigation", level: getBreadcrumbLevel(entry.Level));
        }

        private BreadcrumbLevel getBreadcrumbLevel(LogLevel entryLevel)
        {
            switch (entryLevel)
            {
                case LogLevel.Debug:
                    return BreadcrumbLevel.Debug;

                case LogLevel.Verbose:
                    return BreadcrumbLevel.Info;

                case LogLevel.Important:
                    return BreadcrumbLevel.Warning;

                case LogLevel.Error:
                    return BreadcrumbLevel.Error;

                default:
                    throw new ArgumentOutOfRangeException(nameof(entryLevel), entryLevel, null);
            }
        }

        private SentryLevel getSentryLevel(LogLevel entryLevel)
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
            sentrySession?.Dispose();
        }

        #endregion
    }
}
