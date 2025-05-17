// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Logging;
using osu.Game;
using osu.Game.Configuration;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osu.Game.Screens.Play;
using Velopack;
using Velopack.Sources;

namespace osu.Desktop.Updater
{
    public partial class VelopackUpdateManager : Game.Updater.UpdateManager
    {
        [Resolved]
        private INotificationOverlay notificationOverlay { get; set; } = null!;

        [Resolved]
        private OsuGameBase game { get; set; } = null!;

        [Resolved]
        private ILocalUserPlayInfo? localUserInfo { get; set; }

        [Resolved]
        private OsuConfigManager osuConfigManager { get; set; } = null!;

        private bool isInGameplay => localUserInfo?.PlayingState.Value != LocalUserPlayingState.NotPlaying;

        private readonly Bindable<ReleaseStream> releaseStream = new Bindable<ReleaseStream>();
        private UpdateManager? updateManager;
        private UpdateInfo? pendingUpdate;

        protected override void LoadComplete()
        {
            // Used by the base implementation.
            osuConfigManager.BindWith(OsuSetting.ReleaseStream, releaseStream);
            releaseStream.BindValueChanged(_ => onReleaseStreamChanged(), true);

            base.LoadComplete();
        }

        private void onReleaseStreamChanged()
        {
            updateManager = new UpdateManager(new GithubSource(@"https://github.com/ppy/osu", null, releaseStream.Value == ReleaseStream.Tachyon), new UpdateOptions
            {
                AllowVersionDowngrade = true,
            });

            Schedule(() => Task.Run(CheckForUpdateAsync));
        }

        protected override async Task<bool> PerformUpdateCheck() => await checkForUpdateAsync().ConfigureAwait(false);

        private async Task<bool> checkForUpdateAsync()
        {
            // whether to check again in 30 minutes. generally only if there's an error or no update was found (yet).
            bool scheduleRecheck = false;

            try
            {
                // Avoid any kind of update checking while gameplay is running.
                if (isInGameplay)
                {
                    scheduleRecheck = true;
                    return true;
                }

                // TODO: we should probably be checking if there's a more recent update, rather than shortcutting here.
                // Velopack does support this scenario (see https://github.com/ppy/osu/pull/28743#discussion_r1743495975).
                if (pendingUpdate != null)
                {
                    // If there is an update pending restart, show the notification to restart again.
                    notificationOverlay.Post(new UpdateApplicationCompleteNotification
                    {
                        Activated = () =>
                        {
                            Task.Run(restartToApplyUpdate);
                            return true;
                        }
                    });

                    return true;
                }

                if (updateManager == null)
                {
                    scheduleRecheck = true;
                    return false;
                }

                pendingUpdate = await updateManager.CheckForUpdatesAsync().ConfigureAwait(false);

                // No update is available. We'll check again later.
                if (pendingUpdate == null)
                {
                    scheduleRecheck = true;
                    return false;
                }

                // An update is found, let's notify the user and start downloading it.
                UpdateProgressNotification notification = new UpdateProgressNotification
                {
                    CompletionClickAction = () =>
                    {
                        Task.Run(restartToApplyUpdate);
                        return true;
                    },
                };

                runOutsideOfGameplay(() => notificationOverlay.Post(notification));
                notification.StartDownload();

                try
                {
                    await updateManager.DownloadUpdatesAsync(pendingUpdate, p => notification.Progress = p / 100f).ConfigureAwait(false);
                    runOutsideOfGameplay(() => notification.State = ProgressNotificationState.Completed);
                }
                catch (Exception e)
                {
                    // In the case of an error, a separate notification will be displayed.
                    scheduleRecheck = true;
                    notification.FailDownload();
                    Logger.Error(e, @"update failed!");
                }
            }
            catch (Exception e)
            {
                // we'll ignore this and retry later. can be triggered by no internet connection or thread abortion.
                scheduleRecheck = true;
                Logger.Log($@"update check failed ({e.Message})");
            }
            finally
            {
                if (scheduleRecheck)
                {
                    Scheduler.AddDelayed(() => Task.Run(async () => await checkForUpdateAsync().ConfigureAwait(false)), 60000 * 30);
                }
            }

            return true;
        }

        private void runOutsideOfGameplay(Action action)
        {
            if (isInGameplay)
            {
                Scheduler.AddDelayed(() => runOutsideOfGameplay(action), 1000);
                return;
            }

            action();
        }

        private async Task restartToApplyUpdate()
        {
            if (updateManager == null)
                return;

            await updateManager.WaitExitThenApplyUpdatesAsync(pendingUpdate?.TargetFullRelease).ConfigureAwait(false);
            Schedule(() => game.AttemptExit());
        }
    }
}
