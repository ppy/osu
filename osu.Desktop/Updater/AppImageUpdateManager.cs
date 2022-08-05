// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using AppImage.Update;
using osu.Framework;
using osu.Framework.Allocation;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using UpdateManager = AppImage.Update.Updater;
using State = AppImage.Update.Native.Updater.State;
using LogLevel = AppImage.Update.LogLevel;

namespace osu.Desktop.Updater
{
    /// <summary>
    /// An update manager that shows notifications if a newer release is detected.<para />
    /// Updates the AppImage and backups the previous version with a .zs-old suffix.
    /// </summary>
    [SupportedOSPlatform("linux")]
    public class AppImageUpdateManager : AbstractUpdateManager
    {
#if DEBUG
        private readonly string appPath = $"{RuntimeInfo.StartupDirectory}osu.AppImage";
#else
        private readonly string appPath = UpdateManager.AppImageLocation;
#endif

        private UpdateManager updateManager;
        private INotificationOverlay notificationOverlay;

        private static readonly Logger logger = Logger.GetLogger("updater");

        [Resolved]
        private GameHost host { get; set; }

        /// <summary>
        /// Whether an update has been downloaded but not yet applied.
        /// </summary>
        private bool updatePending;

        [BackgroundDependencyLoader]
        private void load(INotificationOverlay notifications)
        {
            notificationOverlay = notifications;
        }

        protected override async Task<bool> PerformUpdateCheck() => await checkForUpdateAsync().ConfigureAwait(false);

        private bool? hasUpdate;

        private async Task<bool> checkForUpdateAsync(UpdateProgressNotification notification = null)
        {
            // should we schedule a retry on completion of this check?
            bool scheduleRecheck = true;

            try
            {
                updateManager ??= new UpdateManager(new UpdaterOptions
                {
                    AppPath = appPath
                })
                {
                    Logger = new AppImageLogger(),
                    //RawUpdateInformation = "gh-releases-zsync|ppy|osu|2022.709.1|osu.AppImage.zsync"
                };

                hasUpdate ??= await Task.Run(() => updateManager.HasUpdates()).ConfigureAwait(false) ?? false;

                if (hasUpdate.Value)
                {
                    if (updatePending)
                    {
                        // the user may have dismissed the completion notice, so show it again.
                        notificationOverlay.Post(new UpdateCompleteNotification(this));
                        return true;
                    }

                    if (notification == null)
                    {
                        notification = new UpdateProgressNotification(this)
                        {
                            Text = @"Downloading update...",
                            State = ProgressNotificationState.Active
                        };
                        Schedule(() => notificationOverlay.Post(notification));
                    }

                    if (updateManager.Download(p => notification.Progress = Convert.ToSingle(p))
                                     .GetAwaiter().GetResult() != State.SUCCESS)
                    {
                        notification.State = ProgressNotificationState.Cancelled;
                        notification.Close();
                        return false;
                    }

                    notification.Progress = 0;
                    notification.Text = @"Installing update...";

                    bool? success = updateManager.Validate(true);

                    if (success.HasValue && success.Value)
                    {
                        notification.Progress = 1;
                        notification.State = ProgressNotificationState.Completed;
                        updatePending = true;
                    }
                    else
                    {
                        notification.Text = @"Validation failed.";
                        notification.State = ProgressNotificationState.Cancelled;

                        notificationOverlay.Post(new SimpleNotification
                        {
                            Text = "Validation failed.\n\n"
                                   + "Click here to see logs",
                            Icon = FontAwesome.Solid.Upload,
                            Activated = () =>
                            {
                                bool opened() => host.Storage.GetStorageForDirectory("logs").OpenFileExternally(logger.Filename);

                                if (opened())
                                {
                                    // Second time for focus
                                    return opened();
                                }

                                return true;
                            }
                        });

                        notification.Close();
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

        public override Task PrepareUpdateAsync() =>
            Task.Run(() =>
            {
                if (preparedToRestart) return;

                AppDomain.CurrentDomain.ProcessExit += (sender, e) =>
                {
                    using Process process = new Process();

                    process.StartInfo = new ProcessStartInfo
                    {
                        FileName = appPath,
                        UseShellExecute = false
                    };
                    // NOTE: throws an Exception if the debugged AppImage is not made executable
                    process.Start();
                };
                preparedToRestart = true;
            });

        private class AppImageLogger : ILogger
        {
#if DEBUG
            public LogLevel Level { get; set; } = LogLevel.Debug;
#else
            public LogLevel Level { get; set; } = LogLevel.Info;
#endif

            public void Write(string message, LogLevel logLevel)
            {
                logger.Add(message);
            }

            public void Dispose()
            {
            }
        }

        public static bool Available => UpdateManager.IsAppImage;
    }
}
