// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Logging;
using osu.Game;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Play;
using osu.Game.Updater;

namespace osu.Desktop.Updater
{
    public partial class VeloUpdateManager : UpdateManager
    {
        private Velopack.UpdateManager? updateManager;
        private INotificationOverlay notificationOverlay = null!;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        private ILocalUserPlayInfo? localUserInfo { get; set; }

        [BackgroundDependencyLoader]
        private void load(INotificationOverlay notifications)
        {
            notificationOverlay = notifications;
        }

        protected override async Task<bool> PerformUpdateCheck() => await checkForUpdateAsync().ConfigureAwait(false);

        private async Task<bool> checkForUpdateAsync(UpdateProgressNotification? notification = null)
        {
            // should we schedule a retry on completion of this check?
            bool scheduleRecheck = true;

            const string? github_token = null; // TODO: populate.

            try
            {
                // Avoid any kind of update checking while gameplay is running.
                if (localUserInfo?.IsPlaying.Value == true)
                    return false;

                updateManager ??= new Velopack.UpdateManager(new Velopack.Sources.GithubSource(@"https://github.com/ppy/osu", github_token, false));

                var info = await updateManager.CheckForUpdatesAsync().ConfigureAwait(false);

                if (info == null)
                {
                    // If there is an update pending restart, show the notification again.
                    if (updateManager.IsUpdatePendingRestart)
                    {
                        notificationOverlay.Post(new UpdateApplicationCompleteNotification
                        {
                            Activated = () =>
                            {
                                restartToApplyUpdate();
                                return true;
                            }
                        });
                        return true;
                    }

                    // Otherwise there's no updates available. Bail and retry later.
                    return false;
                }

                scheduleRecheck = false;

                if (notification == null)
                {
                    notification = new UpdateProgressNotification
                    {
                        CompletionClickAction = restartToApplyUpdate,
                    };

                    Schedule(() => notificationOverlay.Post(notification));
                }

                notification.StartDownload();

                try
                {
                    await updateManager.DownloadUpdatesAsync(info, p => notification.Progress = p / 100f).ConfigureAwait(false);

                    notification.StartInstall();

                    notification.State = ProgressNotificationState.Completed;
                }
                catch (Exception e)
                {
                        // In the case of an error, a separate notification will be displayed.
                        notification.FailDownload();
                        Logger.Error(e, @"update failed!");
                }
            }
            catch (Exception)
            {
                // we'll ignore this and retry later. can be triggered by no internet connection or thread abortion.
                scheduleRecheck = true;
            }
            finally
            {
                if (scheduleRecheck)
                {
                    // check again in 30 minutes.
                    Scheduler.AddDelayed(() => Task.Run(async () => await checkForUpdateAsync().ConfigureAwait(false)), 60000 * 30);
                }
            }

            return true;
        }

        private bool restartToApplyUpdate()
        {
            if (updateManager == null)
                return false;

            updateManager.WaitExitThenApplyUpdates(null);
            Schedule(() => game.AttemptExit());
            return true;
        }
    }
}
