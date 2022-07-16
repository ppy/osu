// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using osu.Desktop.Linux;
using osu.Framework.Allocation;
using osu.Game;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;

namespace osu.Desktop.Updater
{
    /// <summary>
    /// An update manager that shows notifications if a newer release is detected.<para />
    /// Updates the AppImage and backups the previous version with a .zs-old suffix.
    /// </summary>
    [SupportedOSPlatform("linux")]
    public class AppImageUpdateManager : osu.Game.Updater.UpdateManager
    {
        private INotificationOverlay notificationOverlay;

        /// <summary>
        /// Implements appimageupdatetool functionality via cli
        /// </summary>
        private readonly AppImageUpdateTool appimageupdatetool;

        private string version;

        [BackgroundDependencyLoader]
        private void load(OsuGameBase game, INotificationOverlay notifications)
        {
            notificationOverlay = notifications;
            version = game.Version;
        }

        protected override async Task<bool> PerformUpdateCheck() => await checkForUpdateAsync().ConfigureAwait(false);

        private async Task<bool> checkForUpdateAsync(UpdateProgressNotification notification = null)
        {
            // should we schedule a retry on completion of this check?
            bool scheduleRecheck = true;

            try
            {
                var releases = new OsuJsonWebRequest<osu.Game.Updater.GitHubRelease>("https://api.github.com/repos/ppy/osu/releases/latest");

                await releases.PerformAsync().ConfigureAwait(false);

                var latest = releases.ResponseObject;

                // avoid any discrepancies due to build suffixes for now.
                // eventually we will want to support release streams and consider these.
                string latestTagName = latest.TagName.Split('-').First();

                if (latestTagName != version)
                {
                    if (appimageupdatetool.State == AppImageUpdateTool.States.Completed)
                    {
                        // the user may have dismissed the completion notice, so show it again.
                        notificationOverlay.Post(new ProgressCompleteNotification(this));
                        return true;
                    }

                    if (appimageupdatetool.HasUpdates())
                    {
                        if (notification == null)
                        {
                            notification = new UpdateProgressNotification(this)
                            {
                                Text = @"Downloading update...",
                                State = ProgressNotificationState.Active
                            };
                            Schedule(() => notificationOverlay.Post(notification));
                        }

                        await appimageupdatetool.ApplyUpdateAsync((progress, state) =>
                        {
                            notification.Progress = progress;

                            switch (state)
                            {
                                case AppImageUpdateTool.States.Verifying:
                                    notification.Text = @"Installing update...";
                                    break;

                                case AppImageUpdateTool.States.Completed:
                                    notification.State = ProgressNotificationState.Completed;
                                    break;

                                case AppImageUpdateTool.States.Canceled:
                                    notification.State = ProgressNotificationState.Cancelled;
                                    break;
                            }
                        }).ConfigureAwait((false));
                    }

                    return true;
                }
                else
                {
                    scheduleRecheck = false;
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

        private bool preparedToRestart;

        public AppImageUpdateManager(AppImageUpdateTool appimageupdatetool)
        {
            this.appimageupdatetool = appimageupdatetool;
        }

        public override Task PrepareUpdateAsync() =>
            Task.Run(() =>
            {
                if (!preparedToRestart)
                {
                    AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                    {
                        using (Process process = new Process())
                        {
                            process.StartInfo = new ProcessStartInfo
                            {
                                FileName = AppImageUpdateTool.AppImagePath,
                                UseShellExecute = false
                            };
                            // NOTE: throws an Exception if the debugged AppImage is not made executable
                            process.Start();
                        }
                    };
                    preparedToRestart = true;
                }
            });
    }
}
