// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Framework.Threading;
using osu.Game;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Play;
using Velopack;
using Velopack.Sources;
using UpdateManager = osu.Game.Updater.UpdateManager;

namespace osu.Desktop.Updater
{
    public partial class VelopackUpdateManager : UpdateManager
    {
        [Resolved]
        private INotificationOverlay notificationOverlay { get; set; } = null!;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        private ILocalUserPlayInfo? localUserInfo { get; set; }

        private bool isInGameplay => localUserInfo?.PlayingState.Value != LocalUserPlayingState.NotPlaying;

        private ScheduledDelegate? scheduledBackgroundCheck;

        private void scheduleNextUpdateCheck()
        {
            scheduledBackgroundCheck?.Cancel();
            scheduledBackgroundCheck = Scheduler.AddDelayed(() =>
            {
                log("Running scheduled background update check...");
                CheckForUpdate();
            }, 60000 * 30);
        }

        protected override async Task<bool> PerformUpdateCheck(CancellationToken cancellationToken)
        {
            scheduledBackgroundCheck?.Cancel();

            if (isInGameplay)
            {
                log("Update check cancelled - user is in gameplay");
                scheduleNextUpdateCheck();
                return false;
            }

            try
            {
                IUpdateSource updateSource = new GithubSource(@"https://github.com/ppy/osu", null, ReleaseStream.Value == Game.Configuration.ReleaseStream.Tachyon);
                Velopack.UpdateManager updateManager = new Velopack.UpdateManager(updateSource, new UpdateOptions
                {
                    AllowVersionDowngrade = true
                });

                UpdateInfo? update = await updateManager.CheckForUpdatesAsync().ConfigureAwait(false);

                if (cancellationToken.IsCancellationRequested)
                {
                    log("Update check cancelled");
                    scheduleNextUpdateCheck();
                    return true;
                }

                if (update == null)
                {
                    // No update is available.
                    log("No update found");
                    scheduleNextUpdateCheck();
                    return false;
                }

                // Download update in the background while notifying awaiters of the update being available.
                log($"New update available: {update.TargetFullRelease.Version}");
                downloadUpdate(updateManager, update, cancellationToken);
                return true;
            }
            catch (Exception e)
            {
                log($"Update check failed with error ({e.Message})");

                // we shouldn't crash on a web failure. or any failure for the matter.
                scheduleNextUpdateCheck();
                return true;
            }
        }

        private void downloadUpdate(Velopack.UpdateManager updateManager, UpdateInfo update, CancellationToken cancellationToken) => Task.Run(async () =>
        {
            log($"Beginning download of update {update.TargetFullRelease.Version}...");

            UpdateDownloadProgressNotification progressNotification = new UpdateDownloadProgressNotification(cancellationToken)
            {
                CompletionClickAction = () =>
                {
                    restartToApplyUpdate(updateManager, update);
                    return true;
                }
            };

            try
            {
                using (var cts = CancellationTokenSource.CreateLinkedTokenSource(progressNotification.CancellationToken, cancellationToken))
                {
                    progressNotification.StartDownload();
                    runOutsideOfGameplay(() => notificationOverlay.Post(progressNotification), cts.Token);

                    await updateManager.DownloadUpdatesAsync(update, p => progressNotification.Progress = p / 100f, cts.Token).ConfigureAwait(false);
                    runOutsideOfGameplay(() => progressNotification.State = ProgressNotificationState.Completed, cts.Token);
                }
            }
            catch (OperationCanceledException)
            {
                progressNotification.FailDownload();
                log(@"Update cancelled");
            }
            catch (Exception e)
            {
                // In the case of an error, a separate notification will be displayed.
                progressNotification.FailDownload();
                Logger.Error(e, @"Update failed!");
            }

            return true;
        }, cancellationToken);

        private void runOutsideOfGameplay(Action action, CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;

            if (isInGameplay)
            {
                Scheduler.AddDelayed(() => runOutsideOfGameplay(action, cancellationToken), 1000);
                return;
            }

            action();
        }

        private void restartToApplyUpdate(Velopack.UpdateManager updateManager, UpdateInfo update) => Task.Run(async () =>
        {
            await updateManager.WaitExitThenApplyUpdatesAsync(update.TargetFullRelease).ConfigureAwait(false);
            Schedule(() => game.AttemptExit());
        });

        private static void log(string text) => Logger.Log($"VelopackUpdateManager: {text}");
    }
}
