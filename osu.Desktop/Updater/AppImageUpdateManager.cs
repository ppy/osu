// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable disable

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Colour;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Game;
using osu.Game.Graphics;
using osu.Game.Online.API;
using osu.Game.Overlays;
using osu.Game.Overlays.Notifications;
using osuTK;
using osuTK.Graphics;

namespace osu.Desktop.Updater
{
    /// <summary>
    /// An update manager that shows notifications if a newer release is detected.<para />
    /// Updates the AppImage and backups the previous version with a .zs-old suffix.
    /// </summary>
    [SupportedOSPlatform("linux")]
    public class AppImageUpdateManager : osu.Game.Updater.UpdateManager
    {
        /// <summary>
        /// Implements appimageupdatetool functionality via cli
        /// </summary>
        private INotificationOverlay notificationOverlay;

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
                string currentTagName = version.Split('-').First();
                string latestTagName = latest.TagName.Split('-').First();

                if (latestTagName != version)
                {
                    if (AppImageUpdateTool.State == AppImageUpdateTool.States.COMPLETED)
                    {
                        // the user may have dismissed the completion notice, so show it again.
                        notificationOverlay.Post(new UpdateCompleteNotification(this));
                        return true;
                    }

                    if (AppImageUpdateTool.HasUpdates)
                    {
                        notificationOverlay.Post(new SimpleNotification
                        {
                            Text = $"A newer release of osu! has been found ({currentTagName} → {latestTagName}).\n\n"
                                   + "Click here to download the new version.",
                            Icon = FontAwesome.Solid.Upload,
                            Activated = () =>
                            {
                                if (notification == null)
                                {
                                    notification = new UpdateProgressNotification(this);
                                    Schedule(() => notificationOverlay.Post(notification));
                                }

                                notification.Text = @"Downloading update...";
                                AppImageUpdateTool.ApplyUpdateAsync((progress, state) =>
                                {
                                    notification.Progress = progress;

                                    switch (state)
                                    {
                                        case AppImageUpdateTool.States.DOWNLOADING:
                                            notification.State = ProgressNotificationState.Active;
                                            break;

                                        case AppImageUpdateTool.States.VERIFYING:
                                            notification.Text = @"Installing update...";
                                            notification.State = ProgressNotificationState.Active;
                                            break;

                                        case AppImageUpdateTool.States.COMPLETED:
                                            notification.State = ProgressNotificationState.Completed;
                                            break;

                                        case AppImageUpdateTool.States.CANCELLED:
                                            notification.State = ProgressNotificationState.Cancelled;
                                            break;
                                    }
                                });
                                return true;
                            }
                        });
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

        /// <inheritdoc cref="AppImageUpdateTool.IsInstalled"/>
        public static bool IsInstalled => AppImageUpdateTool.IsInstalled;

        private class UpdateCompleteNotification : ProgressCompletionNotification
        {
            [Resolved]
            private OsuGame game { get; set; }

            public UpdateCompleteNotification(AppImageUpdateManager updateManager)
            {
                Text = @"Update ready to install. Click to restart!";

                Activated = () =>
                {
                    updateManager.PrepareUpdateAsync()
                                 .ContinueWith(_ => updateManager.Schedule(() => game?.AttemptExit()));
                    return true;
                };
            }
        }

        private class UpdateProgressNotification : ProgressNotification
        {
            private readonly AppImageUpdateManager updateManager;

            public UpdateProgressNotification(AppImageUpdateManager updateManager)
            {
                this.updateManager = updateManager;
            }

            protected override Notification CreateCompletionNotification()
            {
                return new UpdateCompleteNotification(updateManager);
            }

            [BackgroundDependencyLoader]
            private void load(OsuColour colours)
            {
                IconContent.AddRange(new Drawable[]
                {
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = ColourInfo.GradientVertical(colours.YellowDark, colours.Yellow)
                    },
                    new SpriteIcon
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Icon = FontAwesome.Solid.Upload,
                        Colour = Color4.White,
                        Size = new Vector2(20),
                    }
                });
            }

            public override void Close()
            {
                // cancelling updates is not currently supported by the underlying updater.
                // only allow dismissing for now.

                switch (State)
                {
                    case ProgressNotificationState.Cancelled:
                        base.Close();
                        break;
                }
            }
        }
    }
}
